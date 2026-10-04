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

Como lee el teclado, hay que ejecutarla en una terminal: en la Pi, o desde el PC con *Ejecutar en la Pi*.

## Nuestras medidas

Con las ruedas en el aire (velocidad 0,6): 85 pulsos en 2 s, unos 43 cm por segundo. En la parte 2 se paró a los
49 pulsos (50 cm) y la rueda siguió 4 pulsos más (4 cm). Con los motores nuevos (`HatMotor`, 04/10/2026): 92 pulsos en
2 s (47 cm por segundo); en la parte 2, parada a los 49 pulsos y 5 más.

En el suelo (7 pruebas, velocidad 0,6, UPS HAT (B), suelo liso):

* **Parte 1:** casi siempre de 72 a 77 pulsos, **37–39 cm por segundo**. Va más despacio que en el aire, porque las
  ruedas mueven todo el peso del robot.
* **Parte 2:** siempre se paró a los 49 pulsos (50 cm) y la rueda siguió de **4 a 7 pulsos más**. En total, unos
  **55 cm**. En el suelo resbala más que en el aire: la inercia de todo el robot empuja la rueda.
* **Con la cinta:** cuando el robot fue recto, midió 55 cm, igual que lo que contó (55,1 cm).
* **Se desvía a la izquierda:** la rueda derecha gira más rápido que la izquierda. Solo medimos la derecha, así que
  el robot no se entera. Para ir recto hace falta un sensor en cada rueda (segunda parte de la lección).

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

## Si no cuenta ningún pulso

Si el programa dice `0 pulses` y `Time is up!`, el sensor no ve pasar ninguna ranura. Hay dos causas posibles: el
sensor no envía la señal o **la rueda derecha no gira**. Pasó el 04/10/2026: el cable azul del motor derecho se había
soltado de la borna MOTOR 1. La Pi enviaba la señal al motor, pero la corriente no llegaba, y la rueda derecha estaba
quieta mientras la izquierda giraba. A simple vista no se notaba: parecía que giraban las dos.

Comprueba, en este orden:

1. **Mira la rueda derecha** mientras el programa la mueve. Si no gira, revisa el motor (abajo).
2. **El sensor, sin motores:** con el LED de alimentación del sensor encendido, gira la rueda a mano. Su LED de señal
   debe parpadear con cada ranura. En la Pi, `pinctrl lev 25` debe cambiar entre 0 y 1.
3. **El cable D0** va a INPUT 4, no a OUTPUT 4.

Si la rueda no gira:

1. **La borna MOTOR 1:** tira con suavidad de los dos cables. Si uno sale o se mueve, aprieta su tornillo.
2. **Las lengüetas del motor:** el cable va soldado a dos lengüetas de metal muy finas. Mira si una soldadura está
   suelta o una lengüeta está rota.
3. **El disco:** no debe rozar el sensor ni el chasis.
4. Con las ruedas en el aire, `bash tools/probar-cableado.sh` prueba cada motor por separado.

## Retos

* **Otra distancia.** Cambia `distance` a 100. ¿Llega? ¿Cuánto resbala?
* **Frena antes.** Si sabes cuánto resbala (por ejemplo, 4 cm), para los motores 4 cm antes de llegar.
* **La tabla de velocidades.** Mide los cm por segundo con `speed` a 0,4, 0,6, 0,8 y 1,0. ¿Coinciden con nuestra tabla?
* **Ida y vuelta.** Avanza 50 cm, da media vuelta (lección 04) y vuelve. ¿Llega al punto de salida?
* **Cuentakilómetros.** Muestra en la pantalla los centímetros mientras el robot avanza.
