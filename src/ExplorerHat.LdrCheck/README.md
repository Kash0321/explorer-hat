# Medidor de fotorresistencias (LdrCheck)

Mide la resistencia de una fotorresistencia (LDR) con el **ADS1015 del Explorer HAT Pro**, sin multímetro. Para cada
LDR hace tres medidas (luz de la sala, con un dedo encima y a oscuras) y al final escribe una tabla. **No mueve los
motores** y no toca ningún pin: solo lee el conversor por I2C.

Las medidas que ya se hicieron (con multímetro), el divisor de tensión y las ideas de uso están en
[`docs/fotorresistencias-LDR.md`](../../docs/fotorresistencias-LDR.md). Este programa sirve para medir **otras** LDR con
el robot. Se escribió el 10/10/2026, antes de tener multímetro, y se probó sin nada conectado.

## Qué hardware usa

* **El ADS1015** del HAT (I2C, bus 1, dirección 0x48). Va alimentado a 5 V: las entradas admiten de 0 a 5 V.
* **Un divisor de tensión** con la LDR y una resistencia fija de **10 kΩ** (marrón, negro, naranja), en una
  protoboard:

  ```
  5V del HAT ── LDR ──┬── 10 kΩ ── GND del HAT
                      │
                      └── Analog 1
  ```

* Opcional: **Analog 2 → resistencia de 10 kΩ → 5V**, para que el programa mida la alimentación en lugar de suponer
  5,30 V.

> ⚠️ **Conecta el montaje con el robot apagado,** y comprueba con el zumbador del multímetro que la fila del 5V de la
> protoboard está unida al pin 5V del HAT: una protoboard no lleva alimentación por sí sola.
>
> ⚠️ **El programa lee el nodo en Analog 1,** y el potenciómetro de la lección 08 también está en Analog 1. Si
> ejecutas el programa con el potenciómetro conectado, mide el potenciómetro y no la LDR. **Desconecta antes el cable
> SEÑAL (negro) del potenciómetro,** o cambia `Node` en `Program.cs` a otra entrada libre (por ejemplo, Analog 3 es
> `InputMultiplexer.AIN1`; el HAT numera las entradas al revés que el chip).

## Cómo se calcula la resistencia

`R de la LDR = R fija × (V de la alimentación / V del nodo − 1)`. Es la fórmula del divisor de tensión despejada.
La tensión de la alimentación es la que mide en **Analog 2**, si está conectada, o 5,30 V (la del pin 5V del HAT,
medida el 04/10/2026).

Cada lectura usa **la escala más pequeña del ADS1015 que admite la tensión** (hasta ±0,256 V, con 0,125 mV por paso).
Así mide bien varios MΩ. Por eso el programa usa `Ads1115` directamente y **no `hat.Analog` de `ExplorerHat.Common`**:
`hat.Analog` usa una escala fija de ±6,144 V (3 mV por paso) y a oscuras, con una LDR de varios MΩ, perdería
resolución. No lo "simplifiques" pasándolo a `hat.Analog`.

## Qué hace, paso a paso

1. Mide la alimentación en Analog 2 (si no está conectada, usa 5,30 V y lo dice).
2. Pide el **nombre de la LDR** (Enter vacío para terminar) y, de forma opcional, los **lux** que marca una aplicación
   del móvil en la sala.
3. **Tres medidas de 2 s**, cada una cuando pulsas Enter:
   * con la luz de la sala;
   * con un dedo encima;
   * a oscuras: tápala del todo y espera 10 s (las hojas de datos miden la resistencia a oscuras tras 10 s).
4. Cada medida muestra la tensión (con el mínimo y el máximo) y la resistencia. Avisa si el nodo está casi a 0 V (¿está
   la LDR conectada al 5V?) o casi a la tensión de la alimentación (¿está la resistencia fija conectada a GND?).
5. Al terminar, una **tabla** con todas las LDR.

## Cómo ejecutarlo

**Desde el PC con VS Code:** `Terminal > Run Task... > Ejecutar en la Pi` y elige `src/ExplorerHat.LdrCheck`. Pide
datos con el teclado: usa la terminal de la tarea.

**En la Raspberry Pi**, para guardar el resultado (en `~`, no en `/tmp`: en la Pi, `/tmp` es `tmpfs` y se borra al
reiniciar):

```bash
cd ~/apps/ExplorerHat.LdrCheck && dotnet ExplorerHat.LdrCheck.dll | tee ~/ldr.log
```

Al **desplegarlo**, queda en `~/apps/ExplorerHat.LdrCheck` (el nombre de su carpeta). La copia antigua `~/apps/LdrCheck`
de la Pi se hizo a mano antes de mover el programa a `src/`: vale la de `ExplorerHat.LdrCheck`.

## Qué esperar

Con una LDR de la bolsa y 10 kΩ, las lecturas deberían parecerse a las del documento de las LDR: unos 23 kΩ con la luz
de la sala, unos 110 kΩ con un dedo encima y más de 2 MΩ a oscuras. Las demás LDR de la bolsa no se han medido: pueden
ser de otro modelo.
