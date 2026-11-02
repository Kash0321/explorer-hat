#!/bin/bash
# Sends SIGINT (what Ctrl+C does) to the order service in the middle of a movement by pulses (avanzar 60 cm) and checks that the
# motor pins end low and stay low. A movement by pulses writes the motors every 2 ms: this is the case where a late write could
# turn a motor back on while the emergency stop releases the pins (the bug of 17/10/2026). --velocidad-cero cannot show it,
# because it only ever writes 0.0.
# Run it on the Raspberry Pi, in a SECOND terminal, while the service runs in the first one:
#   bash tools/probar-ordenes/senal-en-movimiento.sh [port, default 5000] [seconds before the signal, default 0.4]
# With --real it MOVES the wheels for less than a second: the wheels in the air first. It ENDS the service: start it again after.
# The three distance sensors must read 30 cm or more (otherwise avanzar stops by itself at once and the test says nothing).
PORT=${1:-5000}
DELAY=${2:-0.4}
URL=http://127.0.0.1:$PORT
KEY=$(tr -d '\r\n' < ~/.explorerhat-clave) || exit 1

STATE=$(curl -s --max-time 3 -H "X-Clave: $KEY" "$URL/estado")
if [ -z "$STATE" ]; then
    echo "El servicio no responde en $URL. ¿Está en marcha?"
    exit 1
fi
read -r MODE FREE <<< "$(echo "$STATE" | python3 -c 'import sys, json; d = json.load(sys.stdin); dist = d.get("distanciasCm"); print(d["modo"], "si" if dist and min(dist.values()) >= 30 else "no")')"
echo "Servicio en modo $MODE. Distancias: $(echo "$STATE" | python3 -c 'import sys, json; print(json.load(sys.stdin).get("distanciasCm"))')"
if [ "$FREE" != "si" ]; then
    echo "Algún sensor de distancia marca menos de 30 cm o no da datos: avanzar se pararía al instante. Despeja el frente."
    exit 1
fi

if [ "$MODE" = "real" ]; then
    if [ ! -t 0 ]; then
        echo "Este script hace una pregunta: ejecútalo en una terminal."
        exit 1
    fi
    echo "¡LAS RUEDAS SE MUEVEN menos de 1 s y el servicio se cierra!"
    read -p "¿Tiene el robot las ruedas en el aire? (s/n) " answer
    if [ "$answer" != "s" ]; then
        echo "No se movió nada."
        exit 1
    fi
fi

# The PID of the service: a dotnet process that runs the order service (looking at /proc, so this command never matches itself)
PID=""
for p in $(pgrep -x dotnet); do
    if tr '\0' ' ' < /proc/$p/cmdline | grep -q "ExplorerHat.Orders.dll"; then
        PID=$p
    fi
done
if [ -z "$PID" ]; then
    echo "No encuentro el proceso del servicio."
    exit 1
fi
echo "PID del servicio: $PID. Mando avanzar 60 cm y SIGINT a los $DELAY s."

curl -s --max-time 6 -H "X-Clave: $KEY" -X POST "$URL/accion" -d '{"nombre":"avanzar","args":{"centimetros":60}}' > ~/senal-respuesta.txt 2>&1 &
CURL=$!
sleep "$DELAY"
if ! kill -INT "$PID"; then
    echo "kill falló: paro el servicio con /parar."
    curl -s --max-time 3 -H "X-Clave: $KEY" -X POST "$URL/parar"
fi

# 15 readings of the pins, one every 0.1 s, from the moment of the signal. The first one or two can still be before the
# service attends the signal; the rest must be low.
HI=0
LAST_HI=0
for i in $(seq 1 15); do
    if pinctrl get 19,20,21,26 | grep -q "| hi"; then
        HI=$((HI + 1))
        LAST_HI=$i
    fi
    sleep 0.1
done
wait $CURL
echo "Lecturas con algún pin en alto: $HI de 15 (la última, la número $LAST_HI)."
echo "Pines ahora: $(pinctrl get 19,20,21,26 | awk '{printf "%s%s ", $1, $6}')"
if [ "$LAST_HI" -le 3 ] && ! pinctrl get 19,20,21,26 | grep -q "| hi"; then
    echo "BIEN: los pines quedaron en lo y no volvieron a subir."
else
    echo "FALLO: un pin siguió o volvió a estar en alto después de la señal. ¡Mira las ruedas y pulsa Ctrl+C en el servicio o ejecuta tools/parar-robot.sh!"
fi
echo "Respuesta de la acción (puede ser un error de conexión o un 500 'Cannot access a disposed object ... HatMotor': es normal, la parada de"
echo "emergencia cerró el hardware mientras el movimiento escribía y el servicio lo capturó; lo importante son los pines): $(head -c 300 ~/senal-respuesta.txt)"
rm -f ~/senal-respuesta.txt
echo "Mira también en la terminal del servicio: debe decir «Parada de emergencia (SIGINT): motores parados». Y tú, ¿pararon las ruedas y se quedaron paradas?"
