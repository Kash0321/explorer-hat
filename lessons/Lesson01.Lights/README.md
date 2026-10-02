# Lección 01: Luces

Primer programa del taller. Enciende y apaga las cuatro luces del Explorer HAT. **No mueve los motores.**

## Qué aprende

* Un programa son **instrucciones que se ejecutan en orden**, de arriba abajo.
* `Thread.Sleep(1000)` **espera** 1000 milisegundos, es decir, un segundo. Sin esperas, todo pasaría tan rápido
  que no lo veríamos.
* El bucle **`for` repite** las instrucciones que hay entre `{` y `}`.
* `Console.WriteLine` escribe un mensaje en la pantalla del ordenador.

## Qué hardware usa

Las cuatro luces de la placa: azul (`Blue`, GPIO 4), amarilla (`Yellow`, GPIO 17), roja (`Red`, GPIO 27) y verde
(`Green`, GPIO 5). Cada luz es un **LED** (diodo emisor de luz): se enciende cuando el programa pone su pin a
nivel alto (3,3 V) y se apaga con el nivel bajo (0 V).

## Qué hace

Dura unos 10 segundos.

1. Enciende la luz azul durante un segundo.
2. Enciende las cuatro luces una detrás de otra, cada medio segundo.
3. Apaga todas las luces a la vez con `hat.Lights.Off()`.
4. Hace parpadear la luz roja 5 veces con un bucle `for`.

## Retos

Cambia una cosa cada vez y vuelve a ejecutar:

* **Más rápido o más lento.** Cambia los `Thread.Sleep(300)` del parpadeo por `100` o por `1000`.
* **Más parpadeos.** Cambia el `5` del bucle por `10`.
* **Otro color.** Haz parpadear la luz verde en vez de la roja.
* **Al revés.** Apaga las luces una a una, empezando por la verde.
* **Las cuatro a la vez.** Haz que parpadeen todas juntas con `hat.Lights.On()` y `hat.Lights.Off()`.
* **Coche fantástico.** Enciende las luces de una en una (azul, amarilla, roja, verde) y después en el orden
  contrario, sin dejar dos encendidas a la vez.
