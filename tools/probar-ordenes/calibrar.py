#!/usr/bin/env python3
"""Runs movements by pulses (centimetros, grados) one at a time against the order service, shows what the wheel
sensors counted and, if you measure the real result, suggests the brake of the service. It MOVES the robot.

    python tools/probar-ordenes/calibrar.py --url http://<ip of the Pi>:5000
    python tools/probar-ordenes/calibrar.py --url http://<ip of the Pi>:5000 --medir      (on the floor: you measure each result)

Before each action it waits for Enter, so you can put the robot on its mark. Ctrl+C stops the robot.
The service must run with --real (the wheels in the air for the first test) or --simulado (to try this script).
The key is read from a file (default ~/.explorerhat-clave), never from the command line.

The brake is how many pulses before its target each wheel stops, because the robot slides after the motors stop. If the
robot goes farther than asked, the brake has to be bigger; if it stops short, smaller. One pulse is 1.02 cm of wheel or
9 degrees of turn. Change the brake when you start the service: --freno-avance, --freno-atras, --freno-giro.
"""
import argparse
import json
import statistics
import sys
import urllib.error
import urllib.request
from pathlib import Path

CM_PER_PULSE = 1.021     # the same numbers as src/ExplorerHat.Orders/Wheels.cs (Geometry)
DEGREES_PER_PULSE = 9.0

# (title, tool, arguments, kind): the default plan. Short moves first.
PLAN = [
    ("avanzar 20 cm", "avanzar", {"centimetros": 20}, "avance"),
    ("avanzar 40 cm", "avanzar", {"centimetros": 40}, "avance"),
    ("avanzar 40 cm", "avanzar", {"centimetros": 40}, "avance"),
    ("avanzar 60 cm", "avanzar", {"centimetros": 60}, "avance"),
    ("retroceder 10 cm", "retroceder", {"centimetros": 10}, "atras"),
    ("retroceder 20 cm", "retroceder", {"centimetros": 20}, "atras"),
    ("girar a la derecha 20 grados", "girar", {"direccion": "derecha", "grados": 20}, "giro"),
    ("girar a la derecha 45 grados", "girar", {"direccion": "derecha", "grados": 45}, "giro"),
    ("girar a la derecha 90 grados", "girar", {"direccion": "derecha", "grados": 90}, "giro"),
    ("girar a la derecha 90 grados", "girar", {"direccion": "derecha", "grados": 90}, "giro"),
    ("girar a la derecha 180 grados", "girar", {"direccion": "derecha", "grados": 180}, "giro"),
    ("girar a la izquierda 90 grados", "girar", {"direccion": "izquierda", "grados": 90}, "giro"),
    ("girar a la izquierda 90 grados", "girar", {"direccion": "izquierda", "grados": 90}, "giro"),
]

TARGET_KEY = {"avance": "centimetros", "atras": "centimetros", "giro": "grados"}
UNIT = {"avance": ("cm", CM_PER_PULSE), "atras": ("cm", CM_PER_PULSE), "giro": ("grados", DEGREES_PER_PULSE)}
BRAKE_KEY = {"avance": "frenoAvance", "atras": "frenoAtras", "giro": "frenoGiro"}
BRAKE_OPTION = {"avance": "--freno-avance", "atras": "--freno-atras", "giro": "--freno-giro"}
GOOD_REASONS = ("distancia", "angulo")


class Service:
    def __init__(self, url, key):
        self.url, self.key = url.rstrip("/"), key

    def call(self, method, path, body=None):
        data = json.dumps(body).encode("utf-8") if body is not None else None
        request = urllib.request.Request(self.url + path, data=data, method=method,
                                         headers={"Content-Type": "application/json", "X-Clave": self.key})
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return response.status, json.loads(response.read().decode("utf-8") or "null")
        except urllib.error.HTTPError as error:
            text = error.read().decode("utf-8")
            try:
                return error.code, json.loads(text)
            except ValueError:
                return error.code, text


def read_number(prompt):
    """A number typed by the user (a comma is accepted), or None if the answer is empty or not a number."""
    text = input(prompt).strip().replace(",", ".")
    try:
        return float(text) if text else None
    except ValueError:
        print("   (no es un número: se salta)")
        return None


def summarize(rows, settings):
    print("\n== Resumen")
    if not rows:
        print("No hay medidas: nada que calibrar. (Usa --medir para escribir lo que mides.)")
        return
    for kind in ("avance", "atras", "giro"):
        measured = [row for row in rows if row["kind"] == kind]
        if not measured:
            continue
        unit, per_pulse = UNIT[kind]
        errors = [row["measured"] - row["target"] for row in measured]
        mean_error = statistics.mean(errors)
        spread = max(errors) - min(errors) if len(errors) > 1 else 0.0
        brake = settings.get(BRAKE_KEY[kind])
        print(f"{kind}: {len(measured)} medidas; error medio {mean_error:+.1f} {unit} "
              f"(mínimo {min(errors):+.1f}, máximo {max(errors):+.1f}); un pulso son {per_pulse:g} {unit}")
        for row in measured:
            print(f"    pedido {row['target']:g} {unit}, medido {row['measured']:g}, el robot contó {row['estimated']:g}")
        if brake is None:
            continue
        change = round(mean_error / per_pulse)
        if change == 0:
            print(f"    El freno actual ({brake}) está bien" + (f", pero la dispersión es de {spread:.1f} {unit}: más de 1 pulso." if spread > per_pulse else "."))
        else:
            word = "sube" if change > 0 else "baja"
            print(f"    Va {'más lejos' if change > 0 else 'más corto'} de lo pedido: {word} el freno de {brake} a {max(0, brake + change)} "
                  f"({BRAKE_OPTION[kind]} {max(0, brake + change)}) y repite este tipo de medida.")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--url", required=True)
    parser.add_argument("--clave-archivo", default=str(Path.home() / ".explorerhat-clave"))
    parser.add_argument("--medir", action="store_true", help="after each action, ask for the real cm or degrees")
    parser.add_argument("--tipo", choices=("avance", "atras", "giro"), help="run only this kind of movement")
    arguments = parser.parse_args()
    sys.stdout.reconfigure(encoding="utf-8")

    key = Path(arguments.clave_archivo).read_text(encoding="utf-8").strip()
    service = Service(arguments.url, key)
    status, state = service.call("GET", "/estado")
    if status != 200:
        print(f"El servicio responde {status}: {state}")
        return 2
    mode = state.get("modo")
    settings = state.get("pulsos", {})
    print(f"Servicio en modo {mode}. Pulsos: {settings}")
    if mode == "velocidadCero":
        print("En --velocidad-cero las ruedas no giran: no hay nada que calibrar. Arranca el servicio con --real.")
        return 2
    if mode == "real":
        print("\n¡LAS RUEDAS SE MUEVEN! Primero con las ruedas en el aire. En el suelo: espacio libre, cargador de la UPS quitado")
        print("y esta terminal a mano. Ctrl+C para parar el robot.\n")

    plan = [step for step in PLAN if arguments.tipo is None or step[3] == arguments.tipo]
    rows = []
    try:
        for number, (title, tool, args, kind) in enumerate(plan, start=1):
            input(f"[{number}/{len(plan)}] {title}. Pon el robot en su marca y pulsa Enter... ")
            status, body = service.call("POST", "/accion", {"nombre": tool, "args": args})
            if status != 200:
                print(f"   El servicio responde {status}: {body}")
                continue
            reason = body.get("motivo")
            pulses = body.get("pulsos", {})
            estimated = body.get("grados", body.get("centimetros"))
            print(f"   motivo: {reason}; {body.get('segundosReales')} s; pulsos derecha {pulses.get('derecha')} / izquierda {pulses.get('izquierda')}; "
                  f"resbalón {body.get('resbalon')}; el robot cree que hizo {estimated} {UNIT[kind][0]}"
                  + (f"; ajustes {body['ajustes']}" if body.get("ajustes") else ""))
            if reason not in GOOD_REASONS:
                print(f"   ¡No terminó bien ({reason})! Esa medida no vale. Si es 'sin_pulsos' o 'ruedas_distintas', mira qué rueda no gira o qué sensor no cuenta.")
                continue
            if arguments.medir:
                measured = read_number(f"   ¿Cuántos {UNIT[kind][0]} midió de verdad? (Enter = no medido) ")
                if measured is not None:
                    rows.append({"kind": kind, "target": args[TARGET_KEY[kind]], "measured": measured, "estimated": estimated})
    except KeyboardInterrupt:
        print("\nCtrl+C: parando el robot.")
    finally:
        try:
            service.call("POST", "/parar")
        except Exception as error:  # the service may be gone (emergency stop): nothing else to do
            print(f"(no pude mandar la parada: {error})")
    summarize(rows, settings)
    return 0


if __name__ == "__main__":
    sys.exit(main())
