# Lección 06: Distancia

El sensor de ultrasonidos del centro mide la distancia al obstáculo que hay delante del robot. Las luces dicen si
está lejos o cerca, y el robot va más despacio y se para antes de tocarlo. **Mueve los motores**, pero solo si
cambias `moveMotors` a `true`.

## Aviso de seguridad

> ⚠️ **Con `moveMotors = true`, prueba primero con las ruedas en el aire:** pon la mano delante del sensor y
> acércala poco a poco. Después, con el visto bueno del monitor, ponlo en el suelo mirando a una pared o a una caja,
> a más de un metro.

## Qué aprende

* Un **sensor** da números: la distancia en centímetros. El programa decide con esos números.
* Una **condición con tres caminos**: `if`, `else if` y `else`. El programa elige solo uno de los tres.
* Un **bucle `while`**: repite mientras se cumple una condición (que no haya pasado el tiempo máximo).
* **Salir de un bucle con `break`**: cuando el obstáculo está demasiado cerca, el robot se para y el bucle termina.
* **No fiarse de una sola lectura**: a veces el sensor se equivoca. El programa usa la más cercana de las dos
  últimas lecturas.
* Un **cronómetro** (`Stopwatch`): cuenta el tiempo desde que empieza el programa.

## Qué hardware usa

* El sensor HC-SR04 del **centro**: TRIG en OUT1 (GPIO 6) y ECHO en IN1 (GPIO 23).
* Las luces verde, amarilla y roja.
* Los dos motores, solo con `moveMotors = true`.

## Cómo mide la distancia el sensor

El HC-SR04 es como un murciélago. Envía un sonido muy agudo (ultrasonido, a 40 000 hercios), que las personas no
oyen. El sonido rebota en el obstáculo y vuelve. El sensor mide cuánto tarda en volver: el **eco**. El sonido
recorre unos 343 metros por segundo. Si el eco tarda 2 milésimas de segundo, el sonido recorrió unos 68 cm entre
la ida y la vuelta: el obstáculo está a 34 cm.

El sensor mide de 2 a 400 cm. Si el eco no vuelve, el programa entiende que no hay nada delante (400 cm).

## Qué hace

Repite durante `maxTime` (10 s), unas 15 veces por segundo:

1. Mide la distancia y la escribe en la pantalla.
2. Si el obstáculo está a menos de `stopDistance` (30 cm): luz **roja**, para los motores y termina.
3. Si no, y está a menos de `slowDistance` (60 cm): luz **amarilla** y velocidad lenta (`slowSpeed`).
4. Si no: luz **verde** y velocidad rápida (`fastSpeed`).

Entre dos lecturas espera `pauseTime` (60 ms), para que los ecos de la lectura anterior se apaguen.

Con `moveMotors = false`, el programa hace lo mismo, pero no mueve los motores. Úsalo primero para ver las
distancias que mide el sensor.

## Las lecturas falsas

A veces el sensor no oye el eco del obstáculo y oye el de la pared de detrás: da una distancia mucho más lejana que
la real. Es peligroso, porque el robot cree que el camino está libre y no frena.

Estas lecturas falsas casi siempre vienen solas: la lectura siguiente vuelve a estar bien. Por eso el programa
guarda la lectura anterior (`lastReading`) y usa la más cercana de las dos (`Math.Min`). Si una lectura sale mal,
la otra la corrige. El precio es pequeño: cuando el obstáculo se aparta, el robot tarda una lectura más en verlo.

## Frenar a tiempo

El robot no se para en el acto. Entre dos lecturas el robot sigue avanzando, y al cortar la corriente sigue un poco
por la inercia. Por eso `stopDistance` no puede ser muy pequeño. Mide en el suelo dónde se para de verdad.

Nuestra prueba (velocidades 0,8 y 0,6, UPS HAT (B), suelo liso, pared a 115 cm): el robot empezó a ir despacio al
leer 58 cm, leyó 27 cm, se paró y quedó a unos **20 cm** de la pared, igual en las dos pruebas. Entre dos lecturas
avanzaba unos 3 cm (rápido) o 2,5 cm (despacio). Los 7 cm que faltan son lo que recorre desde la última lectura hasta pararse del todo.

El filtro no retrasa la frenada: cuando el robot se acerca, la lectura nueva es la más cercana de las dos. Solo
tarda una lectura más en dar el camino por libre cuando el obstáculo se aparta.

## Retos

* **Mide la frenada.** Pon una regla en el suelo. ¿A cuántos centímetros de la pared se para el robot? ¿Y si
  subes `fastSpeed` a 1.0?
* **Más zonas.** Añade otro `else if` con la luz azul para "muy lejos" (más de 100 cm).
* **Sin filtro.** Cambia `distance` por `reading` y mira qué pasa con las lecturas. ¿Se nota en el suelo?
* **Marcha atrás.** Cuando el obstáculo está demasiado cerca, retrocede un poco antes de terminar.
* **No te acerques.** En lugar de terminar con `break`, que el robot espere parado y siga cuando quites el
  obstáculo.
* **Mantén la distancia.** Si el obstáculo está a menos de 20 cm, retrocede; entre 20 y 40, párate; si está más
  lejos, avanza. Persigue tu mano.
