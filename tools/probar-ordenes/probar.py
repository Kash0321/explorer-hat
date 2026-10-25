#!/usr/bin/env python3
"""Black-box tests of the order service (src/ExplorerHat.Orders). They only talk HTTP, so they run
against the service on the PC (--simulado) and on the Raspberry Pi (--velocidad-cero, real pins).

    python tools/probar-ordenes/probar.py --url http://localhost:5000 --clave <key>

The expected result of every case was written BEFORE the service. The cases marked (sim) need the
simulator endpoint PUT /simulado/sensores and are skipped when the service does not run with --simulado.
Nothing here moves a real robot: the service must run with --simulado or --velocidad-cero.
"""
import argparse
import json
import sys
import threading
import time
import urllib.error
import urllib.request


class Service:
    def __init__(self, url, key):
        self.url, self.key = url.rstrip("/"), key

    def call(self, method, path, body=None, key="default", raw=None):
        """Returns (status, json). key="default" sends the right key, None sends no key."""
        data = raw if raw is not None else (json.dumps(body).encode("utf-8") if body is not None else None)
        headers = {"Content-Type": "application/json"}
        if key == "default":
            key = self.key
        if key is not None:
            headers["X-Clave"] = key
        request = urllib.request.Request(self.url + path, data=data, method=method, headers=headers)
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return response.status, json.loads(response.read().decode("utf-8") or "null")
        except urllib.error.HTTPError as error:
            text = error.read().decode("utf-8")
            try:
                return error.code, json.loads(text)
            except ValueError:
                return error.code, text

    def action(self, name, **args):
        return self.call("POST", "/accion", {"nombre": name, "args": args})

    def background(self, name, **args):
        """Starts an action in another thread. The returned dict gets 'status', 'body' and 'seconds'."""
        result = {}

        def run():
            started = time.time()
            result["status"], result["body"] = self.action(name, **args)
            result["seconds"] = time.time() - started

        thread = threading.Thread(target=run)
        thread.start()
        result["thread"] = thread
        return result

    def state(self):
        status, body = self.call("GET", "/estado")
        assert status == 200, f"/estado gave {status}"
        return body

    def sensors(self, left=400, center=400, right=400, no_data=False):
        status, _ = self.call("PUT", "/simulado/sensores",
                              {"izquierda": left, "centro": center, "derecha": right, "sinDatos": no_data})
        assert status == 200, f"PUT /simulado/sensores gave {status}"


CASES = []


def case(name, simulator=False):
    def register(function):
        CASES.append((name, simulator, function))
        return function
    return register


def check(condition, message):
    if not condition:
        raise AssertionError(message)


def between(value, low, high, what):
    check(low <= value <= high, f"{what} = {value}, expected between {low} and {high}")


def rest(service):
    """Every case starts with the robot stopped and free sensors."""
    service.call("POST", "/parar")
    time.sleep(0.3)  # the filter of the sensors keeps the nearest of the last two readings


# ---- Key ------------------------------------------------------------------------------------------------

@case("01 no key: 401")
def _(s):
    status, _ = s.call("POST", "/accion", {"nombre": "parar", "args": {}}, key=None)
    check(status == 401, f"status {status}, expected 401")


@case("02 wrong key: 401, also for /estado and /parar")
def _(s):
    check(s.call("POST", "/accion", {"nombre": "parar", "args": {}}, key="wrong")[0] == 401, "accion")
    check(s.call("GET", "/estado", key="wrong")[0] == 401, "estado")
    check(s.call("POST", "/parar", key="wrong")[0] == 401, "parar")


# ---- Validation -----------------------------------------------------------------------------------------

@case("03 unknown tool: 400, and the robot does not move")
def _(s):
    status, body = s.action("volar", segundos=1)
    check(status == 400 and "error" in body, f"{status} {body}")
    check(s.state()["ocupado"] is False, "busy after a rejected order")


@case("04 unknown argument: 400")
def _(s):
    check(s.action("avanzar", segundos=1, altura=3)[0] == 400, "extra argument accepted")


@case("05 missing required argument (girar without direccion): 400")
def _(s):
    check(s.action("girar", segundos=1)[0] == 400, "accepted")


@case("06 number as text ('2'): 400")
def _(s):
    check(s.action("avanzar", segundos="2")[0] == 400, "text accepted as number")


@case("07 boolean as text ('true'): 400")
def _(s):
    check(s.action("luz", color="azul", encendida="true")[0] == 400, "text accepted as boolean")


@case("08 color that does not exist ('morada'): 400")
def _(s):
    check(s.action("luz", color="morada", encendida=True)[0] == 400, "accepted")


@case("09 direction that does not exist ('arriba'): 400")
def _(s):
    check(s.action("girar", direccion="arriba", segundos=1)[0] == 400, "accepted")


@case("10 body that is not JSON: 400")
def _(s):
    status, _ = s.call("POST", "/accion", raw=b"esto no es json")
    check(status == 400, f"status {status}")


@case("11 order without name: 400")
def _(s):
    status, _ = s.call("POST", "/accion", {"args": {}})
    check(status == 400, f"status {status}")


# ---- Lights ---------------------------------------------------------------------------------------------

@case("12 light on and off, seen in /estado")
def _(s):
    status, body = s.action("luz", color="azul", encendida=True)
    check(status == 200 and body["estado"] == "ok", f"{status} {body}")
    check(s.state()["luces"]["azul"] is True, "blue light not on")
    check(s.action("luz", color="azul", encendida=False)[0] == 200, "off failed")
    check(s.state()["luces"]["azul"] is False, "blue light not off")


# ---- Movements ------------------------------------------------------------------------------------------

@case("13 avanzar 0.5 s: ends by time in 0.4..0.8 s; motors at 0.45 meanwhile and 0 after")
def _(s):
    run = s.background("avanzar", segundos=0.5)
    time.sleep(0.2)
    state = s.state()
    check(state["ocupado"] is True and state["accion"] == "avanzar", f"not busy: {state}")
    check(state["motores"]["derecha"] == 0.45 and state["motores"]["izquierda"] == 0.45, f"motors {state['motores']}")
    run["thread"].join()
    check(run["status"] == 200 and run["body"]["motivo"] == "tiempo", f"{run['status']} {run['body']}")
    between(run["body"]["segundosReales"], 0.4, 0.8, "segundosReales")
    state = s.state()
    check(state["ocupado"] is False and state["motores"] == {"derecha": 0.0, "izquierda": 0.0}, f"after: {state}")


@case("14 retroceder: both motors negative; girar: opposite signs (One = right wheel)")
def _(s):
    run = s.background("retroceder", segundos=0.4, velocidad=0.5)
    time.sleep(0.15)
    check(s.state()["motores"] == {"derecha": -0.5, "izquierda": -0.5}, "retroceder motors")
    run["thread"].join()
    run = s.background("girar", direccion="derecha", segundos=0.4, velocidad=0.5)
    time.sleep(0.15)
    check(s.state()["motores"] == {"derecha": -0.5, "izquierda": 0.5}, "girar derecha motors")
    run["thread"].join()
    run = s.background("girar", direccion="izquierda", segundos=0.4, velocidad=0.5)
    time.sleep(0.15)
    check(s.state()["motores"] == {"derecha": 0.5, "izquierda": -0.5}, "girar izquierda motors")
    run["thread"].join()


@case("15 avanzar 300 s: cut to 3 s, with 'segundos' in ajustes")
def _(s):
    status, body = s.action("avanzar", segundos=300)
    check(status == 200 and body["motivo"] == "tiempo", f"{status} {body}")
    between(body["segundosReales"], 2.8, 3.4, "segundosReales")
    check("segundos" in body["ajustes"], f"ajustes {body['ajustes']}")


@case("16 retroceder 10 s: cut to 1 s (it goes blind)")
def _(s):
    status, body = s.action("retroceder", segundos=10)
    check(status == 200, f"{status} {body}")
    between(body["segundosReales"], 0.8, 1.4, "segundosReales")
    check("segundos" in body["ajustes"], f"ajustes {body['ajustes']}")


@case("17 velocidad 5 -> 1.0 and segundos -1 -> 0.1, both in ajustes")
def _(s):
    run = s.background("avanzar", segundos=-1, velocidad=5)
    run["thread"].join()
    check(run["status"] == 200, f"{run['status']} {run['body']}")
    check(sorted(run["body"]["ajustes"]) == ["segundos", "velocidad"], f"ajustes {run['body']['ajustes']}")
    between(run["body"]["segundosReales"], 0.05, 0.4, "segundosReales")


@case("18 default speed: avanzar without velocidad uses 0.45 (checked in case 13)")
def _(s):
    status, body = s.action("avanzar", segundos=0.2)
    check(status == 200 and body["ajustes"] == [], f"{status} {body}")


# ---- One action at a time, stop -------------------------------------------------------------------------

@case("19 an action while another runs: 409, and the first one goes on")
def _(s):
    run = s.background("avanzar", segundos=1.0)
    time.sleep(0.3)
    status, body = s.action("luz", color="roja", encendida=True)
    check(status == 409, f"status {status}, expected 409")
    status, _ = s.action("avanzar", segundos=1)
    check(status == 409, f"second movement gave {status}, expected 409")
    run["thread"].join()
    check(run["status"] == 200 and run["body"]["motivo"] == "tiempo", f"first action: {run['body']}")


@case("20 /parar in the middle of avanzar 3 s: the action ends by 'parada' in < 1 s and motors are 0")
def _(s):
    run = s.background("avanzar", segundos=3)
    time.sleep(0.5)
    started = time.time()
    status, body = s.call("POST", "/parar")
    between(time.time() - started, 0, 0.3, "time of /parar")
    check(status == 200 and body["estado"] == "parado", f"{status} {body}")
    run["thread"].join()
    check(run["body"]["motivo"] == "parada", f"motivo {run['body']}")
    between(run["body"]["segundosReales"], 0.3, 1.0, "segundosReales")
    check(s.state()["motores"] == {"derecha": 0.0, "izquierda": 0.0}, "motors not 0")


@case("21 the tool 'parar' in /accion does the same as /parar")
def _(s):
    run = s.background("avanzar", segundos=3)
    time.sleep(0.4)
    status, body = s.action("parar")
    check(status == 200 and body["estado"] == "parado", f"{status} {body}")
    run["thread"].join()
    check(run["body"]["motivo"] == "parada", f"motivo {run['body']}")


@case("22 /parar with nothing running: 200")
def _(s):
    status, body = s.call("POST", "/parar")
    check(status == 200 and body["estado"] == "parado", f"{status} {body}")


@case("23 the service takes a new action right after the last one ends (no queue)")
def _(s):
    check(s.action("avanzar", segundos=0.2)[0] == 200, "first")
    check(s.action("girar", direccion="izquierda", segundos=0.2)[0] == 200, "second")


# ---- Obstacle layer (simulator only) --------------------------------------------------------------------

@case("24 (sim) obstacle already there: avanzar does not move, motivo 'obstaculo', segundosReales < 0.3", True)
def _(s):
    s.sensors(center=20)
    status, body = s.action("avanzar", segundos=2)
    check(status == 200 and body["motivo"] == "obstaculo", f"{status} {body}")
    between(body["segundosReales"], 0, 0.3, "segundosReales")


@case("25 (sim) obstacle that appears at 0.5 s: ends by 'obstaculo' in < 1.2 s", True)
def _(s):
    run = s.background("avanzar", segundos=3)
    time.sleep(0.5)
    s.sensors(left=25)
    run["thread"].join()
    check(run["body"]["motivo"] == "obstaculo", f"motivo {run['body']}")
    between(run["body"]["segundosReales"], 0.4, 1.2, "segundosReales")
    check(s.state()["motores"] == {"derecha": 0.0, "izquierda": 0.0}, "motors not 0")


@case("26 (sim) obstacle at exactly 30 cm does not stop; at 29 cm it does", True)
def _(s):
    s.sensors(center=30)
    status, body = s.action("avanzar", segundos=0.3)
    check(body["motivo"] == "tiempo", f"30 cm: {body}")
    s.sensors(center=29)
    status, body = s.action("avanzar", segundos=0.3)
    check(body["motivo"] == "obstaculo", f"29 cm: {body}")


@case("27 (sim) the obstacle does not block retroceder or girar (the sensors look forward)", True)
def _(s):
    s.sensors(center=10)
    check(s.action("retroceder", segundos=0.2)[1]["motivo"] == "tiempo", "retroceder blocked")
    check(s.action("girar", direccion="derecha", segundos=0.2)[1]["motivo"] == "tiempo", "girar blocked")


@case("28 (sim) sensors without data: avanzar gives 503 and does not move; girar still works", True)
def _(s):
    s.sensors(no_data=True)
    status, body = s.action("avanzar", segundos=0.5)
    check(status == 503 and "error" in body, f"{status} {body}")
    check(s.action("girar", direccion="izquierda", segundos=0.2)[0] == 200, "girar blocked")


@case("29 (sim) /estado shows the distances", True)
def _(s):
    s.sensors(left=111, center=222, right=333)
    time.sleep(0.2)
    check(s.state()["distanciasCm"] == {"izquierda": 111, "centro": 222, "derecha": 333}, f"{s.state()}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--url", default="http://localhost:5000")
    parser.add_argument("--clave", required=True)
    parser.add_argument("--solo", help="run only the cases whose name starts with this text (e.g. 20)")
    arguments = parser.parse_args()
    sys.stdout.reconfigure(encoding="utf-8")

    service = Service(arguments.url, arguments.clave)
    mode = service.state().get("modo")
    print(f"Service mode: {mode}")
    if mode not in ("simulado", "velocidadCero"):
        print("REFUSED: these tests only run against --simulado or --velocidad-cero.")
        return 2

    passed = failed = skipped = 0
    for name, needs_simulator, function in CASES:
        if arguments.solo and not name.startswith(arguments.solo):
            continue
        if needs_simulator and mode != "simulado":
            print(f"SKIP  {name}")
            skipped += 1
            continue
        try:
            if mode == "simulado":
                service.sensors()
            rest(service)
            function(service)
            print(f"ok    {name}")
            passed += 1
        except Exception as error:  # a failed case must not hide the next ones
            print(f"FAIL  {name}\n        {type(error).__name__}: {error}")
            failed += 1
            service.call("POST", "/parar")
    print(f"\n{passed} ok, {failed} failed, {skipped} skipped")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
