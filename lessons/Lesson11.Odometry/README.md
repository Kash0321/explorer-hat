# Lección 11: Contar vueltas (odometría)

El robot cuenta las vueltas de su rueda derecha con un sensor de velocidad. Así sabe qué distancia ha recorrido:
primero mide su velocidad y después avanza una distancia exacta. **Mueve los motores.**

## Aviso de seguridad

> ⚠️ **Prueba primero con las ruedas en el aire.** Después, con el visto bueno del monitor, ponlo en el suelo con
> unos 2 metros libres en línea recta y una cinta métrica al lado.

## Qué aprende

* **Odometría:** medir la distancia contando las vueltas de las ruedas, como el cuentakilómetros de un coche.
* **El número pi (π):** una vuelta de la rueda avanza lo mismo que su borde, el diámetro por pi. Nuestra rueda mide
  6,5 cm de diámetro: 6,5 × 3,14 = 20,4 cm por vuelta.
* **Un `while` con dos condiciones** unidas con `&&`: el robot avanza mientras no llega a la distancia **y** no se
  acaba el tiempo.
* **Repasa** el contador de pulsaciones de la lección 05: el disco es como un botón que la rueda pulsa 20 veces en
  cada vuelta.
* **Medir y comparar:** ¿coincide lo que cuenta el robot con lo que mide la cinta?

## Qué hardware usa

* Un **sensor de velocidad LM393** en la rueda derecha (motor One), con el disco de 20 ranuras pasando por su ranura
  sin rozar. Conexión: VCC a 5V, GND a GND y D0 a **INPUT 4** (GPIO 25). A0 no se conecta.
* Los dos motores y las luces azul (parte 1), verde (parte 2) y roja (parado).

> ℹ️ Fíjate bien en el pin del HAT: INPUT 4, no OUTPUT 4. En nuestra primera prueba el cable estaba en OUTPUT 4 y no
> llegaba ningún pulso, aunque el LED del sensor parpadeaba.

## Cómo funciona el sensor

El sensor tiene forma de horquilla. En un lado hay un LED infrarrojo (una luz que no vemos) y en el otro un
**fototransistor**, un componente que deja pasar corriente cuando le llega luz. El disco gira dentro de la
horquilla: cuando pasa una ranura llega la luz, y cuando pasa la parte opaca se corta. El chip LM393 es un
**comparador**: convierte esa luz en una señal digital, 0 o 5 V, en su salida D0. Su LED de señal parpadea con
cada ranura.

20 ranuras dan **20 pulsos por vuelta**. Cada pulso es 20,4 / 20 = **1,02 cm**.

## Qué hace

1. **Parte 1 (luz azul):** avanza durante `measureTime` (2 s) contando pulsos, se para y calcula la velocidad en
   centímetros por segundo. Después espera una tecla, para que puedas medir con la cinta.
2. **Parte 2 (luz verde):** avanza hasta que los pulsos suman `distance` (50 cm) y se para. Si pasan `maxTime`
   (5 s) sin llegar, también se para: así no se escapa si el sensor falla.
3. Después de cada parte, sigue contando medio segundo: **el robot no se para en el acto**. La rueda sigue girando
   un poco por la inercia, y el programa lo muestra.

Como lee el teclado, hay que ejecutarla en una terminal: en la Pi, o desde el PC con *Ejecutar una lección en la Pi*.

## Nuestras medidas

Con las ruedas en el aire (velocidad 0,6): 85 pulsos en 2 s, unos 43 cm por segundo. En la parte 2 se paró a los
49 pulsos (50 cm) y la rueda siguió 4 pulsos más (4 cm).

En el suelo, cuando el robot fue recto, la distancia que contó coincidió con la de la cinta. Pero a menudo se
desvía: los dos motores no giran exactamente igual, y solo medimos la rueda derecha. Para ir recto hace falta un
sensor en cada rueda (segunda parte de la lección).

Velocidad de la rueda en el aire según `Speed`:

| `Speed` | Pulsos por segundo | Vueltas por minuto | cm por segundo |
|---|---|---|---|
| 0,4 | 37 | 112 | 38 |
| 0,6 | 46 | 139 | 47 |
| 0,8 | 51 | 154 | 52 |
| 1,0 | 55 | 166 | 56 |

La velocidad no es proporcional a `Speed`: de 0,4 a 1,0 solo sube la mitad.

## Los saltos del sensor

Al medir con mucha precisión vimos que, justo cuando una ranura empieza o termina, la salida del sensor salta 2 o 3
veces entre 0 y 1 en unas millonésimas de segundo. Pasa porque la luz no cambia de golpe, y el comparador de este
módulo no tiene **histéresis**, un pequeño margen que le obligaría a decidirse. El programa lee el sensor cada
milisegundo, mucho más despacio que esos saltos, así que no los ve. Una ranura dura al menos 7 milisegundos, así que
tampoco pierde ninguna.

## Retos

* **Otra distancia.** Cambia `distance` a 100. ¿Llega? ¿Cuánto resbala?
* **Frena antes.** Si sabes cuánto resbala (por ejemplo, 4 cm), para los motores 4 cm antes de llegar.
* **La tabla de velocidades.** Mide los cm por segundo con `speed` a 0,4, 0,6, 0,8 y 1,0. ¿Coinciden con nuestra tabla?
* **Ida y vuelta.** Avanza 50 cm, da media vuelta (lección 04) y vuelve. ¿Llega al punto de salida?
* **Cuentakilómetros.** Muestra en la pantalla los centímetros mientras el robot avanza.
