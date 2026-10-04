# Lección 04: Dibujar un cuadrado

El robot recorre un cuadrado: avanza, gira a la derecha y repite 4 veces. **Mueve los motores.**

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire.** Después, con el visto bueno del monitor, ponlo en el suelo con
> al menos un metro libre alrededor.

## Qué aprende

* Un **método** da un nombre a un grupo de instrucciones: `MoveForwards`, `TurnRight` y `Stop`. Después se usa
  ese nombre las veces que haga falta, sin repetir el código.
* Un método puede recibir un **parámetro**: `MoveForwards(1000)` avanza durante 1000 milisegundos.
* Un **bucle con movimiento**: el cuadrado es "avanzar y girar", 4 veces.
* **Calibrar**: probar y medir hasta encontrar el valor correcto (`turnTime`).
* Una **condición** con `if` y `else`: el programa hace una cosa u otra según la tecla que pulsas al empezar.

## Qué hardware usa

Los dos motores y las luces verde (avanzar) y amarilla (girar), como en la lección 03.

## Qué hace

Al empezar, pregunta si quieres el **modo calibración**: pulsa `y` para sí o cualquier otra tecla para no.

Después repite 4 veces: avanza durante `sideTime` (1 s) y gira a la derecha durante `turnTime` (200 ms). Después
de cada movimiento para los motores, apaga las luces y espera `pauseTime` (0,3 s) para que el robot no resbale.
En modo calibración, además, se para después de cada giro y espera a que pulses una tecla.

Como lee el teclado, hay que ejecutarla en una terminal: en la Pi, o desde el PC con *Ejecutar una lección en la Pi*.

## Calibrar el giro

El robot no sabe cuántos grados gira: solo sabe cuánto tiempo mueve las ruedas. Para un giro de 90 grados hay que
buscar el tiempo correcto:

1. Pon el robot en el suelo, alineado con una baldosa o con una cinta adhesiva.
2. Ejecuta la lección en modo calibración (`y`).
3. Después de cada giro, el robot se para: mide el ángulo con un transportador. A ojo es fácil equivocarse.
4. Si gira menos de 90 grados, sube `turnTime`. Si gira más, bájalo.
5. Repite hasta que las esquinas midan 90 grados. Después ejecútala sin modo calibración y mira si el robot vuelve
   cerca del punto de salida.

Nuestras medidas (velocidad 0,8, UPS HAT (B), suelo liso): 200 ms giran unos 90 grados y 250 ms unos 120. Las
cuatro esquinas salen casi iguales. (Hasta el 04/10/2026, la rueda que va hacia atrás giraba más despacio y hacían
falta 250 ms para 90 grados: lo cambió la forma de mover los motores, ver `HatMotor` en el README de Common.) El giro no crece de forma regular con el tiempo: el motor tarda
un poco en arrancar, así que con tiempos muy cortos casi no gira.

El tiempo correcto cambia con el suelo (baldosa, madera, alfombra), con la carga de las baterías y con la
velocidad. Por eso el cuadrado nunca sale perfecto: el robot no mide lo que hace. En la lección 11 contaremos las
vueltas de las ruedas con sensores para hacer giros precisos.

## Retos

* **Un cuadrado más grande.** Sube `sideTime`.
* **Al otro lado.** Escribe un método `TurnLeft` y dibuja el cuadrado girando a la izquierda.
* **Un triángulo.** ¿Cuántas veces hay que repetir y cuánto hay que girar? (Pista: 3 lados y giros de 120 grados.)
* **Un hexágono.** 6 lados y giros de 60 grados.
* **Luces de esquina.** Haz que la luz roja parpadee en cada esquina.
* **Ida y vuelta.** Avanza, da media vuelta (dos giros de 90 grados) y vuelve al punto de salida.
