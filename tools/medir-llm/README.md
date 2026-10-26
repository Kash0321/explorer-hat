# Medir un modelo de lenguaje local (Fase 7)

Mide cuánto tarda y cuánto acierta un modelo de Ollama al traducir órdenes en español a las acciones del robot. No toca la Pi ni los motores.

| Archivo | Qué es |
| --- | --- |
| `herramientas.json` | Mensaje de sistema, límites y la lista cerrada de acciones (`avanzar`, `retroceder`, `girar`, `luz`, `parar`). Es el contrato que validará el servicio de la Pi. |
| `ordenes.json` | 24 órdenes con su resultado esperado, escritas **antes** de ejecutar ninguna medida. |
| `medir.py` | Envía cada orden a `/api/chat` (sin *streaming*, temperatura 0), puntúa las llamadas y guarda los tiempos. Solo usa la biblioteca estándar de Python. |
| `resultados/` | Un JSON por equipo y modelo, con cada respuesta y cada tiempo. `resultados/historial-huggingface/` guarda la primera tanda (25/10/2026), hecha con modelos de Hugging Face: sirve de historial, **no** para comparar modelos. |

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

## Qué modelos usar y qué hacer si la descarga falla

Usa los modelos de la **biblioteca de Ollama** (`ollama pull qwen2.5:3b`). El 25/10/2026 una red bloqueaba el servidor de archivos de Ollama (`*.r2.cloudflarestorage.com`) y `ollama pull` daba `max retries exceeded ... connectex`; el 26/10/2026 la descarga funcionó. Si vuelve a fallar, Hugging Face suele responder (`ollama pull hf.co/Qwen/Qwen2.5-3B-Instruct-GGUF:Q4_K_M`), pero esos modelos usan la plantilla de su GGUF, que no es la de la biblioteca: **cambia los resultados** (con la de Hugging Face, `qwen2.5:3b` llamaba a `avanzar` ante `Para.`). Si mides uno de Hugging Face, no lo compares con los oficiales.

Para medir un modelo que está en otro equipo (por ejemplo, un PC de gaming con más tarjeta gráfica), usa `--url http://<ip>:11434` y un `--equipo` distinto.

## Varios clientes a la vez (`--paralelo N`)

`--paralelo 6` simula un taller con 6 portátiles: 6 clientes envían a la vez las 24 órdenes (cada uno empieza por una orden distinta) y el resultado va a `<equipo>_<modelo>_paralelo6.json`, con el campo `cliente` en cada ejecución y `segundosDePared` y `peticionesPorSegundo` en el resumen. Es la peor carga: los niños no escriben una orden cada segundo. Los tiempos incluyen la espera en la cola. El servidor atiende a la vez tantas peticiones como diga `OLLAMA_NUM_PARALLEL`; las demás esperan. Compara siempre con la medida de un solo cliente, del mismo modelo y del mismo equipo.

En Windows, mide un Ollama local con `--url http://127.0.0.1:11434`: con `localhost` cada petición pierde unos 2 s y distorsiona `segundosDePared` (no los tiempos de `totalSegundos`, que los mide Ollama).
