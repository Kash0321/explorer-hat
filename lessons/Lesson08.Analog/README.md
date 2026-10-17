# Lección 08: Sensores analógicos

Un potenciómetro es un mando que se gira. El robot mide cuánto lo has girado y lo muestra con las cuatro luces,
como una barra: cuanto más giras, más luces se encienden. **No mueve los motores.**

## Aviso de seguridad

> ⚠️ **Monta y cambia los cables con el robot apagado.** Con la placa encendida, una punta que resbale entre dos
> pines puede unir 5V con GND. Apaga la Raspberry Pi (`sudo shutdown -h now`), espera a que deje de parpadear el
> LED verde, apaga la UPS y desconecta el cargador.
>
> ⚠️ **En este potenciómetro, el cable negro no es GND.** Es el cursor (la señal). Si lo conectas a GND y el rojo
> a 5V, al girar el mando hacia el rojo unes 5V con GND casi sin resistencia: un cortocircuito. Pon una etiqueta
> en el cable: **NEGRO = SEÑAL**.

## Qué aprende

* Un **sensor analógico** da una **tensión** (voltios) que cambia poco a poco. Un botón solo da dos valores
  (pulsado o no), y un potenciómetro da todos los intermedios.
* Cómo se mide: el HAT tiene un **conversor** (ADS1015) que convierte la tensión de 0 a 5 V en un número.
* Un **divisor de tensión**: dos resistencias en serie reparten la tensión. El potenciómetro es un divisor que
  cambias con la mano.
* **Poner un número en una escala** (`Math.Clamp`): un voltaje de 0 a 5,3 V pasa a un nivel de 0 a 1.
* Varios **`if` seguidos** (sin `else`): cada uno se comprueba por separado, y por eso puede haber varias luces
  encendidas a la vez.

## Qué hardware usa

* Un **potenciómetro** lineal de 10 kΩ (B10K), con un cable de tres hilos y una carcasa XH2.54:
  * **rojo**: un extremo de la pista, a **5V**;
  * **amarillo**: el otro extremo, a **GND**;
  * **negro**: el cursor (la patilla del medio), a **Analog 1**.
* La entrada **Analog 1** del HAT.
* Las cuatro luces.
* Para el reto de la luz: una **fotorresistencia** y una resistencia de **10 kΩ** (marrón, negro, naranja), en
  **Analog 2**.

## Montaje del potenciómetro (con el robot apagado)

La carcasa del cable es hembra y no se enchufa a una protoboard. Mete tres cables Dupont macho en sus agujeros y
pon una etiqueta en el otro extremo de cada uno: **5V** (el del hilo rojo), **SEÑAL** (el del negro) y **GND** (el
del amarillo).

1. **Comprueba el potenciómetro con el multímetro** (selector en `20k`, puntas en los extremos de los Dupont):
   * entre **5V y GND**: unos 9 a 10 kΩ, gires lo que gires el mando;
   * entre **SEÑAL y GND**: de 0 a unos 10 kΩ, y cambia al girar.
2. Conecta los cables en este orden: primero **GND**, después **SEÑAL** (a **Analog 1**) y al final **5V**.
3. El cable **5V tiene que ir a un punto que esté unido al pin 5V del HAT**, por ejemplo el nodo de 5V de la
   protoboard de la Pi. **Una protoboard no lleva alimentación por sí sola:** si el cable rojo va a una fila que
   nadie ha unido al 5V, el potenciómetro no recibe nada.
4. Con el multímetro en el **zumbador** (continuidad), comprueba que suena entre la fila del cable 5V y el pin 5V
   del HAT. Si no suena, no están unidos.
5. Enciende el robot y ejecuta la lección.

## Qué hace

Repite durante `maxTime` (30 s), unas 10 veces por segundo:

1. Mide la tensión de **Analog 1** (`hat.Analog.One.ReadVoltage().Volts`).
2. La convierte en un **nivel** de 0 a 1: 0 con `minVolts` (0 V) o menos, 1 con `maxVolts` (5,3 V) o más.
3. Calcula cuántas luces encender, de 0 a 4 (`lightsOn`).
4. Apaga todas las luces y enciende las primeras `lightsOn`: azul, amarilla, roja y verde.
5. Escribe en la terminal los voltios y una barra.

Con el mando al mínimo marca unos 0 V y no se enciende ninguna luz. Con el mando al máximo marca unos 5,3 V y se
encienden las cuatro. **5,3 V y no 5 V:** es la tensión real del pin 5V del HAT (la medimos).

## Cómo funciona un potenciómetro

Por dentro hay una pista de carbono de unos 10 kΩ entre el cable rojo y el amarillo. El cursor (el cable negro)
se desliza por la pista cuando giras el mando y toma una parte de la tensión:

```
5V ──┐
     │ pista roja ─ negro   (resistencia R1)
     ├──────────────── negro: Analog 1
     │ pista negro ─ amarillo   (resistencia R2)
GND ─┘
```

La tensión del cursor es `5,3 V × R2 / (R1 + R2)`. Con el cursor en el extremo del amarillo, R2 vale 0 y la tensión
es 0 V. Con el cursor en el extremo del rojo, R1 vale 0 y la tensión es toda la del 5V. Las dos mitades suman
siempre lo mismo (unos 9,3 a 10,4 kΩ según el potenciómetro). Por eso da igual que el valor exacto de cada
potenciómetro sea un poco distinto: lo que cambia la tensión es la fracción, no el total.

Nuestras medidas (potenciómetro WH148 B10K): al mínimo, 0,00 kΩ entre el cursor y el amarillo; a 1/4, 1,97 kΩ;
a la mitad, 4,41 kΩ; a 3/4, 7,31 kΩ; al máximo, 9,33 kΩ. Es casi lineal.

## Si no funciona

Mira en la terminal qué tensión marca con el mando al mínimo y al máximo.

| Qué marca | Qué pasa |
|---|---|
| Unos **0,5 V** y no cambia | El cable SEÑAL no hace contacto con Analog 1. La entrada **flota**: sin nada conectado no marca 0 V, sino unos 0,5 V. |
| Unos **0,02 V** y no cambia | Analog 1 está atada a masa (GND). Casi siempre es que **el cable 5V no recibe 5 V**: el cursor lee 0 V con cualquier giro. Haz la prueba de continuidad del paso 4. |
| Unos **5,3 V** y no cambia | Falta GND, o SEÑAL está unido a 5V. |
| Va de 5,3 V a 0 V al girar a la derecha | Está al revés. Intercambia los cables 5V y GND, o gira hacia el otro lado. No pasa nada. |

## La fotorresistencia (LDR)

Una fotorresistencia (LDR) es una resistencia que **baja** cuando le da la luz. Con una resistencia fija forma un
divisor de tensión, igual que el potenciómetro, pero lo cambia la luz y no tu mano:

```
5V ── LDR ──┬── 10 kΩ ── GND
            │
            └── Analog 2
```

Más luz: la LDR baja de resistencia y sube la tensión del nodo (el punto entre las dos). Con la fórmula del
potenciómetro, `V = 5,3 V × 10 kΩ / (R de la LDR + 10 kΩ)`.

Nuestras medidas (tres LDR casi iguales, con 10 kΩ):

| Condición | Resistencia de la LDR | Tensión esperada |
|---|---|---|
| A oscuras | más de 2 MΩ | menos de 0,03 V |
| Con un dedo encima | unos 110 kΩ | unos 0,4 V |
| Luz de la sala (55 a 60 lux) | unos 23 kΩ | unos 1,6 V |
| Una linterna de frente cerca | unos 12 kΩ | unos 2,4 V |

Tus valores serán distintos: dependen de la luz de tu sala. Mira en la terminal qué marca con luz y con la mano
encima, y ajusta `maxVolts` (por ejemplo, 2,5).

## Retos

* **La barra de la luz.** Conecta la LDR y la resistencia de 10 kΩ a **Analog 2**. Cambia `hat.Analog.One` por
  `hat.Analog.Two` y `maxVolts` por 2,5. Pon la mano encima y quítala: ¿cuántas luces se encienden en la sala?
* **La luz de noche.** Quita la barra. Si la tensión baja de un valor que elijas (`darkVolts`), enciende las
  cuatro luces. Si no, apágalas. ¿Con qué valor se enciende al tapar la LDR con la mano y no se enciende en la sala?
* **El nivel en porcentaje.** Escribe en la terminal el nivel de 0 a 100 (`level * 100`) y no los voltios.
* **La barra al revés.** Que se enciendan más luces cuantas menos voltios haya (`1 - level`).
* **Los cuatro colores.** Haz que solo se encienda una luz cada vez: azul con un nivel bajo, verde con un nivel alto.
* **El volumen de un motor.** Que el potenciómetro controle la velocidad de un motor (`Speed = level`). **Es el
  único reto que mueve los motores:** pruébalo solo con las ruedas en el aire y con el visto bueno del monitor. Pon
  el potenciómetro en el mínimo antes de empezar, y termina siempre con `Speed = 0.0`.
