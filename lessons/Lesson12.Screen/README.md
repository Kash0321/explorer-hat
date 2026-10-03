# Lección 12: La pantalla

El robot tiene una pantalla de 4 líneas con 20 caracteres cada una. Con ella puede enseñar mensajes y datos sin
necesidad de un ordenador: la distancia a un obstáculo, la batería, lo que está haciendo... **No mueve los motores.**

## Qué aprende

* **Mostrar texto y variables** en una pantalla: `screen.Write(1, "Number: " + number)`.
* **Las líneas se cuentan desde 0:** la primera es la línea 0 y la última, la 3. Lo mismo pasa en muchas cosas de la
  programación.
* **Juntar texto y números** con `+`.
* **Un espacio limitado:** en cada línea caben 20 caracteres. Lo que sobra no se ve.
* **Construir un texto con un bucle:** la barra de `#` crece con la distancia.
* **Repasa** el sensor de distancia y su filtro de la lección 06.

## Qué hardware usa

* **La pantalla LCD 2004** (20 columnas × 4 líneas) con su adaptador I2C, en la dirección 0x27. Se conecta a la fila
  lateral del Explorer HAT marcada "3.3V ONLY":

  | Adaptador | Explorer HAT |
  |---|---|
  | GND | GND |
  | VCC | **3v3** (nunca a 5V: el bus I2C llega directo a la Raspberry Pi) |
  | SDA | SDA |
  | SCL | SCL |

* El sensor de distancia del centro (TRIG en OUT1, ECHO en IN1).

> ℹ️ Si la pantalla se enciende pero no se ven las letras, gira despacio el **potenciómetro azul** del adaptador
> con un destornillador pequeño. Ajusta el **contraste**: cuánto se oscurecen los puntos de cada letra. A 3,3 V,
> el punto bueno está cerca de un extremo.

## Cómo funciona la pantalla

Cada carácter es una cuadrícula de 5 × 8 puntos. Un chip dentro de la pantalla (el HD44780) sabe dibujar las letras,
los números y algunos símbolos. El adaptador de la parte de atrás (el chip PCF8574) traduce las órdenes que le manda
la Raspberry Pi por el **bus I2C**: dos cables, SDA (datos) y SCL (reloj), que comparten la pantalla, la UPS y los
chips del Explorer HAT. Cada uno tiene su dirección, como las casas de una calle: la pantalla es la 0x27.

El chip de la pantalla **no tiene acentos ni eñes**: escribe "Hola", "nino" o "adios".

El programa usa `Screen`, una clase del proyecto `ExplorerHat.Common` que esconde la configuración del adaptador.
`screen.Write(línea, texto)` escribe la línea entera: si el texto es más corto, rellena el resto con espacios para
borrar lo que hubiera antes.

## Qué hace

1. **Parte 1 (5 s):** escribe un saludo con tu nombre (`name`), una regla de 20 números y una frase demasiado larga,
   que sale cortada ("This line is too lon").
2. **Parte 2 (7 s):** cuenta del 1 al 10 y escribe también el doble de cada número.
3. **Parte 3 (`panelTime`, 20 s):** un panel con la distancia del sensor central y una **barra**: un `#` por cada
   10 cm, como mucho 20 (2 metros). Si hay algo a menos de 30 cm, escribe "Too near!".
4. Al final se despide. La pantalla guarda el último texto aunque el programa termine.

## Retos

* **Tu nombre.** Cambia `name`. ¿Qué pasa si tu nombre tiene tilde o eñe?
* **Centrar.** Escribe tu nombre en el centro de la línea: ¿cuántos espacios hay que poner delante?
* **Cuenta atrás.** Del 10 al 0 y después "Despegue!".
* **Barra al revés.** Que la barra crezca cuando el obstáculo se **acerca**.
* **Los tres sensores.** Muestra la distancia de la izquierda, del centro y de la derecha, una en cada línea.
* **El robot habla.** Añade la pantalla a la lección 09: que escriba "Avanzo", "Obstaculo!" o "Giro a la derecha".
* **Cuentakilómetros.** Añade la pantalla a la lección 11: que muestre los centímetros recorridos mientras avanza.
