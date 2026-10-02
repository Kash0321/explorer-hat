# Lección 03: Motores

El robot avanza, retrocede, gira a la derecha y gira a la izquierda, un segundo cada vez. Cada movimiento
enciende una luz. **Mueve los motores.**

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire.** Pon el robot sobre un bote o un libro, sin que las ruedas
> toquen nada. Solo después, con el visto bueno del monitor, déjalo en el suelo con sitio libre alrededor.

## Qué aprende

* Cada motor tiene una **velocidad** (`Speed`) de −1,0 (toda hacia atrás) a 1,0 (toda hacia delante). El 0,0 es
  parado.
* El motor `One` es la **rueda derecha** y el motor `Two` la **izquierda**.
* El robot **gira** cuando las dos ruedas van en sentidos contrarios: para girar a la derecha, la rueda derecha va
  hacia atrás y la izquierda hacia delante.
* Cada movimiento **termina parando los dos motores** (`Speed = 0.0`) y con una pausa corta.

## Qué hardware usa

* **Motores TT** (amarillos, con reductora 1:48): motor 1 = GPIO 19 (velocidad) y 20 (dirección); motor 2 =
  GPIO 21 y 26.
* **Luces:** verde para avanzar, roja para retroceder, amarilla para girar a la derecha y azul para girar a la
  izquierda.

El chip del HAT que mueve los motores (un **puente en H**, el DRV8833) puede invertir la corriente que llega a
cada motor: así el mismo motor gira hacia delante o hacia atrás. La velocidad se controla con **PWM**: el programa
enciende y apaga el motor muchas veces por segundo, y cuanto más tiempo está encendido, más rápido gira.

## Qué hace

Dura unos 6 segundos: avanza, retrocede, gira a la derecha y gira a la izquierda, 1 s cada movimiento y 0,5 s de
pausa entre ellos.

## Retos

* **Más lento.** Cambia `speed` a `0.5`. ¿Qué velocidad es la más baja con la que todavía se mueven las ruedas?
  Por debajo de ella, el motor no tiene fuerza suficiente para arrancar.
* **Más tiempo.** Cambia `moveTime` a `2000`.
* **Girar con una sola rueda.** Para girar a la derecha, deja la rueda derecha parada (`0.0`) y mueve solo la
  izquierda. ¿En qué se diferencia el giro?
* **Una curva.** Pon las dos ruedas hacia delante, pero una más rápida que la otra (`0.8` y `0.5`).
* **Tu recorrido.** Escribe tu propia secuencia de movimientos, sin olvidar parar los motores al final de cada uno.
