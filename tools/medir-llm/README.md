# Medir un modelo de lenguaje local (Fase 7)

Mide cuánto tarda y cuánto acierta un modelo de Ollama al traducir órdenes en español a las acciones del robot. No toca la Pi ni los motores.

| Archivo | Qué es |
| --- | --- |
| `herramientas.json` | Mensaje de sistema, límites y la lista cerrada de acciones (`avanzar`, `retroceder`, `girar`, `luz`, `parar`). Es el contrato que validará el servicio de la Pi. |
| `ordenes.json` | 24 órdenes con su resultado esperado, escritas **antes** de ejecutar ninguna medida. |
| `medir.py` | Envía cada orden a `/api/chat` (sin *streaming*, temperatura 0), puntúa las llamadas y guarda los tiempos. Solo usa la biblioteca estándar de Python. |
| `resultados/` | Un JSON por equipo y modelo, con cada respuesta y cada tiempo. |

## Uso

```bash
python tools/medir-llm/medir.py --modelo qwen2.5:3b --equipo prestige15
```

* Antes, Ollama tiene que estar en marcha y el modelo descargado. Para medir la **carga en frío**, descarga antes el modelo de la memoria (`ollama stop <modelo>`): la primera petición es una de calentamiento y no entra en las estadísticas de tiempo.
* Con un modelo que piensa (Qwen3), añade `--sin-pensar`: envía `think: false`; si no, la respuesta llega en el campo `thinking`.
* Cada orden se repite 3 veces (`--repeticiones`). El portátil debe estar **enchufado**: con batería, la tarjeta gráfica baja su rendimiento.
* Tarda de 2 a 5 minutos con un modelo de 3B y unos 10 con uno de 7B.

## Qué mide

* **Herramienta OK:** las llamadas tienen los mismos nombres que las esperadas, en el mismo orden. Una orden absurda se puntúa bien si el modelo **no** llama a ninguna.
* **Valores OK:** además, los valores cumplen lo esperado (por ejemplo, `segundos` = 2).
* **Válidas:** todas las llamadas siguen el esquema (nombres, argumentos obligatorios, colores existentes, números y no texto) y respetan los límites de segundos y velocidad.
* **Tiempo total** de cada respuesta (mediana y máximo), tokens por segundo y reparto entre tarjeta gráfica y CPU (`/api/ps`).

Los resultados y su lectura están en `PLAN.md`, Fase 7.

## Si la descarga de Ollama falla

En una red que bloquee el servidor de archivos de Ollama (`*.r2.cloudflarestorage.com`), `ollama pull qwen2.5:3b` da `max retries exceeded ... connectex`. Hugging Face suele responder: `ollama pull hf.co/Qwen/Qwen2.5-3B-Instruct-GGUF:Q4_K_M`. Los modelos de Hugging Face usan la plantilla de su GGUF, que puede no ser la de la biblioteca de Ollama: comprueba que la llamada a herramientas funcione antes de medir.
