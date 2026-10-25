#!/bin/bash
# Tests the obstacle layer of the order service with the real sensors, without moving the wheels.
# The service must be running in the Raspberry Pi with --velocidad-cero (this script refuses any other mode):
#   cd ~/apps/ExplorerHat.Orders && dotnet ExplorerHat.Orders.dll --velocidad-cero
# Then, in another terminal of the Pi:
#   bash tools/probar-obstaculo.sh
# For each sensor: put a hand about 20 cm in front of it, press Enter and keep the hand there until the answer shows.
KEY=$(cat ~/.explorerhat-clave)
URL=http://127.0.0.1:5000

call() { curl -s -m 15 -H "X-Clave: $KEY" "$@"; }

MODE=$(call "$URL/estado" | grep -o '"modo":"[a-zA-Z]*"')
if [ "$MODE" != '"modo":"velocidadCero"' ]; then
  echo "RECHAZADO: el servicio tiene que estar en --velocidad-cero (ahora: ${MODE:-no responde})"
  exit 2
fi

echo "Distancias ahora (cm): $(call "$URL/estado" | grep -o '"distanciasCm":{[^}]*}')"
echo "Sin nada delante a menos de 30 cm, el control debe terminar por 'tiempo'."

# $1 = what to do, $2 = expected reason
step() {
  echo
  read -r -p "$1 Pulsa Enter cuando estés listo: " _
  RESPONSE=$(call -X POST -d '{"nombre":"avanzar","args":{"segundos":3}}' "$URL/accion")
  echo "  Respuesta: $RESPONSE"
  if echo "$RESPONSE" | grep -q "\"motivo\":\"$2\""; then
    echo "  OK (motivo esperado: $2)"
  else
    echo "  FALLO (se esperaba: $2)"
  fi
}

step "CONTROL: quita todo lo que haya a menos de 30 cm delante del robot." tiempo
step "Mano a ~20 cm delante del sensor del CENTRO y no la muevas hasta ver la respuesta." obstaculo
step "Mano a ~20 cm delante del sensor de la IZQUIERDA." obstaculo
step "Mano a ~20 cm delante del sensor de la DERECHA." obstaculo
echo
echo "Distancias al final (cm): $(call "$URL/estado" | grep -o '"distanciasCm":{[^}]*}')"
