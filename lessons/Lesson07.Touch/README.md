# Lección 07: Pads táctiles

El HAT tiene ocho pads táctiles: placas de cobre que notan un dedo sin ningún botón. Con cuatro de ellos haces un
**mando a distancia** para el robot. **Mueve los motores, pero solo con `moveMotors = true`**, y solo mientras
mantienes el dedo en un pad.

## Aviso de seguridad

> ⚠️ **Primero con `moveMotors = false`** (el valor de fábrica): el robot solo enciende las luces y escribe en la
> terminal qué pad tocas. Cuando funcione, cambia `moveMotors` a `true` y prueba **con las ruedas en el aire** (el
> robot sobre un bote o un libro). Después, en el suelo.
>
> ⚠️ **Si el robot no se para, pulsa Ctrl+C.** Para los motores y apaga las luces. Si algo falla, usa la tarea
> *Parar el robot* de VS Code o `bash tools/parar-robot.sh`.

## Qué aprende

* Un sensor **táctil capacitivo**: el pad es una placa de cobre y el chip (CAP1208) mide cuánta carga eléctrica
  acepta (su **capacidad**). Tu dedo es conductor y, al acercarlo, aumenta esa capacidad. El chip nota el cambio y
  marca el pad como "tocado". No hace falta apretar: basta con acercar el dedo.
* Un sensor que dice **verdadero o falso** (`IsTouched()`) y no un número, como el de la lección 06 o el de la 08.
* Un **mando a distancia**: el robot hace algo mientras mantienes el dedo y se para al soltarlo. Es una forma
  segura de manejar un robot (si te asustas, sueltas).
* Varios `if` / `else if` seguidos con cuatro opciones, y un método propio (`SetSpeeds`) para no repetir código.

## Qué hardware usa

* Los pads **1 a 4** del HAT (no hace falta conectar nada).
* Las cuatro luces: **cada pad enciende su propia luz**, en el orden de las luces del HAT: pad 1 azul, pad 2
  amarilla, pad 3 roja y pad 4 verde.
* Los dos motores, solo con `moveMotors = true`.

## Qué hace

Repite durante `maxTime` (60 s), unas 15 a 20 veces por segundo:

1. Pregunta a cada pad si lo tocan (`hat.Touch.One.IsTouched()`...).
2. Apaga las luces y, según el pad, enciende una luz y pone una velocidad a cada rueda:

   | Pad | Luz | Movimiento | Rueda derecha (`One`) | Rueda izquierda (`Two`) |
   |---|---|---|---|---|
   | 1 | Azul | Avanzar | `speed` | `speed` |
   | 2 | Amarilla | Retroceder | `-speed` | `-speed` |
   | 3 | Roja | Girar a la izquierda | `speed` | `-speed` |
   | 4 | Verde | Girar a la derecha | `-speed` | `speed` |
   | ninguno | ninguna | Parado | 0 | 0 |

3. Escribe en la terminal el movimiento, pero solo cuando cambia (si no, la pantalla se llenaría de líneas).

## Cosas que hay que saber de los pads

Lo comprobamos en el robot (17/10/2026):

* **Un pad a la vez.** De fábrica, el chip solo cuenta un toque cada vez: si tocas dos pads, solo sale uno. Por eso
  el programa usa `else if` y no hay giros mientras avanza.
* **Se suelta al instante.** Al levantar el dedo, la luz se apaga y la rueda se para sin retraso apreciable.
* **Un golpecito rapidísimo puede perderse.** Con un toque muy, muy corto, a veces la luz ni se enciende: el programa
  pregunta unas 15 a 20 veces por segundo y el toque pasa entre dos preguntas. Un golpecito rápido sí parpadea, y un
  toque normal (de 0,3 a 0,6 s) siempre se ve. Por eso la lección usa "mantener el dedo", no "contar toques".
* **Los motores no engañan a los pads.** Con los motores a toda velocidad y nadie tocando, el chip no notó ningún
  toque falso (probado con los dos motores en los dos sentidos).
* **Numeración.** El HAT numera los pads de otra forma que el chip (los pads 1 a 4 del HAT son las entradas 5 a 8 del
  chip). `SafeExplorerHat` ya lo tiene en cuenta: tú usas los números que están impresos en el HAT.

## Si no funciona

| Qué pasa | Qué hacer |
|---|---|
| Falla al empezar con una excepción de `IOException` | El chip no contesta en la dirección 0x28 (o contesta otro chip). Mira `/usr/sbin/i2cdetect -y 1`: deben salir 0x28 y 0x48 (y 0x42 de la UPS). |
| Ningún pad responde | Comprueba con `/usr/sbin/i2cdetect -y 1` que sale 0x28. |
| La luz de un pad no coincide | Comprueba el número impreso junto al pad en el HAT. |
| El robot avanza al revés | Comprueba los cables de los motores (ver el README del repositorio). |

## Retos

* **Más rápido, más lento.** Cambia `speed` (0.4, 0.8). ¿Qué velocidad es la más fácil de manejar?
* **El turbo.** Haz que, mientras tocas el pad 5 (`hat.Touch.Five`), la velocidad sea 1.0. Recuerda que el chip solo
  cuenta un toque a la vez: ¿puedes usar el turbo y avanzar a la vez? ¿Por qué no? Piensa otra forma (por
  ejemplo, que el pad 5 cambie la variable `speed` cada vez que se toca).
* **El claxon de luces.** Con el pad 8, enciende las cuatro luces a la vez.
* **Cuenta los toques.** Cuenta cuántas veces tocas el pad 1 en 10 segundos (necesitas una variable
  `wasTouched`, como en la lección 05). ¿Cuentas todos los golpecitos? ¿Por qué no?
* **El baile.** Escribe una secuencia (avanzar, girar, retroceder) que se ejecute cuando toques el pad 5.
  **Pruébalo con las ruedas en el aire** y termina siempre con `Speed = 0.0`.
