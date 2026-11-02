#!/bin/bash
# Quick check that both wheels spin, in both directions. Run it before every workshop and after touching
# the motor wires. THE WHEELS MUST BE IN THE AIR. Run it on the Raspberry Pi from a terminal (it asks questions):
#   bash tools/probar-ruedas.sh
# It drives the motor pins with pinctrl (full speed, no PWM), 2 to 3 s per step, and leaves them low when the
# script ends, also with Ctrl+C, tools/parar-robot.sh or a lost SSH session. Every step waits for Enter before the
# wheel moves and asks what you saw afterwards. The LM393 speed sensors only add a hint: the verdict is what you see.
# For all the wiring (lights, sensors, screen), use tools/probar-cableado.sh.
# Motor 1 = right wheel (GPIO 19 forward, 20 backward); motor 2 = left wheel (GPIO 21 forward, 26 backward).

if [ ! -t 0 ]; then
    echo "Este script hace preguntas: ejecútalo en una terminal (bash tools/probar-ruedas.sh)."
    exit 1
fi

if pgrep -f '[E]xplorerHat[.]|[L]esson[0-9]+[.]' > /dev/null; then
    echo "Hay un programa del robot en marcha: páralo antes (bash tools/parar-robot.sh)."
    exit 1
fi

failed=()   # the steps where you answered anything but "s"

motors_off() {
    pinctrl set 19,20,21,26 op dl
}

count_edges() {   # count_edges <seconds>: changes of GPIO 25 (right LM393) and GPIO 18 (left LM393)
    local end=$((SECONDS + $1)) a b x y ca=0 cb=0
    a=$(pinctrl lev 25); b=$(pinctrl lev 18)
    while [ $SECONDS -lt $end ]; do
        x=$(pinctrl lev 25); y=$(pinctrl lev 18)
        if [ "$x" != "$a" ]; then ca=$((ca + 1)); a=$x; fi
        if [ "$y" != "$b" ]; then cb=$((cb + 1)); b=$y; fi
    done
    echo "   Pista (no decide): cambios del sensor, rueda derecha (GPIO 25) = $ca, rueda izquierda (GPIO 18) = $cb"
}

spin() {   # spin <pin high> <pin low> <wheel> <direction>
    read -p "Enter: rueda $3, $4 (GPIO $1 alto, $2 bajo)... "
    pinctrl set "$2" op dl
    pinctrl set "$1" op dh
    count_edges 3          # SECONDS has a resolution of 1 s: the step lasts between 2 and 3 s
    motors_off
    read -p "   ¿Giró la rueda $3 $4? (s/n) " answer
    if [ "$answer" != "s" ]; then
        failed+=("rueda $3 $4")
    fi
}

echo "== Prueba de las ruedas, a toda velocidad. ¡SOLO CON LAS RUEDAS EN EL AIRE!"
read -p "¿Tiene el robot las ruedas en el aire? (s/n) " answer
if [ "$answer" != "s" ]; then
    echo "No se movió nada."
    exit 1
fi

trap motors_off EXIT
trap 'motors_off; exit 1' INT TERM HUP

echo "   Hacia delante = la parte de arriba de la rueda va hacia los sensores de distancia."
echo "   Cada paso espera Enter, dura 2 o 3 s y se para solo. Ctrl+C para todo."
spin 19 20 "DERECHA (MOTOR 1)" "hacia delante"
spin 20 19 "DERECHA (MOTOR 1)" "hacia atrás"
spin 21 26 "IZQUIERDA (MOTOR 2)" "hacia delante"
spin 26 21 "IZQUIERDA (MOTOR 2)" "hacia atrás"

echo "== Resultado"
if [ ${#failed[@]} -eq 0 ]; then
    echo "Bien: las dos ruedas giran en los dos sentidos."
    result=0
else
    echo "Fallaron estos pasos:"
    for step in "${failed[@]}"; do
        echo "  - $step"
    done
    echo "Si una rueda no giró en ningún sentido: toca los cables de su borna (MOTOR 1 = derecha, MOTOR 2 = izquierda) y repite la prueba."
    echo "Si falló solo un sentido: repite la prueba y apunta cuál."
    echo "Si giró la otra rueda, los motores están cambiados de borna. Si una rueda giró al revés, cambia entre sí los dos cables de ese motor (README, montaje)."
    result=1
fi
echo "Estado de los pines (lo = apagado):"
pinctrl get 19,20,21,26 | awk '{printf "%s%s ", $1, $6} END {print ""}'
exit $result
