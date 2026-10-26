#!/usr/bin/env python3
"""Black-box tests of the brain (src/ExplorerHat.Brain). They need no robot and no real model:

  * the language model is a fake Ollama (a small HTTP server in this file) that answers what each case needs;
  * the robot is the order service on this PC with --simulado (no pins, no wheels);
  * the brain is the real program, with the keyboard replaced by a pipe.

    python tools/probar-cerebro/probar.py

It builds the two projects in a temporary folder and starts them on free ports. The expected result of every case is
written in the case itself. Nothing here can move a real robot: the service always runs with --simulado.
"""
import json
import socket
import subprocess
import sys
import tempfile
import threading
import time
import urllib.request
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
KEY = "clave-de-prueba"
TOOLS_FILE = ROOT / "tools" / "medir-llm" / "herramientas.json"
ORDERS_FILE = ROOT / "tools" / "medir-llm" / "ordenes.json"

# What the fake model answers to each order: (seconds of delay, tool calls, text)
SCRIPT = {
    "Avanza.": (0, [("avanzar", {"segundos": 1, "velocidad": 0.45})], ""),
    "Sin segundos": (0, [("avanzar", {"velocidad": 0.5})], ""),
    "Luz verde": (0, [("luz", {"color": "verde", "encendida": True})], ""),
    "Pregunta tonta": (0, [], "No puedo hacer eso."),
    "Texto como numero": (0, [("avanzar", {"segundos": "2"})], ""),
    "Dos pasos largos": (0, [("avanzar", {"segundos": 3}), ("girar", {"direccion": "derecha", "segundos": 3})], ""),
    "Lento": (1.5, [("avanzar", {"segundos": 1})], ""),
}


def free_port():
    with socket.socket() as s:
        s.bind(("127.0.0.1", 0))
        return s.getsockname()[1]


class FakeOllama:
    """Records every request of the brain and answers from SCRIPT (nothing for an order that is not in it)"""

    def __init__(self):
        self.requests = []
        self.scripted = True  # False: it answers "no action" to everything
        outer = self

        class Handler(BaseHTTPRequestHandler):
            def do_POST(self):
                body = json.loads(self.rfile.read(int(self.headers["Content-Length"])).decode("utf-8"))
                outer.requests.append(body)
                order = body["messages"][-1]["content"]
                delay, calls, text = SCRIPT.get(order, (0, [], "")) if outer.scripted else (0, [], "")
                time.sleep(delay)
                message = {"role": "assistant", "content": text}
                if calls:
                    message["tool_calls"] = [{"function": {"name": n, "arguments": a}} for n, a in calls]
                data = json.dumps({"model": body["model"], "message": message, "done": True}).encode("utf-8")
                self.send_response(200)
                self.send_header("Content-Type", "application/json")
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)

            def log_message(self, *args):
                pass

        self.port = free_port()
        self.server = ThreadingHTTPServer(("127.0.0.1", self.port), Handler)
        threading.Thread(target=self.server.serve_forever, daemon=True).start()

    @property
    def url(self):
        return f"http://127.0.0.1:{self.port}"

    def orders(self):
        return [r["messages"][-1]["content"] for r in self.requests]


def build(project, output):
    result = subprocess.run(["dotnet", "build", str(ROOT / "src" / project / f"{project}.csproj"), "-o", str(output), "-v", "q", "-nologo"],
                            capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stdout[-2000:], result.stderr[-2000:])
        sys.exit("No compila " + project)


class Pi:
    """The order service on this PC, with --simulado"""

    def __init__(self, folder):
        self.port = free_port()
        self.key_file = folder / "clave-robot"
        self.key_file.write_text(KEY)
        self.process = subprocess.Popen(["dotnet", str(folder / "orders" / "ExplorerHat.Orders.dll"), "--simulado", "--puerto", str(self.port),
                                         "--clave-archivo", str(self.key_file)], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        for _ in range(100):
            try:
                self.get("/estado")
                return
            except Exception:
                time.sleep(0.3)
        sys.exit("El servicio de órdenes no arranca")

    @property
    def url(self):
        return f"http://127.0.0.1:{self.port}"

    def get(self, path):
        request = urllib.request.Request(self.url + path, headers={"X-Clave": KEY})
        with urllib.request.urlopen(request, timeout=10) as response:
            return json.loads(response.read().decode("utf-8"))

    def put_sensors(self, **values):
        data = json.dumps(values).encode("utf-8")
        request = urllib.request.Request(self.url + "/simulado/sensores", data=data, method="PUT", headers={"X-Clave": KEY, "Content-Type": "application/json"})
        urllib.request.urlopen(request, timeout=10).read()

    def stop(self):
        self.process.terminate()


def run_brain(folder, lines, args, timeout=60):
    """lines: [(seconds to wait before, text)]. Returns (exit code, output, seconds)"""
    start = time.time()
    process = subprocess.Popen(["dotnet", str(folder / "brain" / "ExplorerHat.Brain.dll")] + args, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                               stderr=subprocess.STDOUT)
    chunks = []
    reader = threading.Thread(target=lambda: chunks.append(process.stdout.read()), daemon=True)
    reader.start()
    for wait, text in lines:
        time.sleep(wait)
        process.stdin.write(text.encode("utf-8") + b"\n")
        process.stdin.flush()
    process.stdin.close()
    process.wait(timeout=timeout)
    reader.join(5)
    return process.returncode, b"".join(chunks).decode("utf-8"), time.time() - start


class Report:
    def __init__(self):
        self.passed = 0
        self.failed = []

    def check(self, name, condition, detail=""):
        if condition:
            self.passed += 1
            print(f"  ok   {name}")
        else:
            self.failed.append(name)
            print(f"  FALLA {name}\n{detail}")


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    folder = Path(tempfile.mkdtemp(prefix="probar-cerebro-"))
    print("Compilando el cerebro y el servicio de órdenes (la primera vez tarda)...", flush=True)
    build("ExplorerHat.Brain", folder / "brain")
    build("ExplorerHat.Orders", folder / "orders")
    fake, pi, report = FakeOllama(), Pi(folder), Report()
    base = ["--llm", f"{fake.url}=fake", "--robot", pi.url, "--clave-archivo", str(pi.key_file)]
    try:
        print("1. Una acción simple")
        code, out, _ = run_brain(folder, [(0, "Avanza.")], base)
        report.check("avanza y termina por tiempo", "-> avanzar(" in out and "terminó a los" in out and "cancelado" not in out and code == 0, out)
        sent = fake.requests[-1]
        system = json.loads(TOOLS_FILE.read_text(encoding="utf-8"))["sistema"]
        tools = json.loads(TOOLS_FILE.read_text(encoding="utf-8"))["herramientas"]
        report.check("la petición al modelo es la medida: mismo sistema, mismas herramientas, solo la orden, sin keep_alive ni think",
                     sent["model"] == "fake" and sent["messages"][0] == {"role": "system", "content": system}
                     and sent["messages"][1] == {"role": "user", "content": "Avanza."} and sent["tools"] == tools and sent["stream"] is False
                     and sent["options"] == {"temperature": 0, "seed": 1, "num_ctx": 2048} and "keep_alive" not in sent and "think" not in sent,
                     json.dumps(sent)[:600])

        print("2. Falta 'segundos'")
        code, out, _ = run_brain(folder, [(0, "Sin segundos")], base)
        report.check("pone 1,0 s y avanza", "Falta 'segundos': pongo 1,0 s" in out and "terminó a los" in out, out)

        print("3. Una luz")
        code, out, _ = run_brain(folder, [(0, "Luz verde")], base)
        report.check("enciende la luz verde", "-> luz(" in out and "hecho" in out and pi.get("/estado")["luces"]["verde"] is True, out)

        print("4. El modelo contesta con texto y sin acciones")
        code, out, _ = run_brain(folder, [(0, "Pregunta tonta")], base)
        report.check("enseña el texto y no mueve nada", "Modelo: No puedo hacer eso." in out and "no propone ninguna acción" in out and pi.get("/estado")["ocupado"] is False, out)

        print("5. El robot rechaza un número escrito como texto")
        code, out, _ = run_brain(folder, [(0, "Texto como numero")], base)
        report.check("dice 400 y cancela el plan", "(400)" in out and "Plan cancelado" in out, out)

        print("6. Obstáculo delante (simulado a 20 cm)")
        pi.put_sensors(izquierda=400, centro=20, derecha=400)
        code, out, _ = run_brain(folder, [(0, "Avanza.")], base)
        pi.put_sensors(izquierda=400, centro=400, derecha=400)
        report.check("se para por obstáculo y cancela el plan", "se paró por un obstáculo" in out and "Plan cancelado" in out, out)

        print("7. Parada en plena acción (plan de dos acciones de 3 s)")
        code, out, seconds = run_brain(folder, [(0, "Dos pasos largos"), (1.0, "para")], base)
        report.check("'para' corta la primera acción y cancela el plan", "PARADA: para" in out and "Plan cancelado por la parada" in out and "terminó a los" not in out, out)
        report.check("la segunda acción no llega a ejecutarse", "-> girar" not in out, out)
        report.check("y termina mucho antes de los 6 s del plan", seconds < 4.5, f"{seconds:.1f} s\n{out}")
        report.check("el robot queda libre", pi.get("/estado")["ocupado"] is False)

        print("8. Parada mientras el modelo piensa")
        code, out, _ = run_brain(folder, [(0, "Lento"), (0.4, "para")], base)
        report.check("el plan no empieza", "Plan cancelado por la parada" in out and "-> avanzar" not in out, out)

        print("9. Órdenes escritas antes de la parada")
        code, out, _ = run_brain(folder, [(0, "Dos pasos largos"), (0.2, "Avanza."), (0.2, "Avanza."), (0.2, "para")], base)
        report.check("las que esperaban se descartan", out.count("Orden descartada por la parada: Avanza.") == 2, out)

        print("10. Las órdenes de parada de tools/medir-llm/ordenes.json no llegan al modelo")
        orders = json.loads(ORDERS_FILE.read_text(encoding="utf-8"))["ordenes"]
        stop_orders = [o["texto"] for o in orders if any(e["nombre"] == "parar" for e in o["esperado"])]
        other_orders = [o["texto"] for o in orders if o["texto"] not in stop_orders]
        before = len(fake.requests)
        fake.scripted = False  # no order of ordenes.json may start an action: a running action would delay the next line
        # An order that was written before a stop and has not started yet is dropped (that is the rule), so the lines go
        # with enough time between them, and the first one waits for the brain to start
        code, out, _ = run_brain(folder, [((1.5 if i == 0 else 0.4), o["texto"]) for i, o in enumerate(orders)], base, timeout=120)
        fake.scripted = True
        received = fake.orders()[before:]
        missing = [t for t in other_orders if t not in received]
        report.check("hay dos órdenes de parada (la 3 y la 24)", len(stop_orders) == 2, str(stop_orders))
        report.check("el modelo recibe todas las demás, con sus tildes, y ninguna de parada", received == other_orders, f"recibidas {len(received)} de {len(other_orders)}; faltan {missing}\n{out[-1500:]}")
        report.check("el cerebro anuncia dos paradas", out.count("PARADA:") == 2, out[-1500:])

        print("11. Un servidor caído y otro que responde (respaldo)")
        code, out, _ = run_brain(folder, [(0, "Avanza.")], ["--llm", "http://127.0.0.1:1=muerto"] + base)
        report.check("usa el segundo", "no responde" in out and "terminó a los" in out, out)
        code, out, _ = run_brain(folder, [(0, "Avanza."), (1.5, "Luz verde")], ["--llm", "http://127.0.0.1:1=muerto"] + base)
        report.check("y no vuelve a esperar al caído en la orden siguiente", out.count("no responde") == 1 and "-> luz(" in out, out)

        print("12. Ningún servidor responde")
        code, out, _ = run_brain(folder, [(0, "Avanza.")], ["--llm", "http://127.0.0.1:1=muerto", "--robot", pi.url, "--clave-archivo", str(pi.key_file)])
        report.check("no mueve el robot", "Ningún servidor de modelos responde" in out and "-> avanzar" not in out and pi.get("/estado")["ocupado"] is False, out)

        print("13. --solo-mostrar")
        code, out, _ = run_brain(folder, [(0, "Avanza.")], ["--llm", f"{fake.url}=fake", "--solo-mostrar"])
        report.check("enseña el plan sin robot", "-> avanzar(" in out and "terminó" not in out and code == 0, out)

        print("14. Clave equivocada")
        wrong = folder / "clave-mala"
        wrong.write_text("otra")
        code, out, _ = run_brain(folder, [(0, "Avanza.")], ["--llm", f"{fake.url}=fake", "--robot", pi.url, "--clave-archivo", str(wrong)])
        report.check("el robot dice 401 y el plan se cancela", "rechaza la clave (401)" in out, out)

        print("15. Ajustes mal escritos")
        code, out, _ = run_brain(folder, [], ["--robot", pi.url])
        report.check("sin --llm, sale con 2 y enseña el uso", code == 2 and "Falta al menos un --llm" in out and "Uso:" in out, out)
        code, out, _ = run_brain(folder, [], base + ["--clave", "123"])
        report.check("la clave no se acepta en la línea de comandos", code == 2 and "Opción desconocida: --clave" in out, out)
    finally:
        pi.stop()

    print(f"\n{report.passed} comprobaciones bien, {len(report.failed)} mal")
    for name in report.failed:
        print("  FALLA:", name)
    sys.exit(1 if report.failed else 0)


if __name__ == "__main__":
    main()
