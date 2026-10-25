# ExplorerHat.Orders: servicio de órdenes (Fase 7)

La Pi es el cuerpo del robot. Este servicio recibe **una acción cada vez** por HTTP desde el cerebro (un modelo de lenguaje en un portátil, o `curl` a mano), la comprueba contra una lista cerrada y la ejecuta. **La seguridad está aquí, nunca en el modelo.**

## Modos de arranque (hay que elegir uno)

| Opción | Pines | Las ruedas |
| --- | --- | --- |
| `--simulado` | ninguno | no existen. Para probar en el PC |
| `--velocidad-cero` | reales | **no giran nunca** (los pines reciben 0.0; las luces sí funcionan) |
| `--real` | reales | giran. **Primero, con las ruedas en el aire** |

Más opciones: `--puerto 5000` y `--clave-archivo <ruta>`.

```bash
dotnet ExplorerHat.Orders.dll --velocidad-cero        # en la Pi, desde ~/apps/ExplorerHat.Orders
```

## La clave

Todas las peticiones llevan la cabecera `X-Clave`. La clave está en `~/.explorerhat-clave` (se crea con un valor al azar la primera vez, y solo la puede leer el usuario). Sin la clave, o con otra, la respuesta es 401: nadie más en la red del aula puede mover el robot.

## Rutas

| Ruta | Qué hace |
| --- | --- |
| `POST /accion` | `{"nombre": "avanzar", "args": {"segundos": 2}}`. Espera a que termine y responde |
| `POST /parar` | Para los motores al momento. Es lo mismo que la acción `parar` |
| `GET /estado` | Modo, si está ocupado, velocidad de las ruedas, luces y distancias |
| `PUT /simulado/sensores` | Solo con `--simulado`: fija las distancias de las pruebas |

Acciones (la misma lista cerrada que `tools/medir-llm/herramientas.json`): `avanzar(segundos, velocidad?)`, `retroceder(segundos, velocidad?)`, `girar(direccion, segundos, velocidad?)`, `luz(color, encendida)` y `parar()`.

## Reglas de seguridad

* **Una acción cada vez y sin cola.** Si llega otra mientras una corre, responde 409. Así, si se corta la wifi, el robot sigue como mucho lo que le quede a la acción en curso (3 s) y se para solo.
* **Duración máxima:** 3 s en `avanzar` y `girar`, **1 s en `retroceder`** (va a ciegas: los sensores miran hacia delante). Mínimo 0,1 s. Velocidad de 0 a 1 (por defecto 0,45). Lo que se pasa de los límites se recorta y la respuesta lo dice en `ajustes`.
* **Se rechaza (400)** lo que no está en la lista: acciones, argumentos o colores desconocidos, y números o booleanos escritos como texto.
* **Parada por obstáculo:** al avanzar, si algún sensor mide menos de 30 cm, para (`motivo: obstaculo`). Si los sensores no dan datos, no avanza (503) y, si se quedan sin datos a mitad, para (`motivo: sensores`). Un sensor desenchufado no da eco, y eso se lee como "no hay nada delante" (400 cm): el mismo límite que la lección 09.
* **Un solo hilo escribe en los motores**, con un `lock`. `/parar` cancela la acción y pone los motores a 0 bajo ese `lock`.
* Las señales (Ctrl+C, `kill`, SSH perdido) las atiende `SafeExplorerHat`, como en las lecciones.

## Pruebas

`tools/probar-ordenes/probar.py` hace pruebas de caja negra por HTTP: 29 casos con el resultado esperado escrito antes del servicio. Rechaza ejecutarse contra un servicio en modo `--real`.

```bash
dotnet run --project src/ExplorerHat.Orders -- --simulado --puerto 5099 --clave-archivo <ruta>
python tools/probar-ordenes/probar.py --url http://127.0.0.1:5099 --clave <clave>
```

Usa `127.0.0.1` y no `localhost`: en Windows, `localhost` prueba primero IPv6 y cada petición tarda 2 s de más, lo que falsea las pruebas de tiempo.
