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
| `PUT /simulado/ruedas` | Solo con `--simulado`: `{"derechaParada": true}` hace que una rueda no cuente pulsos (prueba `sin_pulsos`) |

Acciones: `avanzar`, `retroceder`, `girar(direccion)`, `luz(color, encendida)` y `parar()`. Un movimiento lleva **`segundos` o una medida en pulsos, nunca los dos**: `centimetros` en `avanzar` y `retroceder`, `grados` en `girar` (400 si vienen los dos o ninguno). Además, `velocidad?` (0 a 1). **`tools/medir-llm/herramientas.json` (lo que ve el modelo) todavía solo ofrece `segundos`**: `centimetros` y `grados` los usa el cerebro cuando se actualice el contrato (Fase 7, etapa 1 de la autonomía).

## Movimientos por pulsos (`centimetros` y `grados`)

El robot cuenta los pulsos de los sensores de rueda LM393 (el derecho en GPIO 25 y el izquierdo en GPIO 18) con el método de la lección 10: un hilo lee los dos pines cada milisegundo mientras el robot se mueve y cuenta los cambios de Low a High. Cada disco tiene 20 ranuras: **un pulso son 1,02 cm de rueda** y, al girar sobre sí mismo con las ruedas a 13 cm, **un pulso son 9°**.

* **`avanzar`** (de 5 a 60 cm): las dos ruedas van a la vez y el servicio corrige la velocidad de cada una según los pulsos de diferencia (como la lección 10, parte 2). **`retroceder`** (de 5 a 20 cm) y **`girar`** (de 20 a 360°): cada rueda se para sola al llegar a sus pulsos.
* **El freno:** el robot resbala un poco después de parar los motores, así que cada rueda se para unos pulsos **antes** del objetivo. Los valores iniciales (6 al avanzar, 4 al retroceder y 4 al girar) son los de la lección 10 y **se calibran en el suelo**; se cambian al arrancar el servicio, sin compilar: `--freno-avance`, `--freno-atras` y `--freno-giro`.
* **La velocidad es fija** (`--velocidad-pulsos`, 0,6 por defecto): el freno está calibrado para una sola velocidad. Si el cerebro pide otra, no se usa y la respuesta lo dice en `ajustes`.
* **Los tiempos máximos no cambian:** 3 s (1 s hacia atrás). Si no llega, termina con `tiempo_agotado`. Los límites en cm y grados son iniciales y se revisan con las medidas del suelo.
* **Motivos de fin de un movimiento por pulsos:** `distancia` o `angulo` (llegó); `tiempo_agotado`, `sin_pulsos` (una rueda que se mueve no cuenta ningún pulso en 0,7 s: no gira o su sensor no cuenta), `ruedas_distintas` (al avanzar, más de 10 pulsos de diferencia entre las ruedas), `obstaculo`, `sensores` y `parada`. **El motivo `tiempo` solo es de los movimientos por tiempo**, así no se confunde con haber llegado.
* **La respuesta trae lo que pasó de verdad:** `pulsos` de cada rueda (también en los movimientos por tiempo), y en los de pulsos `resbalon` (pulsos contados en los 0,3 s siguientes a parar) y `centimetros` o `grados` reales. Sirven para calibrar el freno.

## Reglas de seguridad

* **Una acción cada vez y sin cola.** Si llega otra mientras una corre, responde 409. Así, si se corta la wifi, el robot sigue como mucho lo que le quede a la acción en curso (3 s) y se para solo.
* **Duración máxima:** 3 s en `avanzar` y `girar`, **1 s en `retroceder`** (va a ciegas: los sensores miran hacia delante). Mínimo 0,1 s. Velocidad de 0 a 1 (por defecto 0,45). Lo que se pasa de los límites se recorta y la respuesta lo dice en `ajustes`.
* **Se rechaza (400)** lo que no está en la lista: acciones, argumentos o colores desconocidos, y números o booleanos escritos como texto.
* **Parada por obstáculo:** al avanzar, si algún sensor mide menos de 30 cm, para (`motivo: obstaculo`). Si los sensores no dan datos, no avanza (503) y, si se quedan sin datos a mitad, para (`motivo: sensores`). Un sensor desenchufado no da eco, y eso se lee como "no hay nada delante" (400 cm): el mismo límite que la lección 09.
* **Un solo hilo escribe en los motores**, con un `lock`. `/parar` cancela la acción y pone los motores a 0 bajo ese `lock`.
* Las señales (Ctrl+C, `kill`, SSH perdido) las atiende `SafeExplorerHat`, como en las lecciones.

## Pruebas

`tools/probar-ordenes/probar.py` hace pruebas de caja negra por HTTP: 47 casos con el resultado esperado escrito antes del servicio (los 29 primeros, de los movimientos por tiempo; del 30 al 47, de los de pulsos, con ruedas simuladas: 60 pulsos por segundo a velocidad 1 y 3 pulsos de resbalón). Rechaza ejecutarse contra un servicio en modo `--real`. En `--velocidad-cero` pasan solo los casos que no usan el simulador y **el camino de `avanzar` necesita los sensores de distancia despejados** (con algo a menos de 30 cm, termina al instante por `obstaculo`: es la protección, no un fallo).

```bash
dotnet run --project src/ExplorerHat.Orders -- --simulado --puerto 5099 --clave-archivo <ruta>
python tools/probar-ordenes/probar.py --url http://127.0.0.1:5099 --clave <clave>
```

Usa `127.0.0.1` y no `localhost`: en Windows, `localhost` prueba primero IPv6 y cada petición tarda 2 s de más, lo que falsea las pruebas de tiempo.
