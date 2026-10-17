# Fotorresistencias (LDR): medidas, divisor de tensión e ideas de uso

Todo lo que se midió y se decidió sobre las fotorresistencias (LDR) del proyecto, guardado para cuando se vuelvan a usar.
La lección 08 se hizo **solo con el potenciómetro** (decisión del 17/10/2026: montar la LDR pedía una protoboard y
complicaba el montaje, y el potenciómetro ya explica bien el tema). Las medidas, el divisor y el texto para los niños se
conservan aquí.

- [Resumen](#resumen)
- [El material](#el-material)
- [Cómo se midió](#cómo-se-midió)
- [Resultados](#resultados)
- [Qué modelo parece ser](#qué-modelo-parece-ser)
- [El divisor de tensión](#el-divisor-de-tensión)
- [Cómo conectarlo al robot](#cómo-conectarlo-al-robot)
- [Ideas de uso](#ideas-de-uso)
- [Texto para una lección (reutilizable)](#texto-para-una-lección-reutilizable)
- [El programa LdrCheck](#el-programa-ldrcheck)
- [Dónde está el resto y qué falta por saber](#dónde-está-el-resto-y-qué-falta-por-saber)

## Resumen

| Dato | Valor |
|---|---|
| Cantidad | Una bolsa de 20, encontrada el 10/10/2026. Modelo sin confirmar |
| Medidas hechas con | Multímetro ExtraStar DT830B ([manual](multimetro-DT830B.md)), el 17/10/2026, en 3 LDR |
| Con la luz de la sala (55 a 60 lux) | unos **23 kΩ** |
| Con una luz de frente cerca (~170 lux, estimado) | unos **12 kΩ** |
| Con un dedo encima | unos **110 kΩ** |
| A oscuras | **más de 2 MΩ** (el multímetro marca "1": fuera de rango) |
| Modelo probable | GL5539 (o GL5537-2). Solo orientativo |
| Resistencia fija recomendada para el divisor | **10 kΩ** (ya la hay). Con 20 kΩ (dos de 10 kΩ en serie) o 22 kΩ, más margen |

## El material

* **LDR de 5 mm**, sin marca. Las de 5 mm suelen ser de la serie GL55xx, iguales por fuera: una bolsa puede ser un
  surtido de varios modelos. Las tres que se midieron dieron casi lo mismo, así que parecen del mismo modelo.
* **Resistencias de 10 kΩ**, con la cuarta banda dorada (tolerancia del 5 %). Una de ellas, medida en `20k`, dio
  9,65 kΩ. **No hay resistencias de 22 kΩ.**
* **Una protoboard** (la de 400 puntos) para montar el divisor. Con ella se midieron las LDR sin tocarlas con los
  dedos.
* **Los módulos KY-018** ya traen la resistencia fija, pero con ellos se pierde construir el divisor.

## Cómo se midió

1. Se pinchó cada LDR en una protoboard, para tocar sus patas con las puntas sin sujetarlas con los dedos. Tu cuerpo
   es una resistencia en paralelo de cientos de kΩ y falsea las lecturas altas.
2. Multímetro con el cable rojo en `VΩmA` y el negro en `COM`, en la zona `Ω` del selector.
3. **Rango `200k`** para las medidas de unas decenas de kΩ: tiene una precisión de ±0,8 % ±8 dígitos (unos ±1 kΩ).
   El rango `2000k` tiene ±2 % ±10 dígitos con 1 kΩ por dígito, es decir, unos **±10 kΩ**. Sirve para saber el orden
   de magnitud y para ver que algo pasa de 2 MΩ.
4. **Dos fuentes de luz:**
   * La luz natural de la sala, que varía con las nubes y no se puede repetir.
   * Una **luz de unas gafas con lupa**, apuntada a la LDR: es una referencia más estable que la luz de la sala.
5. **A oscuras:** en un cuarto sin luz, y esperando. La LDR tarda en "olvidar" la luz: justo después de taparla, su
   resistencia aún es baja y sube poco a poco (las hojas de datos miden a los 10 s).
6. **Lux con el móvil** (55 a 60 en la sala). El sensor del móvil es poco exacto, y no está en el mismo punto que la
   LDR: es un dato orientativo.

**Límite del multímetro:** el último rango de resistencia es de **2 MΩ**, no de 20 MΩ como dice la tabla del manual
(con 1999 cuentas y 1 kΩ de resolución, el fondo de escala es de unos 2 MΩ; coincide con la marca `2000k` del
selector). Una LDR a oscuras puede pasar de 2 MΩ, y entonces la pantalla muestra solo "1".

## Resultados

Tres LDR, casi idénticas entre sí (el 17/10/2026):

| Condición | Rango | Lectura | Resistencia |
|---|---|---|---|
| Luz de la sala (55 a 60 lux con el móvil) | `2000k` | 22 a 24 | ~23 kΩ (error de ese rango: ±10 kΩ) |
| Luz de unas gafas con lupa apuntando a la LDR | `200k` | 12,1 a 12,4 | ~12,2 kΩ (~170 lux, estimado) |
| Con un dedo encima | `200k` | 108,1 a 112,5 | ~110 kΩ (depende de cómo se tapa) |
| A oscuras (cuarto sin luz) | `2000k` | "1" | > 2 MΩ |

Antes de esto, con el multímetro en `2000k`, las lecturas con el dedo encima fueron de 100 a 120, que coinciden con las
de `200k`. La lectura de la luz de la sala en `200k` no se repitió: la luz natural cambia con las nubes.

**Estimación de lux de la luz de las gafas:** con la ley R ∝ lux^−0,6 y unos 65 kΩ con 10 lux, 12,2 kΩ equivale a
unos 170 lux. Es orientativo.

## Qué modelo parece ser

Valores aproximados de las hojas de datos de la serie GL55xx (de memoria; sin confirmar con la hoja de datos de cada
modelo):

| Modelo | Con 10 lux | A oscuras (mínimo) |
|---|---|---|
| GL5506 | 2–5 kΩ | 0,2 MΩ |
| GL5516 | 5–10 kΩ | 0,5 MΩ |
| GL5528 | 10–20 kΩ | 1 MΩ |
| GL5537-1 | 20–30 kΩ | 2 MΩ |
| GL5537-2 | 30–50 kΩ | 3 MΩ |
| GL5539 | 50–100 kΩ | 5 MΩ |
| GL5549 | 100–200 kΩ | 10 MΩ |

* **A oscuras (> 2 MΩ) no descarta ningún modelo:** la columna "a oscuras" de las hojas de datos suele ser un
  **mínimo**, y una LDR real puede pasar de él (sin confirmar).
* **El dato que sí distingue es el de la luz.** Con 57 lux y 23 kΩ, y una ley de R ∝ lux^−0,6, saldrían unos 65 kΩ
  con 10 lux: **GL5539 (o GL5537-2)**. Para que fuera una GL5528 (10 a 20 kΩ con 10 lux), la sala tendría que estar
  a unos 5 lux, y el móvil marcó 55 a 60.
* Es una estimación. Para el divisor no hace falta el modelo exacto: basta con el rango de resistencias medido.

## El divisor de tensión

Una LDR es una resistencia que **baja** cuando le da la luz. Con una resistencia fija forma un divisor de tensión:

```
5V ── LDR ──┬── Rf (10 kΩ) ── GND
            │
            └── entrada analógica (el "nodo")
```

* **Fórmula:** `V del nodo = 5,3 V × Rf / (R_LDR + Rf)`, con los 5,30 V reales del pin 5V del HAT.
* **Más luz → la LDR baja de resistencia → sube la tensión del nodo.** Es intuitivo para un niño: más luz, más
  voltios. Si se intercambian la LDR y Rf, ocurre al revés.
* **Cómo elegir Rf:** parecida a la resistencia de la LDR con la luz normal de la sala (unos 23 kΩ). Así, la tensión
  cambia mucho entre luz y sombra, y el nodo queda a mitad de escala con la luz habitual. Un valor ideal sería la
  media geométrica entre la resistencia con luz y a oscuras, pero a oscuras se pasa de 2 MΩ.
* **Calcular R_LDR a partir de la tensión:** `R_LDR = Rf × (V_alimentación / V_nodo − 1)`. Es lo que hace
  [LdrCheck](#el-programa-ldrcheck).

**Tensiones esperadas** (con 5,30 V en el pin 5V; valores calculados, no medidos con la lección):

| Condición | R de la LDR | Con Rf = 10 kΩ | Con Rf = 20 kΩ (2 × 10 kΩ) | Con Rf = 22 kΩ |
|---|---|---|---|---|
| A oscuras | > 2 MΩ | < 0,03 V | < 0,06 V | < 0,06 V |
| Dedo encima | ~110 kΩ | 0,44 V | 0,82 V | 0,88 V |
| Luz de la sala (55 a 60 lux) | ~23 kΩ | 1,61 V | 2,47 V | 2,59 V |
| Luz de las gafas (~170 lux) | ~12,2 kΩ | 2,39 V | 3,29 V | 3,41 V |

* **La resolución del conversor ADS1015 es de 3 mV por paso.** A oscuras, con 10 kΩ, el nodo marca menos de 0,03 V,
  es decir, menos de 10 pasos: se distingue de "con luz", pero poco de "casi a oscuras".
* **Con 10 kΩ** los cuatro niveles quedan separados por más de 0,4 V. **Con 20 o 22 kΩ** la sala queda cerca de la
  mitad de la escala. Cambiar la resistencia fija es un reto bueno para los niños: ver qué hace.
* **Dos resistencias de 10 kΩ en serie dan 20 kΩ** (no hay de 22 kΩ).

## Cómo conectarlo al robot

Con el robot **apagado**:

1. **Alimentación de la protoboard.** El cable 5V del divisor tiene que ir a un punto **unido al pin 5V del HAT**, por
   ejemplo, el nodo de 5V de la protoboard de la Pi. **Una protoboard no lleva alimentación por sí sola.** El 17/10/2026,
   el potenciómetro se quedó a 0,02 V porque su cable 5V iba a una fila sin unir al HAT.
2. **Prueba de continuidad** (multímetro en el zumbador, sin corriente): debe sonar entre la fila del 5V y el pin 5V
   del HAT, y entre la fila de GND y un pin GND del HAT. Entre la fila del 5V y la de GND no debe sonar.
3. **Un cable del nodo a una entrada analógica del HAT** (Analog 1 a 4). Las entradas admiten **de 0 a 5 V**. En el
   código, `hat.Analog.One` a `hat.Analog.Four` (ver `ExplorerHat.Common`).
4. **Síntomas de una entrada mal conectada:** sin nada conectado, una entrada flota y marca **unos 0,5 V**. Atada a
   masa, marca **unos 0,02 V** y no cambia. Pegada a 5V, **unos 5,3 V**.
5. Cambiar **solo la LDR con el robot encendido** no es peligroso: la resistencia fija limita la corriente a unos
   pocos mA como mucho.

Orden de conexión recomendado (si algo se conecta con la Pi encendida): GND, señal y, al final, 5V.

## Ideas de uso

* **Una LDR, una entrada:** la barra de la luz (las cuatro luces del HAT muestran el nivel) y la **luz de noche**
  (si la tensión baja de un valor, se encienden las luces). Es el diseño que se quitó de la lección 08.
* **Dos LDR, un vehículo de Braitenberg:** un robot que sigue una linterna o huye de ella. Cada LDR controla un motor.
  Prepara "sentir, pensar y actuar" de la Fase 7. Necesita dos entradas analógicas (Analog 1 y 2), y los motores con
  las ruedas en el aire para probarlo.
* **Un sensor de luz para el robot:** por ejemplo, para encender sus luces cuando se hace de noche.
* **Un reto de física:** medir la resistencia de una LDR con diferentes distancias de una linterna, y dibujar la
  curva.

## Texto para una lección (reutilizable)

Esta es la explicación que tenía la lección 08, adaptada. Sirve de base para una lección nueva.

> **La fotorresistencia (LDR).** Una fotorresistencia es una resistencia que **baja** cuando le da la luz. Con una
> resistencia fija forma un divisor de tensión, igual que el potenciómetro, pero lo cambia la luz y no tu mano.
> Más luz: la LDR baja de resistencia y sube la tensión del nodo (el punto entre las dos). La tensión es
> `5,3 V × 10 kΩ / (R de la LDR + 10 kΩ)`.
>
> ⚠️ **Cuidado con la protoboard:** el cable 5V del divisor tiene que ir a un punto unido al pin 5V del HAT. Una
> fila suelta no lleva alimentación. Con el robot apagado, comprueba con el zumbador del multímetro que esa fila y el
> pin 5V están unidos.

**Cambios en el programa de la lección 08** para usar la LDR (la lección 08 usaba `hat.Analog.One` para el
potenciómetro):

* Conectar el divisor a **Analog 2** y cambiar `hat.Analog.One` por `hat.Analog.Two`.
* Probar `maxVolts = 2.5` (con 10 kΩ, la luz habitual queda en 1,6 V y las luces fuertes en 2,4 V).

**Retos que tenía la lección 08 con la LDR:**

* **La barra de la luz.** Pon la mano encima y quítala: ¿cuántas luces se encienden en la sala?
* **La luz de noche.** Quita la barra. Si la tensión baja de un valor que elijas (`darkVolts`), enciende las cuatro
  luces. Si no, apágalas. ¿Con qué valor se enciende al tapar la LDR con la mano y no se enciende en la sala?

El README completo de la lección 08, con la sección de la LDR, está en el historial de git:
`git show 9bc0066:lessons/Lesson08.Analog/README.md` (el commit anterior a quitarla).

## El programa LdrCheck

[`docs/ldr/LdrCheck`](ldr/LdrCheck/Program.cs) mide la resistencia de LDR con el **ADS1015 del HAT**, sin
multímetro. Se escribió el 10/10/2026, antes de tener multímetro. Se probó sin nada conectado y no se llegó a usar con
LDR (después llegó el multímetro). **No está en la solución** (`ExplorerHatSandbox.slnx`): es un programa auxiliar
suelto.

* **Montaje:** `5V del HAT → LDR → nodo → 10 kΩ → GND`, y el nodo a **Analog 1**. Opcional: `Analog 2 → 10 kΩ → 5V`
  para medir la alimentación en lugar de suponer 5,30 V.
* **Qué hace:** para cada LDR pide un nombre y los lux del móvil (opcional), y hace tres medidas de 2 s que empiezan
  con Enter: luz de la sala, con un dedo encima y a oscuras (espera 10 s). Al final muestra una tabla. Cada lectura
  usa la escala más pequeña del ADS1015 que admite la tensión (hasta ±0,256 V, con 0,125 mV por paso): así mide bien
  varios MΩ. No toca ningún pin ni los motores.
* **Cómo ejecutarlo:** desplegarlo como cualquier programa suelto, por ejemplo:

```bash
dotnet publish docs/ldr/LdrCheck -c Release -r linux-arm64 --self-contained false
# copiar la carpeta publish a ~/apps/LdrCheck en la Pi, y en su terminal:
cd ~/apps/LdrCheck && dotnet LdrCheck.dll | tee ~/ldr.log
```

(El resultado va a `~`, no a `/tmp`: en la Pi, `/tmp` es `tmpfs` y se borra al reiniciar.)

## Dónde está el resto y qué falta por saber

* **Las decisiones y el historial:** `PLAN.md`, Fase 5, lección 08 ("Decisión del usuario (17/10/2026)" y los
  resultados de las medidas).
* **El multímetro:** [multimetro-DT830B.md](multimetro-DT830B.md) (manual en castellano y en inglés).
* **Qué falta por saber:**
  * El modelo exacto de la LDR (se podría comparar con la hoja de datos de GL5539 y GL5537-2).
  * Las otras 17 LDR de la bolsa: no se han medido. Pueden ser de otro modelo.
  * La lectura de la sala con `200k` y una luz fija: se usó la luz de las gafas porque la natural varía.
  * Las tensiones de la tabla son cálculos: no se han leído con el programa de la lección.
