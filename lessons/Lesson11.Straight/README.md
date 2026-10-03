# Lección 11, segunda parte: Ir recto

Ahora las dos ruedas tienen un sensor de velocidad. Dos motores nunca giran exactamente igual, así que el robot se
tuerce. Con los dos sensores, el robot compara sus ruedas mientras avanza: si una va por delante, la frena un poco y
acelera la otra. **Mueve los motores.**

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire.** Después, con el visto bueno del monitor, ponlo en el suelo con
> unos 2 metros libres en línea recta y una línea o una cinta métrica como referencia.

## Qué aprende

* **Realimentación** (*feedback*): el robot mide lo que hace y lo corrige. Es lo que hace una persona al conducir
  una bici: si se va hacia un lado, gira un poco el manillar hacia el otro.
* **Control proporcional:** cuanto mayor es la diferencia entre las ruedas, mayor es la corrección
  (`difference * correction`).
* **Comparar con y sin corrección** cambiando una sola variable: `correct`.
* **No fiarse de los sensores:** si un sensor falla, la corrección se equivoca. El programa lo detecta y se para.
* `Math.Clamp` (mantener un número entre un mínimo y un máximo) y `Math.Abs` (el número sin su signo).

## Qué hardware usa

* **Dos sensores de velocidad LM393**, uno en cada rueda:

  | Rueda | Motor | Alimentación (VCC) | Señal (D0) | GPIO |
  |---|---|---|---|---|
  | Izquierda | Two | **3,3 V** (pin 3v3 de la fila lateral) | **PWM** de la fila lateral "3.3V ONLY" | 18 |
  | Derecha | One | 5 V | INPUT 4 | 25 |

* Las luces verde, amarilla, azul y roja.

> ⚠️ **El sensor izquierdo va a 3,3 V, nunca a 5 V.** Los pines de la fila lateral del HAT ("3.3V ONLY") llegan
> directos a la Pi, sin protección: con el sensor a 5 V, su salida D0 daría 5 V y podría dañar la Pi. Las entradas
> IN1 a IN4 sí admiten 5 V, pero IN1 a IN3 son de los sensores de distancia.

Comprueba que el disco de cada rueda pasa centrado por la ranura de su sensor y que el sensor no se mueve.

**Una curiosidad:** con el motor en marcha, la salida del sensor derecho (por IN4) da saltos falsos de unos
microsegundos en cada borde de ranura, y la del izquierdo (por GPIO 18) no da ninguno. Las entradas de la Pi tienen
**histéresis** (un *disparador de Schmitt*): para cambiar de 0 a 1 la señal tiene que subir bastante, y para volver
a 0, bajar bastante, así que no les afectan las dudas en la zona media. El búfer que protege las entradas IN del HAT
no la tiene y copia esas dudas. El programa no los ve, porque lee cada milisegundo.

## Qué hace

1. Avanza hasta que el centro del robot recorre `distance` (100 cm): la media de las dos ruedas.
2. A cada milisegundo cuenta los pulsos de las dos ruedas y calcula la diferencia (`right - left`).
3. Si `correct` es `true`, la rueda que va por delante va más despacio y la otra más deprisa:
   `speed - difference * correction` y `speed + difference * correction`. Con `correction` a 0,02, una diferencia de
   5 pulsos cambia la velocidad en 0,1.
4. Las luces muestran la diferencia: **verde** = iguales, **amarilla** = la derecha va por delante, **azul** = la
   izquierda va por delante.
5. Al final, la luz roja y el total de pulsos de cada rueda.

**Protección:** con la corrección, si la diferencia pasa de `maxDifference` (10 pulsos), el robot se para y avisa.
Si un sensor no cuenta, el programa cree que la otra rueda va muy por delante, la frena del todo y pone la suya a
tope: el robot gira sobre una rueda. Nos pasó con el sensor izquierdo fuera de su sitio: la rueda derecha se paró y
la izquierda contó 0 pulsos. Sin la corrección no hay protección, porque la diferencia crece de verdad.

## Nuestras medidas

En el aire, las dos ruedas giran casi igual (menos de un 1 % de diferencia a 0,6). En el suelo no: el peso no se
reparte igual, y una rueda patina más que la otra.

| | Diferencia al final (derecha − izquierda) | Separación de la línea recta en 1 m |
|---|---|---|
| Sin corrección (7 pruebas) | de 4 a 11 pulsos | **más de 45 cm**, siempre a la izquierda |
| Con corrección (17 pruebas) | de 0 a 4 pulsos | **de 2 a 10 cm**, a un lado o al otro |

Velocidad 0,6, `correction` 0,02, UPS HAT (B), suelo liso. Las diferencias incluyen el medio segundo que el robot
sigue rodando después de parar, cuando ya no corrige.

¿Por qué no va perfectamente recto? Con una corrección proporcional siempre queda una pequeña diferencia: hace falta
que una rueda vaya algo por delante para que exista la corrección. Nuestras ruedas están a 13 cm una de otra: 1 pulso de diferencia
(1 cm) ya tuerce el robot unos 4 grados.

## Retos

* **Sin corrección.** Pon `correct = false` y mide cuánto se desvía. Después, `true`. ¿Cuánto mejora?
* **Más o menos corrección.** Prueba `correction` a 0,005 y a 0,1. ¿Con cuál va más recto? ¿Con cuál hace eses?
* **Más lejos.** Cambia `distance` a 200. ¿Sigue recto?
* **Girar un ángulo exacto.** Para girar sin moverse del sitio, una rueda va hacia delante y la otra hacia atrás.
  En una vuelta entera, cada rueda recorre un círculo con la distancia entre las ruedas como diámetro: 13 × 3,14 =
  40,8 cm. ¿Cuántos pulsos hacen falta para 90 grados? (Pista: un cuarto de vuelta, 10,2 cm, unos 10 pulsos.)
* **El cuadrado perfecto.** Usa los pulsos en lugar del tiempo para el cuadrado de la lección 04.
