# ExplorerHat.Brain: el cerebro del robot (Fase 7)

Programa de consola para el **portátil**. Lee una orden escrita en castellano, le pide a un modelo de lenguaje (Ollama) un plan con la lista cerrada de acciones y manda las acciones, una a una, al [servicio de órdenes](../ExplorerHat.Orders/README.md) de la Pi. No se despliega en la Pi.

```text
teclado --> cerebro (este programa) --HTTP--> Ollama (modelo)     que devuelve las acciones
                      |
                      +--HTTP--> servicio de órdenes de la Pi --> motores y luces
```

**La seguridad está en la Pi, nunca en el cerebro ni en el modelo.** El cerebro solo manda lo que el servicio acepta; la Pi valida, recorta y para por obstáculo.

## Uso

```bash
dotnet run --project src/ExplorerHat.Brain -- \
    --llm http://<ip-del-pc-de-gaming>:11434=qwen2.5:7b \
    --llm http://127.0.0.1:11434=qwen2.5:3b \
    --robot http://<ip-de-la-pi>:5000
```

| Opción | Qué hace |
| --- | --- |
| `--llm <url>=<modelo>` | Servidor de Ollama y modelo. **Se puede repetir**: el primero es el principal y los demás, el respaldo, en orden. Hay que poner al menos uno |
| `--robot <url>` | Servicio de órdenes de la Pi |
| `--clave-archivo <ruta>` | Archivo con la clave del robot. Por defecto, `~/.explorerhat-clave` |
| `--solo-mostrar` | Enseña lo que haría, sin mandar nada al robot (no hace falta `--robot`) |
| `--num-ctx <n>` | Contexto del modelo. Por defecto, 2048, como en las medidas |

* **Ninguna dirección de red está escrita en el código ni en el repositorio**: se pasan al arrancar. Las IP cambian de un día a otro.
* **La clave** se lee de un archivo o de la variable `EXPLORERHAT_CLAVE`, **nunca de la línea de comandos** (quedaría en el historial de la terminal). Copia `~/.explorerhat-clave` de la Pi a `~/.explorerhat-clave` del portátil, sin meterla en el repositorio. El cerebro no la escribe en pantalla.
* **El modelo y su servidor son ajustes**, no constantes: cambiar de modelo no pide cambiar código (la decisión del modelo es provisional: ver `PLAN.md`, Fase 7).
* En Windows, usa `127.0.0.1` y no `localhost` (cada petición pierde unos 2 s).

## Qué hace con cada orden

1. Si el mensaje **empieza por una palabra de parada** (`para`, `parar`, `stop`, `alto`, `detente`, `quieto`, `frena`, `basta`), llama a `POST /parar` **sin preguntar al modelo**, y cancela el plan en curso y las órdenes que esperan.
2. Si no, le pide al modelo un plan. La petición es **la misma que midió `tools/medir-llm/medir.py`**: el mismo mensaje de sistema y las mismas herramientas (el archivo `tools/medir-llm/herramientas.json`, que se copia al compilar), solo la orden como mensaje del usuario, temperatura 0, sin `keep_alive`.
3. Manda las acciones del plan una a una a `POST /accion`. Si falta `segundos` en un movimiento, pone **1 s** y lo dice (el servicio lo exige y el modelo de 3B a veces lo omite). Hace como mucho **12 acciones** por orden.
4. **Para el plan** al primer fallo (400, 401, 409, 503 o sin respuesta) o si una acción termina por `obstaculo`, `parada` o `sensores`, y lo explica en castellano. Lo que la Pi recorta se avisa (`el robot ajustó: segundos`).
5. Si ningún servidor de modelos responde, **el robot no se mueve**. Un servidor que falla se salta durante 30 s, para no esperar 3 s por él en cada orden.

### La parada

Un segundo hilo lee el teclado **siempre**, también mientras el modelo piensa o el robot ejecuta una acción:

* **La regla mira solo la primera palabra**, sin tildes, mayúsculas ni signos: `¡¡PARA YA!! ¡Que se cae por las escaleras!` para (los dos modelos rápidos fallaron esa orden); `Gira para la derecha` no, porque es una orden de movimiento. **Una parada falsa es segura; una parada que no llega, no.** Por ejemplo, `Para la luz roja` también para el robot.
* Primero se anota la parada, después se manda `/parar` y se repite 300 ms más tarde: una acción que ya iba por el camino cuando llegó la primera parada podría empezar después de ella.
* Las órdenes escritas antes de la parada y que aún no empezaron **se descartan**.
* **Ctrl+C** también manda `/parar` antes de salir.

## Pruebas

```bash
python tools/probar-cerebro/probar.py
```

23 comprobaciones de caja negra, **sin robot y sin modelo real**: un Ollama falso, el servicio de órdenes en `--simulado` y el cerebro real con el teclado en una tubería. Cubren: la petición al modelo igual que la medida, `segundos` que falta, luces, texto sin acciones, un número escrito como texto (400), obstáculo, la parada en plena acción y mientras el modelo piensa, las órdenes descartadas, que las dos órdenes de parada de `ordenes.json` no lleguen al modelo, el respaldo, la clave equivocada y los ajustes mal escritos. Con fallos metidos a propósito (una parada que ignora los mensajes largos y un plan que no mira la parada) las pruebas fallan.

## Probado con la Pi

Contra el servicio de la Pi en `--velocidad-cero` (pines reales, ruedas quietas) y en `--real` (ruedas en el aire), el 26/10/2026, con la parada por palabra y con Ctrl+C. **Falta el suelo.** Resultados en `PLAN.md`, Fase 7.

En la consola de Windows, lo que tecleas se puede mezclar con los mensajes del cerebro. Es solo de aspecto: la orden llega entera.
