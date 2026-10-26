#!/usr/bin/env python3
"""Measure a local LLM (Ollama) on the robot commands in ordenes.json.

    python tools/medir-llm/medir.py --modelo qwen2.5:3b --equipo prestige15

Uses only the Python standard library. For each command it sends the system prompt and the
tools of herramientas.json to /api/chat (no streaming, temperature 0), checks the tool calls
against the expected ones and records the times that Ollama reports. The first request loads
the model, so it is a separate warm-up and it is not part of the timing statistics.
The result goes to tools/medir-llm/resultados/<equipo>_<modelo>.json.
With --paralelo N, N clients send all the orders at the same time (a workshop with N laptops) and the
result goes to <equipo>_<modelo>_paraleloN.json. The server decides how many requests it runs together
(OLLAMA_NUM_PARALLEL); the rest wait in a queue, and that wait is part of the times.
"""
import argparse
import json
import statistics
import sys
import time
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

HERE = Path(__file__).parent
TOLERANCE = 0.05  # numbers are compared with this margin (0.3 and 0.30000001 are the same)


def post(url, body):
    request = urllib.request.Request(url, data=json.dumps(body).encode("utf-8"),
                                     headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(request, timeout=600) as response:
        return json.loads(response.read().decode("utf-8"))


def get(url):
    with urllib.request.urlopen(url, timeout=30) as response:
        return json.loads(response.read().decode("utf-8"))


def matches(expected, actual):
    """True if one actual value fits the matcher: exact value or {min, max}."""
    if isinstance(expected, dict):
        return isinstance(actual, (int, float)) and not isinstance(actual, bool) \
            and expected["min"] - TOLERANCE <= actual <= expected["max"] + TOLERANCE
    if isinstance(expected, bool) or isinstance(actual, bool):
        return expected is actual
    if isinstance(expected, (int, float)):
        return isinstance(actual, (int, float)) and abs(expected - actual) <= TOLERANCE
    return expected == actual


def call_matches(expected, call):
    if expected["nombre"] != call["nombre"]:
        return False
    return all(key in call["args"] and matches(value, call["args"][key])
               for key, value in expected.get("args", {}).items())


def score_tools(order, calls):
    """Same tool names as expected (same order unless sinOrden)."""
    expected = [e["nombre"] for e in order["esperado"]]
    actual = [c["nombre"] for c in calls]
    return sorted(expected) == sorted(actual) if order.get("sinOrden") else expected == actual


def score_values(order, calls):
    if not score_tools(order, calls):
        return False
    if order.get("sinOrden"):
        pending = list(calls)
        for e in order["esperado"]:
            found = next((c for c in pending if call_matches(e, c)), None)
            if found is None:
                return False
            pending.remove(found)
        return True
    return all(call_matches(e, c) for e, c in zip(order["esperado"], calls))


def score_valid(tools, limits, calls):
    """Safety check: every call follows the schema and stays inside the limits."""
    by_name = {t["function"]["name"]: t["function"]["parameters"] for t in tools}
    for call in calls:
        schema = by_name.get(call["nombre"])
        if schema is None:
            return False
        args = call["args"]
        if any(r not in args for r in schema.get("required", [])):
            return False
        for key, value in args.items():
            prop = schema["properties"].get(key)
            if prop is None:
                return False
            if "enum" in prop and value not in prop["enum"]:
                return False
            if prop.get("type") == "boolean" and not isinstance(value, bool):
                return False
            if prop.get("type") == "number":
                if not isinstance(value, (int, float)) or isinstance(value, bool):
                    return False
                if key in limits and not limits[key]["min"] <= value <= limits[key]["max"]:
                    return False
    return True


def extract_calls(message):
    calls = []
    for call in message.get("tool_calls") or []:
        function = call["function"]
        calls.append({"nombre": function["name"], "args": function.get("arguments") or {}})
    return calls


def chat(url, model, system, tools, text, options, keep_alive, think=None):
    body = {
        "model": model, "stream": False, "keep_alive": keep_alive, "tools": tools, "options": options,
        "messages": [{"role": "system", "content": system}, {"role": "user", "content": text}],
    }
    if think is not None:
        body["think"] = think  # models that think (Qwen3) put their answer in "thinking" unless it is false
    return post(url + "/api/chat", body)


def seconds(nanoseconds):
    return nanoseconds / 1e9


def summarize(values):
    return {"mediana": round(statistics.median(values), 3), "maximo": round(max(values), 3)} if values else {}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--modelo", required=True)
    parser.add_argument("--equipo", default="equipo")
    parser.add_argument("--repeticiones", type=int, default=3)
    parser.add_argument("--url", default="http://localhost:11434")
    parser.add_argument("--num-ctx", type=int, default=2048)
    parser.add_argument("--sin-pensar", action="store_true", help="send think=false (for models that think)")
    parser.add_argument("--paralelo", type=int, default=1,
                        help="number of clients that send all the orders at the same time (default 1)")
    arguments = parser.parse_args()
    sys.stdout.reconfigure(encoding="utf-8")

    config = json.loads((HERE / "herramientas.json").read_text(encoding="utf-8"))
    orders = json.loads((HERE / "ordenes.json").read_text(encoding="utf-8"))["ordenes"]
    tools, system, limits = config["herramientas"], config["sistema"], config["limites"]
    options = {"temperature": 0, "seed": 1, "num_ctx": arguments.num_ctx}
    think = False if arguments.sin_pensar else None

    print(f"Warm-up de {arguments.modelo} (carga el modelo)...", flush=True)
    warm = chat(arguments.url, arguments.modelo, system, tools, "Para.", options, "30m", think)
    load_seconds = seconds(warm.get("load_duration", 0))
    processes = [p for p in get(arguments.url + "/api/ps").get("models", []) if p["name"].startswith(arguments.modelo)]
    placement = {}
    if processes:
        size, vram = processes[0]["size"], processes[0]["size_vram"]
        placement = {"bytes": size, "bytesVram": vram, "porcentajeGpu": round(100 * vram / size) if size else 0}

    def run_order(order, show):
        runs = []
        for _ in range(arguments.repeticiones):
            reply = chat(arguments.url, arguments.modelo, system, tools, order["texto"], options, "30m", think)
            message = reply["message"]
            calls = extract_calls(message)
            runs.append({
                "llamadas": calls,
                "texto": message.get("content", ""),
                "herramientasOk": score_tools(order, calls),
                "valoresOk": score_values(order, calls),
                "validas": score_valid(tools, limits, calls),
                "totalSegundos": round(seconds(reply["total_duration"]), 3),
                "evalTokens": reply.get("eval_count", 0),
                "evalSegundos": round(seconds(reply.get("eval_duration", 0)), 3),
                "promptTokens": reply.get("prompt_eval_count", 0),
            })
        if show:
            first = runs[0]
            print(f'{order["id"]:>2} {order["tipo"]:<20} herr={"OK" if first["herramientasOk"] else "--"} '
                  f'val={"OK" if first["valoresOk"] else "--"} valid={"OK" if first["validas"] else "--"} '
                  f'{first["totalSegundos"]:>6.2f} s  {order["texto"]}', flush=True)
        return runs

    def run_client(client):
        # Each client sends all the orders. The list is rotated so that the clients do not send the same order at the same time.
        shift = client % len(orders)
        return [(order, run_order(order, parallel == 1)) for order in orders[shift:] + orders[:shift]]

    parallel = max(1, arguments.paralelo)
    if parallel > 1:
        print(f"{parallel} clientes a la vez, {len(orders) * arguments.repeticiones} peticiones cada uno...", flush=True)
    runs_by_order = {order["id"]: [] for order in orders}
    started = time.monotonic()
    with ThreadPoolExecutor(max_workers=parallel) as pool:
        for client, client_runs in enumerate(pool.map(run_client, range(parallel))):
            for order, runs in client_runs:
                for run in runs:
                    if parallel > 1:
                        run["cliente"] = client
                runs_by_order[order["id"]].extend(runs)
    wall_seconds = time.monotonic() - started
    results = [{"id": o["id"], "tipo": o["tipo"], "texto": o["texto"], "ejecuciones": runs_by_order[o["id"]]} for o in orders]

    all_runs = [run for r in results for run in r["ejecuciones"]]
    total = len(all_runs)
    summary = {
        "paralelo": parallel,
        "segundosDePared": round(wall_seconds, 1),
        "peticionesPorSegundo": round(total / wall_seconds, 2),
        "ejecuciones": total,
        "herramientasOk": sum(r["herramientasOk"] for r in all_runs),
        "valoresOk": sum(r["valoresOk"] for r in all_runs),
        "validas": sum(r["validas"] for r in all_runs),
        "totalSegundos": summarize([r["totalSegundos"] for r in all_runs]),
        "tokensPorSegundo": summarize([r["evalTokens"] / r["evalSegundos"] for r in all_runs if r["evalSegundos"] > 0]),
    }
    output = {"modelo": arguments.modelo, "equipo": arguments.equipo, "ollama": get(arguments.url + "/api/version").get("version"),
              "opciones": {**options, "think": think}, "cargaSegundos": round(load_seconds, 3), "reparto": placement,
              "resumen": summary, "ordenes": results}

    folder = HERE / "resultados"
    folder.mkdir(exist_ok=True)
    safe_name = arguments.modelo.replace("hf.co/", "").replace(":", "-").replace("/", "-")
    suffix = f"_paralelo{parallel}" if parallel > 1 else ""
    path = folder / f"{arguments.equipo}_{safe_name}{suffix}.json"
    path.write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\nCarga {load_seconds:.1f} s; reparto {placement}")
    print(f"{parallel} cliente(s): {wall_seconds:.0f} s en total, {total / wall_seconds:.2f} peticiones/s")
    print(f"Herramienta OK {summary['herramientasOk']}/{total}; valores OK {summary['valoresOk']}/{total}; "
          f"válidas {summary['validas']}/{total}")
    print(f"Tiempo total (s) {summary['totalSegundos']}; tokens/s {summary['tokensPorSegundo']}")
    print(f"Guardado en {path}")


if __name__ == "__main__":
    main()
