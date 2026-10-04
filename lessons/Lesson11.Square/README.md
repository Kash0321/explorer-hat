# Lección 11, tercera parte: El cuadrado perfecto

La lección 04 otra vez, pero ahora el robot no cuenta tiempo: cuenta los pulsos de sus ruedas. Avanza 30 cm, gira
90 grados y repite 4 veces. **Mueve los motores.**

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire.** Después, con el visto bueno del monitor, ponlo en el suelo con un
> metro libre alrededor.

## Qué aprende

* **Girar un ángulo exacto con geometría.** Al girar sobre sí mismo, cada rueda da vueltas alrededor del centro del
  robot. El círculo que recorre tiene como diámetro la distancia entre las ruedas.
* **Cada rueda se para por separado** cuando llega a sus pulsos.
* **Calibrar lo que el cálculo no sabe:** el robot resbala un poco después de parar (`brakePulses`).
* **Repasa** las partes 1 y 2: contar pulsos, ir recto con la corrección y protegerse si falla un sensor.

## Qué hardware usa

Los dos sensores de velocidad, como en la [segunda parte](../Lesson11.Straight/README.md): el izquierdo a 3,3 V en
el pin PWM de la fila lateral (GPIO 18) y el derecho en INPUT 4. Las luces verde (avanzar) y amarilla (girar).

## Las cuentas del giro

* Distancia entre las ruedas: 13 cm. En una vuelta entera sobre sí mismo, cada rueda recorre 13 × 3,14 = **40,8 cm**.
* 90 grados es un cuarto de vuelta: 40,8 / 4 = **10,2 cm**.
* Cada pulso son 1,02 cm, así que 90 grados son **10 pulsos**. Cada pulso vale 9 grados: no se puede girar con más
  precisión que eso.

Para girar a la derecha, la rueda derecha va hacia atrás y la izquierda hacia delante. Cada una se para cuando
llega a sus pulsos.

## Qué hace

Al empezar pregunta si quieres el **modo calibración** (`y`): el robot se para después de cada giro y espera una
tecla, para que puedas medir el ángulo con un transportador. Después repite 4 veces:

1. Avanza `side` (30 cm), corrigiendo las ruedas como en la segunda parte (luz verde).
2. Gira `angle` (90 grados) a la derecha (luz amarilla).

Después de cada movimiento, escribe los pulsos de cada rueda y **cuánto ha resbalado** cada una durante la pausa.

Como lee el teclado, hay que ejecutarla en una terminal: en la Pi, o desde el PC con *Ejecutar una lección en la Pi*.

## Calibrar `brakePulses`

Las cuentas son correctas, pero el robot no se para en el acto: después de parar los motores, cada rueda sigue
girando unos pulsos. En nuestra primera prueba en el suelo (`brakePulses` = 1), cada rueda se paró a los 9 pulsos y
**resbaló 3 o 4 más**: unos 12,5 pulsos, es decir, unos 112 grados. Con el transportador medimos unos **115 grados**.
Las cuentas encajaban; sobraba lo que resbala.

Por eso cada rueda se para `brakePulses` pulsos antes: 10 − 4 = 6 pulsos, y los que resbala la llevan a unos 10.
Con `brakePulses` = 4, **las cuatro esquinas midieron unos 90 grados**: cada rueda se paró a los 6 pulsos (alguna
vez, 7); la izquierda resbaló 2 o 3 más y la derecha, de 3 a 6. Con 3, las esquinas salían de unos 120 grados, y
con 5, de unos 70.

La rueda derecha resbala más porque en el giro va hacia atrás. (Hasta el 04/10/2026 esa rueda giraba más despacio
hacia atrás y bastaba con `brakePulses` = 3: lo cambió la forma de mover los motores, ver `HatMotor` en el README
de Common.)

El sensor solo cuenta cuando empieza una ranura: al empezar y al terminar, el disco puede estar a mitad de un pulso.
Por eso hay un margen de más o menos un pulso, unos 9 grados. Y lo que resbala cambia un poco de una esquina a
otra: por eso unas esquinas salen algo más abiertas que otras.

1. Ejecuta la lección en modo calibración (`y`).
2. Mide cada esquina con el transportador.
3. Si gira más de 90 grados, sube `brakePulses`. Si gira menos, bájalo.

En los lados pasa lo mismo: 29 pulsos y 6 a 8 más resbalando, unos 36 cm en lugar de 30. El cuadrado sale más
grande, pero sigue siendo un cuadrado.

## Retos

* **Lados exactos.** Para también los lados unos pulsos antes, como en el reto "Frena antes" de la primera parte.
* **Más despacio.** Baja `turnSpeed` a 0,4. ¿Resbala menos? ¿Necesitas otro `brakePulses`?
* **Un triángulo.** 3 lados y giros de 120 grados. ¿Cuántos pulsos son?
* **Comparar.** Dibuja el cuadrado con la lección 04 y con esta. ¿Cuál vuelve más cerca del punto de salida? Prueba
  también con las baterías menos cargadas.
* **Girar a la izquierda.** Escribe un método `TurnLeft`.
