#!/bin/bash
# Checks the wiring of the robot after taking it apart: lights, OUT4, speed sensors, the direction of each
# motor (only if you answer "s"), screen and distance sensors. Each step waits for Enter. Run it on the Raspberry Pi
# from a terminal (it needs the keyboard):
#   bash tools/probar-cableado.sh
# The motor step drives the pins with pinctrl (full speed, no PWM) and leaves them low when the script ends,
# also with Ctrl+C or a lost SSH session.
cd "$(dirname "$0")/.." || exit 1

if pgrep -f '[E]xplorerHat[.]|[L]esson[0-9]+[.]' > /dev/null; then
    echo "Hay un programa del robot en marcha: páralo antes (bash tools/parar-robot.sh)."
    exit 1
fi

light() {   # light <gpio> <name>
    read -p "Enter: se enciende 2 s $2 (GPIO $1)... "
    pinctrl set "$1" op dh
    sleep 2
    pinctrl set "$1" op dl
}

count_edges() {   # count_edges <seconds>: changes of GPIO 25 (right LM393) and GPIO 18 (left LM393)
    local end=$((SECONDS + $1)) a b x y ca=0 cb=0
    a=$(pinctrl lev 25); b=$(pinctrl lev 18)
    while [ $SECONDS -lt $end ]; do
        x=$(pinctrl lev 25); y=$(pinctrl lev 18)
        if [ "$x" != "$a" ]; then ca=$((ca + 1)); a=$x; fi
        if [ "$y" != "$b" ]; then cb=$((cb + 1)); b=$y; fi
    done
    echo "  Cambios: rueda derecha (IN4, GPIO 25) = $ca, rueda izquierda (GPIO 18) = $cb"
}

motors_off() {
    pinctrl set 19,20,21,26 op dl
}

motor() {   # motor <pin high> <pin low> <name> <expected>
    read -p "Enter: $3 en marcha (GPIO $1 alto, $2 bajo). Esperado: $4... "
    pinctrl set "$2" op dl
    pinctrl set "$1" op dh
    read -p "  Enter para PARAR... "
    motors_off
    sleep 0.5
}

echo "== 1. Luces del HAT y LED de la lección 05 (OUT4)"
light 4 "la luz AZUL"
light 17 "la luz AMARILLA"
light 27 "la luz ROJA"
light 5 "la luz VERDE"
light 16 "el LED de la protoboard (OUT4; si no está conectado, no se verá nada)"

echo "== 2. Sensores de velocidad LM393 (solo debe contar el de la rueda que giras)"
read -p "Enter y gira despacio con la mano SOLO la rueda DERECHA durante 10 s... "
count_edges 10
read -p "Enter y gira despacio con la mano SOLO la rueda IZQUIERDA durante 10 s... "
count_edges 10

echo "== 3. Motores, a toda velocidad. ¡SOLO CON LAS RUEDAS EN EL AIRE!"
read -p "¿Tiene el robot las ruedas en el aire? (s/n) " answer
if [ "$answer" = "s" ]; then
    trap motors_off EXIT
    trap 'motors_off; exit 1' INT TERM HUP
    echo "   Hacia delante = la parte de arriba de la rueda va hacia los sensores."
    motor 19 20 "MOTOR 1" "la rueda DERECHA hacia delante, la izquierda quieta"
    motor 21 26 "MOTOR 2" "la rueda IZQUIERDA hacia delante, la derecha quieta"
    echo "   Si gira la otra rueda, los motores están cambiados de borna."
    echo "   Si una rueda gira hacia atrás, cambia entre sí los dos cables de ese motor (README, montaje)."
fi

echo "== 4. Pantalla LCD (lección 12, ~35 s; en la parte 3 acerca la mano al sensor CENTRAL)"
read -p "Enter para empezar... "
dotnet run --project lessons/Lesson12.Screen/Lesson12.Screen.csproj

echo "== 5. Sensores de distancia: pon la mano delante de cada uno y mira qué fila cambia"
echo "   Ctrl+C para terminar."
read -p "Enter para abrir el panel... "
dotnet run --project src/ExplorerHat.SonarDashboard/ExplorerHat.SonarDashboard.csproj

echo "== Fin. Estado de los pines (lo = apagado):"
pinctrl get 19,20,21,26,4,17,27,5,16 | awk '{printf "%s%s ", $1, $6} END {print ""}'
