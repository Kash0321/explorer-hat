# Lección 09: Robot autónomo

El robot avanza solo. Cuando ve un obstáculo, se para, retrocede un poco y gira hacia el lado con más sitio.
Después sigue. Nadie le dice qué hacer: **mira, piensa y actúa**. **Mueve los motores.**

Es una versión sencilla de [ObstacleAvoidance](../../src/ExplorerHat.ObstacleAvoidance/README.md), sin hilos ni
clases: todo el programa se lee de arriba abajo.

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire:** pon la mano delante de cada sensor y mira hacia dónde gira.
> Después, con el visto bueno del monitor, ponlo en el suelo con sitio libre y algunas cajas como obstáculos.
> Ten cerca la terminal para pulsar **Ctrl+C**.

## Qué aprende

* **Mirar, pensar y actuar**: el bucle de todos los robots autónomos. Mide los sensores, decide con `if` y mueve
  los motores. Y vuelta a empezar.
* **Juntar condiciones** con `||` (o) y `&&` (y):
  * Hay obstáculo si el centro **o** la izquierda **o** la derecha están cerca: `center < obstacleDistance || ...`.
  * El camino está libre si el centro **y** el lado del que se aleja están lejos: `center >= obstacleDistance && ...`.
* **Repasa** los métodos de la lección 04 (`MoveBackwards`, `TurnRight`...) y el sensor con su filtro de la
  lección 06, ahora con los tres sensores.
* **Un método que devuelve un valor**: `Measure(sensor)` devuelve la distancia medida, y sirve para los tres sensores.

## Qué hardware usa

* Los tres sensores HC-SR04:

  | Sensor | TRIG | ECHO |
  |---|---|---|
  | Izquierda (`leftSensor`) | OUT3 (GPIO 13) | IN3 (GPIO 24) |
  | Centro (`centerSensor`) | OUT1 (GPIO 6) | IN1 (GPIO 23) |
  | Derecha (`rightSensor`) | OUT2 (GPIO 12) | IN2 (GPIO 22) |

* Los dos motores y las cuatro luces, como en la lección 03: verde = avanzar, roja = retroceder, amarilla = girar
  a la derecha, azul = girar a la izquierda.

## Qué hace

Durante `maxTime` (60 s) repite:

1. **Mirar** (`Look`): mide los tres sensores, uno detrás de otro, y escribe las tres distancias. Entre dos
   lecturas espera `echoTime` (60 ms), para que el eco de un sensor no llegue al siguiente. Mira unas 5 veces por
   segundo.
2. **Pensar y actuar:**
   * Si ningún sensor ve nada a menos de `obstacleDistance` (30 cm): avanza (luz verde) y vuelve a mirar.
   * Si alguno lo ve: escribe `Obstacle!`, se para y retrocede durante `backTime` (300 ms). Después mira dos veces
     y gira hacia el lado con más sitio. Si la izquierda está más cerca que la derecha, gira a la derecha. Si no,
     a la izquierda.

**El giro es a pasos.** El robot gira `turnStepTime` (150 ms), se para y mira otra vez. Termina de girar cuando no
hay nada delante ni en el lado del que se aleja. Por ejemplo, si gira a la derecha, mira el centro y la izquierda.
Si después de `maxTurnSteps` (10) pasos el camino sigue bloqueado, vuelve al bucle principal: retrocede otra vez y
gira de nuevo.

**Por qué mira dos veces** (`LookTwice`): el filtro de la lección 06 usa la más cercana de las dos últimas
lecturas. Después de moverse, una de esas lecturas es de antes, cuando el robot miraba hacia otro sitio. Al mirar
dos veces con el robot quieto, las dos lecturas son nuevas.

**Por qué para antes de cambiar de sentido** (`Stop` y `pauseTime`): si un motor pasa de golpe de ir hacia delante
a ir hacia atrás, necesita mucha corriente y el robot da un tirón.

## Prueba con las ruedas en el aire

Antes de ponerlo en el suelo, comprueba que decide bien:

1. Mano a unos 15 cm del sensor **izquierdo**: retrocede (luz roja) y gira a la **derecha** (luz amarilla). Quita
   la mano y vuelve a avanzar.
2. Mano delante del sensor **derecho**: retrocede y gira a la **izquierda** (luz azul).
3. Mano delante del **centro**, sin quitarla: gira hacia el lado con más sitio. Después de 10 pasos, retrocede otra
   vez y repite.

Con las ruedas en el aire el robot no se mueve, así que el obstáculo no cambia de sitio: tú decides cuándo deja de
verlo.

## Retos

* **Más listo con la velocidad.** Copia de la lección 06 la zona amarilla: si algo está a menos de 60 cm, avanza
  más despacio.
* **Siempre al mismo lado.** Haz que gire siempre a la derecha. ¿Sale peor de los rincones?
* **Más lejos o más cerca.** Cambia `obstacleDistance` a 20 o a 50. ¿Qué pasa en un pasillo estrecho?
* **Pasos más grandes.** Sube `turnStepTime` a 300. ¿Gira demasiado?
* **Cuenta los obstáculos.** Usa una variable que sume 1 cada vez que ve uno y escribe el total al final.
* **Sin filtro.** Usa solo la última lectura de cada sensor. ¿Choca más?
