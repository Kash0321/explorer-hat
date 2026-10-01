# Panel de la UPS (UpsDashboard)

Muestra en la consola, en tiempo real, todo lo que la Waveshare UPS HAT (B) puede contar sobre sus dos
baterías 18650, y el estado de alimentación de la Raspberry Pi. **No mueve los motores** y **solo lee**:
no cambia la configuración del chip.

## Qué hardware usa

* **La UPS HAT (B)**, montada debajo de la Raspberry Pi. Su única conexión de datos es un chip **INA219**
  en el bus **I2C 1**, dirección **0x42**. Mide la tensión de las baterías y la tensión en una resistencia
  de 0,1 Ω, de donde sale la corriente.
* **I2C habilitado** en la Pi (`/dev/i2c-1`). Puedes comprobar que el chip responde con `i2cdetect -y 1`:
  deben aparecer 0x28, 0x42 y 0x48 (por SSH sin sesión interactiva, `i2cdetect` está en `/usr/sbin`).
* No usa los pines GPIO del Explorer HAT ni sensores.

## Cómo ejecutarlo

**En la Raspberry Pi:**

```bash
dotnet run --project src/ExplorerHat.UpsDashboard/ExplorerHat.UpsDashboard.csproj
```

**Desde el PC con VS Code:** `Terminal > Run Task... > Ejecutar en la Pi` y elige `ExplorerHat.UpsDashboard`
(preparación en el [README del repositorio](../../README.md)).

Pulsa **Ctrl+C** para salir; la conexión I2C se libera antes de terminar. No mueve nada, así que no hace
falta poner el robot con las ruedas en el aire.

Para comparar la tensión con el robot en marcha, ejecuta el panel en una terminal y `ObstacleAvoidance`
en otra (con las ruedas en el aire). Para registrar las bajadas de tensión durante una prueba, usa
`tools/vigilar-tension.sh`.

## Qué se ve

El panel se actualiza dos veces por segundo, con tres tablas.

**Waveshare UPS HAT (B)**

| Dato | Qué significa |
|---|---|
| Baterías (2×18650) | Tensión de las dos baterías juntas, en voltios |
| Carga estimada | Barra y porcentaje, calculados solo con la tensión: 6,0 V = 0 % y 8,4 V = 100 % (los valores del ejemplo de Waveshare). Es una estimación |
| Corriente | En amperios y **con signo**: positiva, las baterías se cargan; negativa, se descargan |
| Potencia | Tensión por corriente, en vatios |
| Estado | `Cargando` (más de 0,02 A), `Descargando` (menos de −0,02 A: la Pi funciona con las baterías) o casi sin corriente (cargador conectado y baterías llenas) |
| Tensión mínima de la sesión | La tensión más baja vista desde que se abrió el panel |
| Descarga máxima de la sesión | La mayor corriente de descarga vista desde que se abrió el panel |

Si el chip no responde, la tabla muestra `La UPS no responde` y el mensaje de error. El panel sigue
intentándolo.

**Registros del INA219 (datos en bruto)**

| Registro | Valor mostrado |
|---|---|
| `0x00` Configuración | El valor en hexadecimal (rango, ganancia y muestras del chip) |
| `0x01` Tensión en la resistencia | El número con signo y los milivoltios que equivale a (10 µV por unidad) sobre 0,1 Ω |
| `0x02` Tensión de las baterías | El valor en hexadecimal, la tensión (4 mV por unidad), si hay una lectura nueva y si hay desbordamiento |

**Raspberry Pi**

* Los cuatro primeros bits de `vcgencmd get_throttled`, cada uno **ahora** y **desde el arranque**:
  tensión baja, frecuencia limitada, CPU ralentizada y límite de temperatura. Un `✔ no` en verde es lo
  normal. El valor completo sale en hexadecimal (`0x0` es lo normal: todo bien).
* Temperatura de la CPU.
* Si `vcgencmd` o la temperatura no están disponibles, lo indica.

## Por qué no usa el binding `Ina219` de Iot.Device.Bindings

El binding `Ina219` de la versión 4.2.0 lee la tensión de la resistencia y la corriente como números
**sin signo**. Cuando las baterías se descargan, la corriente es negativa y el binding da valores
enormes. `UpsHat` lee los registros con `I2cDevice` y convierte a `short` (con signo). Es un fallo del
binding pendiente de arreglar (ver la Fase 6 de `PLAN.md`).

## Qué mirar con el robot

* En reposo, con la Pi sola, la corriente es de unos −0,4 A (descargando) si el cargador de la UPS está
  desconectado.
* Con los motores en marcha sube la descarga. En las pruebas, con `ObstacleAvoidance`, la tensión mínima
  fue de 7,7 V y la corriente máxima de 1,1 A.
* El panel da por vacías las baterías a 6,0 V (3,0 V cada una). Conecta el cargador antes de llegar ahí: las
  baterías de litio duran más si no se descargan del todo.

## Ideas para el taller

* ¿Cuánta corriente gasta el robot parado? ¿Y con los motores en marcha?
* Cambia `REFRESH_TIME` para que el panel se actualice más despacio o más deprisa.
* Cambia los límites de colores de `ChargeBar`: ahora es rojo por debajo del 20 %, amarillo por debajo del 50 % y verde por encima.
