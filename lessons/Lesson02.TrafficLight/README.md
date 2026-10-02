# Lección 02: Semáforo

El robot hace de semáforo: verde, amarillo y rojo, varias veces. Al final, parpadea en amarillo como un semáforo
de noche. **No mueve los motores.**

## Qué aprende

* Una **variable** guarda un valor con un nombre: `int greenTime = 3000;`. Si cambias el número en un solo sitio,
  cambia todo el programa.
* Un **ciclo** es una secuencia que se repite. El bucle `for` lo repite `cycles` veces.
* El contador del bucle (`cycle`) se puede usar dentro: el programa escribe "Cycle 1 of 3", "Cycle 2 of 3"...

## Qué hardware usa

Las luces verde (GPIO 5), amarilla (GPIO 17) y roja (GPIO 27) del Explorer HAT.

## Qué hace

Dura unos 27 segundos con los valores iniciales.

1. Repite 3 veces: verde 3 s, amarillo 1 s y rojo 3 s.
2. Modo noche: la luz amarilla parpadea 6 veces.

## Retos

* **Semáforo con prisa.** Cambia los tiempos de las variables. ¿Qué pasa si `yellowTime` vale `0`?
* **Más ciclos.** Cambia `cycles` a `5`.
* **Semáforo para peatones.** Usa la luz azul como el muñeco de los peatones: que se encienda mientras el
  semáforo de los coches está en rojo.
* **Aviso antes del rojo.** Haz que la luz verde parpadee 3 veces antes de pasar al amarillo.
* **Tu variable.** Crea una variable `blinkTime` para el parpadeo del modo noche y úsala en los dos
  `Thread.Sleep`.
