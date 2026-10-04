# 🤖 Proyecto Robot Educativo - Explorer Hat & .NET 10

¡Bienvenidos al repositorio del robot educativo basado en **Raspberry Pi 3 Model B+** y el complemento **Pimoroni Explorer Hat Pro**! Este proyecto está diseñado para que los niños aprendan lógica de programación, electrónica básica y robótica utilizando **C# y .NET 10 (LTS)**.

> 💡 **Nota Histórica:** Este proyecto utiliza las librerías oficiales `System.Device.Gpio` e `Iot.Device.Bindings`, las cuales incluyen soporte nativo para el Explorer Hat gracias a las contribuciones directas realizadas en el repositorio oficial de Microsoft .NET IoT.

---

## 🛠️ Requisitos del Sistema (Hardware & Software)

* **Placa:** Raspberry Pi 3 Model B+ (ARM64 de 64 bits).
* **OS:** Raspberry Pi OS 64-bit (Debian Moderno).
* **Hardware IoT:** Pimoroni Explorer Hat Pro (Conexión I2C habilitada).
* **Alimentación:** Waveshare UPS HAT (B) con dos baterías 18650, montada debajo de la Raspberry Pi. Las baterías externas USB y los cables micro-USB malos provocan bajadas de tensión al mover los motores (comparativa en `PLAN.md`, Fase 2).
* **Entorno:** .NET SDK 10.0.401 instalado localmente en la Raspberry Pi.

---

## 🔩 Montaje del robot

### Piezas

* **Raspberry Pi 3 Model B+**: el cerebro del robot.
* **Waveshare UPS HAT (B)** con dos baterías 18650: la alimentación. Va **debajo** de la Raspberry Pi.
* **Pimoroni Explorer HAT Pro**: va **encima** de la Raspberry Pi. Maneja los motores, las luces y las entradas y salidas.
* **Dos motores TT amarillos** con reductora 1:48. El motor **One** es la rueda **derecha** y el motor **Two** es la rueda **izquierda**.
* **Tres sensores de distancia HC-SR04P** (ultrasonidos): izquierda, centro y derecha. El HC-SR04P es la versión del HC-SR04 que funciona con cualquier tensión de 3 a 5,5 V. En el resto del documento los llamamos HC-SR04.
* **Dos discos de 20 ranuras**, uno en cada motor. Servirán para contar vueltas (lección 11 de `PLAN.md`).
* **Chasis** con soportes para los motores.

### Pasos

1. Monta los motores en los soportes del chasis, con los discos de 20 ranuras en sus ejes.
2. Pon la UPS HAT (B) **debajo** de la Raspberry Pi, con el montaje que trae. Sus contactos tocan por debajo los pines de alimentación y de I2C de la Pi. No ocupan ningún GPIO del Explorer HAT.
3. Pon el Explorer HAT Pro **encima** de la Raspberry Pi, en los 40 pines.
4. Con su montaje, la UPS y la Pi forman un solo bloque. Ese bloque encaja en el hueco del chasis, entre los soportes de los motores, justo encima de dos ranuras. Atorníllalo por esas ranuras. Así no se mueve en los giros ni al frenar.
5. Conecta el motor de la rueda **derecha** a la borna **MOTOR 1** del HAT y el de la rueda **izquierda** a **MOTOR 2**. En este robot, cada motor tiene un cable azul y otro morado:

   | Borna | Motor | + | − |
   |---|---|---|---|
   | MOTOR 1 | rueda derecha | morado | azul |
   | MOTOR 2 | rueda izquierda | azul | morado |

   Un motor de corriente continua no se estropea si se conecta al revés: solo gira hacia el otro lado. Los dos motores van montados en espejo, así que el mismo color en el + no hace que las dos ruedas avancen. Comprueba el resultado con `bash tools/probar-cableado.sh` (con las ruedas en el aire): si una rueda gira al revés, intercambia los dos cables de ese motor; si gira la otra rueda, los motores están cambiados de borna.
6. Monta los tres sensores en la parte delantera del robot, cada uno con su soporte, mirando hacia delante: izquierda, centro y derecha.
7. Conecta los sensores como indica la tabla de cableado de abajo.
8. Pon el interruptor deslizante de la UPS en **OFF**. Después coloca las dos baterías 18650, respetando la polaridad (+ y −) que marca el soporte. Si colocas las baterías con el interruptor en ON, la placa se puede dañar por un cortocircuito.
9. Pulsa el pulsador de la UPS (*boot*). Activa el circuito de protección de las baterías. Hay que hacerlo cada vez que se colocan las baterías.
10. Pon el interruptor en **ON** para encender el robot.

> 🔋 **Alimentación:** con la UPS HAT (B) no hay bajadas de tensión, ni con los motores en marcha. Las baterías externas USB y los cables micro-USB malos sí las provocan. La comparativa está en `PLAN.md`, Fase 2.

---

## 🔌 Pinout y cableado

Esta tabla recoge qué va a cada conector del Explorer HAT Pro y a qué GPIO de la Raspberry Pi llega.

| Elemento | Conector del HAT | GPIO | Notas |
|---|---|---|---|
| Motor 1 (rueda derecha, `One`) | Borna MOTOR 1 | 19 (hacia delante) / 20 (hacia atrás) | PWM por software en el pin del sentido de giro (`HatMotor`) |
| Motor 2 (rueda izquierda, `Two`) | Borna MOTOR 2 | 21 (hacia delante) / 26 (hacia atrás) | PWM por software en el pin del sentido de giro (`HatMotor`) |
| Luz azul (`Lights.One`) | LED 1 de la placa | 4 | |
| Luz amarilla (`Lights.Two`) | LED 2 de la placa | 17 | |
| Luz roja (`Lights.Three`) | LED 3 de la placa | 27 | |
| Luz verde (`Lights.Four`) | LED 4 de la placa | 5 | |
| Sensor izquierdo, TRIG | OUT3 | 13 | Salida |
| Sensor izquierdo, ECHO | IN3 | 24 | Entrada (admite 5 V) |
| Sensor central, TRIG | OUT1 | 6 | Salida |
| Sensor central, ECHO | IN1 | 23 | Entrada (admite 5 V) |
| Sensor derecho, TRIG | OUT2 | 12 | Salida |
| Sensor derecho, ECHO | IN2 | 22 | Entrada (admite 5 V) |
| LED de la lección 05 (en la protoboard) | OUT4 | 16 | Salida. La parada de emergencia también la pone a nivel bajo |
| Sensor de velocidad LM393 de la rueda derecha (o el pulsador de la lección 05) | IN4 | 25 | Entrada, con pull-down dentro del HAT. Sensor a 5 V |
| Sensor de velocidad LM393 de la rueda izquierda | PWM (fila lateral) | 18 | Entrada de **3,3 V**: el sensor se alimenta desde el pin 3v3 |
| Pantalla LCD 2004 (adaptador I2C) | SDA y SCL (fila lateral) | 2 y 3 | Bus I2C, alimentada desde el pin 3v3 |

> ⚠️ **La fila lateral del HAT (marcada "3.3V ONLY")** tiene SDA, SCL, PWM, MOSI, MISO, SCK, CS, RX, TX y 3v3. Esos pines van directos a la Raspberry Pi, sin protección: **nunca les llegue más de 3,3 V**. Todo lo que se conecte a ellos se alimenta desde el pin 3v3, nunca desde 5V. En la protoboard pequeña del HAT, el nodo de 3,3 V está separado del de 5 V.

**Chips I2C** (bus `/dev/i2c-1`):

| Dirección | Chip | Para qué sirve |
|---|---|---|
| `0x28` | CAP1208 | Pads táctiles del Explorer HAT Pro |
| `0x42` | INA219 | Mide la tensión y la corriente de las baterías de la UPS HAT (B) |
| `0x48` | ADS1015 | Entradas analógicas del Explorer HAT Pro |
| `0x27` | PCF8574T | Adaptador I2C de la pantalla LCD 2004 (funciona a 3,3 V; el contraste se ajusta con su potenciómetro azul) |

> ℹ️ Cada sensor HC-SR04 tiene cuatro cables: **VCC** (alimentación), **TRIG**, **ECHO** y **GND** (masa). **VCC va al pin 5V del HAT** y **GND al pin GND del HAT**. TRIG va a una salida OUT del HAT y ECHO a una entrada IN. Todos los GND (Pi, HAT y sensores) tienen que estar unidos: es la *masa común*, la referencia de 0 V con la que se miden todas las señales.

> ⚠️ Los pines de la tabla ya tienen dueño. No conectes otro aparato a un pin que ya usa el robot.

---

## ⚡ Por qué los sensores van a las entradas del HAT

El sensor HC-SR04 funciona con **5 V**. Cuando mide, su pin **ECHO** responde con una señal de **5 V**.

Los pines GPIO de la Raspberry Pi solo aguantan **3,3 V**. Con 5 V se puede estropear la Pi.

Las entradas **IN1 a IN4** del Explorer HAT Pro sí **admiten 5 V**: protegen al GPIO de la Pi. Por eso **ECHO va siempre a una entrada IN** del HAT y nunca a un GPIO suelto.

El pin **TRIG** es al revés: la Pi envía el pulso al sensor. Va **directo** a una salida **OUT** del HAT, sin ningún componente en medio.

Las salidas OUT del HAT son de **colector abierto**: dentro tienen un transistor que funciona como un interruptor a masa. Activa, la salida une el cable a 0 V. Inactiva, deja el cable suelto: no lo pone a 5 V. Para que el cable suba a 5 V hace falta una **resistencia de pull-up** entre el cable y 5 V. En este robot no hay ninguna en el cableado y los sensores funcionan, así que es el propio módulo HC-SR04P el que sube su Trig. No se ve en la placa si lo hace con una resistencia o con la resistencia interna de su chip de control (U3, RCW9006). Se puede comprobar con un multímetro: con el robot encendido y sin ningún programa en marcha, entre Trig y Gnd debe haber unos 5 V.

Una curiosidad: con este montaje **la señal se invierte**. Cuando el programa pone el pin en alto, la salida se activa y el TRIG baja a 0 V. El sensor recibe un pulso bajo en lugar de uno alto, y aun así mide bien.

---

## 🔧 Habilitar I2C

El HAT (táctil y analógico) y la UPS hablan con la Raspberry Pi por el bus I2C. Hay que activarlo una sola vez.

1. Abre el archivo de configuración y añade (o descomenta) esta línea:
   ```bash
   sudo nano /boot/firmware/config.txt
   ```
   ```
   dtparam=i2c_arm=on
   ```
   También puedes activarlo con `sudo raspi-config` → *Interface Options* → *I2C*.
2. Reinicia la Raspberry Pi:
   ```bash
   sudo reboot
   ```
3. Comprueba que existe `/dev/i2c-1` y que se ven los chips:
   ```bash
   /usr/sbin/i2cdetect -y 1
   ```
   Deben aparecer **0x28**, **0x42** y **0x48**. (`i2cdetect` está en `/usr/sbin`, que no siempre está en el `PATH`, sobre todo por SSH.)

Si no aparece alguno, apaga la Raspberry Pi y revisa que el HAT y la UPS estén bien encajados.

---

## 🦺 Normas de seguridad en el taller

Un robot con motores puede hacerse daño y hacer daño. Estas normas valen para niños y monitores.

### Antes de ejecutar un programa

* **Prueba siempre con las ruedas en el aire.** Pon el robot sobre un bote o un libro, sin que las ruedas toquen nada.
* Esto vale sobre todo para programas **nuevos** y para depurar.
* Solo después de probarlo en el aire, déjalo en el suelo.
* **El monitor da el visto bueno** antes de mover el robot.
* Despeja la zona: sin cables por el suelo y sin objetos frágiles.

### Cómo parar el robot

1. **Ctrl+C** en el terminal: el programa para los motores antes de salir.
2. La **tecla del programa**: en ObstacleAvoidance, pulsa cualquier tecla para parar.
3. **Parada de emergencia**, en la Raspberry Pi:
   ```bash
   bash tools/parar-robot.sh
   ```
   Desde el PC también sirve la tarea de VS Code **Parar el robot**.
4. **Levanta el robot** del suelo y aparta los dedos de las ruedas.
5. Si nada funciona, **pon en OFF el interruptor deslizante** de la UPS HAT (B). Es un apagado de golpe: úsalo solo en una emergencia, porque puede dañar la tarjeta microSD. Para apagar normalmente, ejecuta `sudo poweroff`, espera a que el LED verde de la Pi deje de parpadear y después pon el interruptor en OFF.

### Si se corta la wifi

* El programa se para solo en unos **15 a 30 segundos**, si has hecho la preparación SSH de este README.
* Durante ese tiempo los motores **siguen girando**. Levanta el robot o pulsa la parada de emergencia.

### Depuración

* Un **punto de interrupción congela el programa** y los motores se quedan **parados o a toda velocidad**.
* Depura siempre con las ruedas en el aire.
* El botón de parar del depurador cierra el programa de golpe. Después se ejecuta sola la tarea *Parar el robot*.

### En el código

* Usa siempre `SafeExplorerHat` dentro de un bloque `using`.
* Termina cada movimiento con `Speed = 0.0` en los dos motores, también si hay una excepción.

### Baterías 18650

* **No las cortocircuites.** No pongas metal ni cables pelados sobre sus polos.
* Colócalas con la polaridad correcta y con el interruptor de la UPS en **OFF**.
* **No las dejes cargando sin vigilar.**
* No uses baterías hinchadas, abolladas o calientes. Avisa al monitor.
* Usa solo baterías que quepan en la UPS (el límite es de 67 mm de largo).
* Guárdalas fuera del robot si no se usan durante mucho tiempo.

### Cables y manos

* Revisa que **no haya cables sueltos** ni sin sujetar. Pueden engancharse en las ruedas.
* **Pelo largo recogido, y dedos y ropa lejos de las ruedas** mientras los motores están en marcha.
* No toques los pines ni las placas con el robot encendido.
* Apaga el robot y desconecta la alimentación antes de cambiar un cable.
* Si ves humo, huele a quemado o algo se calienta mucho: desconecta la alimentación y avisa al monitor.

### Aviso de tensión baja

* Un LED rojo **PWR** fijo en la Raspberry Pi indica que la tensión es correcta.
* Si el robot hace cosas raras, mira el estado de alimentación con `vcgencmd get_throttled` (`0x0` = bien).

---

## 🎓 Lecciones del taller (`lessons/`)

Itinerario para los niños, de lo más sencillo a lo más difícil: 01 Luces, 02 Semáforo, 03 Motores, 04 Cuadrado, 05 Botones, 06 Distancia, 09 Robot autónomo y 11 Contar vueltas (con sus partes Ir recto y El cuadrado perfecto) y 12 La pantalla.
Cada lección es un proyecto pequeño con un único `Program.cs` y un README con retos. Índice y forma de
ejecutarlas en [`lessons/README.md`](lessons/README.md).

---

## 📂 Estructura del Proyecto (`src/`)

El entorno está organizado en soluciones independientes según el nivel de aprendizaje:

* 🟢 **`ExplorerHat.BasicSample/`**: El laboratorio de inicio. Ideal para enseñar bucles, hilos con `Thread.Sleep`, encendido de luces LED secuenciales y movimientos básicos de los dos motores (Adelante, Atrás, Giros).
* ⚙️ **`ExplorerHat.Common/`**: Código compartido por los ejemplos y las lecciones. `SafeExplorerHat` se usa igual que `ExplorerHat`, pero al terminar para los motores, apaga las luces y libera los pines sin errores. Sus motores (`HatMotor`) giran igual de rápido hacia delante que hacia atrás. `Screen` maneja la pantalla LCD.
* 📡 **`ExplorerHat.SonarDashboard/`**: Panel en la consola con la distancia que mide cada sensor de ultrasonidos, sin mover los motores. Sirve para comprobar el montaje de los sensores. La tecla **F** activa o desactiva el mismo filtro de lecturas falsas que usa el robot autónomo. Con `--registro <archivo> [segundos]` no muestra el panel: guarda cada lectura en un archivo CSV para estudiar las lecturas falsas.
* 🔋 **`ExplorerHat.UpsDashboard/`**: Panel en la consola con los datos de la Waveshare UPS HAT (B): tensión y carga de las baterías, corriente, potencia y si se cargan o se descargan, junto con el estado de alimentación y la temperatura de la Raspberry Pi. No mueve los motores.
* 🔵 **`ExplorerHat.ObstacleAvoidance/`**: El robot autónomo. Integra lecturas de sensores de distancia por ultrasonidos (HC-SR04) para calcular proximidad y tomar decisiones de esquiva en tiempo real.

---

## 🧰 Herramientas (`tools/`)

Scripts para la Raspberry Pi: `parar-robot.sh` (parada de emergencia), `probar-cableado.sh` (comprueba todo el cableado paso a paso, después de desmontar el robot), `vigilar-pines.sh` (registra los pines de los motores y si hay un programa en marcha) y `vigilar-tension.sh` + `comparar-tension.sh` (registran y resumen las bajadas de tensión). Cada script explica su uso al principio.

---

## 🚀 Comandos Rápidos de Consola

Para compilar o ejecutar los proyectos directamente desde la terminal remota de la Raspberry Pi:

### Compilar la solución completa:
```bash
dotnet build ExplorerHatSandbox.slnx
```

### Ejecutar el laboratorio básico:
```bash
dotnet run --project src/ExplorerHat.BasicSample/ExplorerHat.BasicSample.csproj
```

### Ejecutar el robot autónomo:
```bash
dotnet run --project src/ExplorerHat.ObstacleAvoidance/ExplorerHat.ObstacleAvoidance.csproj
```

---

## 💻 Trabajar desde el PC con VS Code

El programa se compila en el PC, se copia a `~/apps` en la Raspberry Pi y se ejecuta allí por SSH. Funciona en Windows, macOS y Linux: solo hacen falta `ssh`, `scp` y `dotnet`.

### Preparación (una sola vez)

**En el PC:** VS Code con la extensión de C# y el SDK de .NET 10.

> ⚠️ **Versión de la extensión de C#:** desde la 2.160 la extensión envía al depurador de la Raspberry Pi comprobaciones de archivo (SHA384/SHA512) que este no admite, y los puntos de interrupción no funcionan aunque se vean en rojo ([dotnet/vscode-csharp#9802](https://github.com/dotnet/vscode-csharp/issues/9802)). Instala la **2.140.9** (en Extensiones, rueda ⚙ de *C#* → *Descargar VSIX de versión específica*, o desde las [versiones del repositorio](https://github.com/dotnet/vscode-csharp/releases), archivo `csharp-win32-x64-2.140.9.vsix`; después *Instalar desde VSIX...*), desactiva su actualización automática y reinicia las extensiones cuando VS Code lo pida.

> ⚠️ **Deshabilita C# Dev Kit en este repositorio.** Con la extensión de C# 2.140.9, C# Dev Kit (probado con la 3.40.210) deja el depurador parado en el primer punto de interrupción: no responde a F10 (paso a paso) ni a F5 (continuar). En Extensiones, rueda ⚙ de *C# Dev Kit* → **Deshabilitar (área de trabajo)**. Comprueba después que la extensión **C#** sigue habilitada y recarga la ventana (*Developer: Reload Window*). Si VS Code dice que el tipo de depuración `coreclr` no es compatible, es que la extensión C# se ha deshabilitado también: no instales nada, solo vuelve a habilitarla.

**En la Raspberry Pi:** el depurador de Visual Studio en `~/vsdbg`:
```bash
curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
```

**Entrar por SSH sin contraseña.** En el PC (PowerShell en Windows):
1. Crea una clave (si ya tienes una en `~/.ssh/id_ed25519`, sáltate este paso):
   ```powershell
   ssh-keygen -t ed25519
   ```
2. Copia la clave pública a la Raspberry Pi (pedirá la contraseña de `pi` por última vez):
   ```powershell
   type $env:USERPROFILE\.ssh\id_ed25519.pub | ssh pi@harlequin.local "cat >> ~/.ssh/authorized_keys"
   ```
   En macOS o Linux: `ssh-copy-id pi@harlequin.local`.
3. Da un nombre a la Raspberry Pi en `~/.ssh/config` (en Windows, `%USERPROFILE%\.ssh\config`):
   ```
   Host harlequin
       HostName harlequin.local
       User pi
   ```
4. Comprueba que `ssh harlequin` entra sin pedir contraseña.
5. *(Opcional, recomendado)* Si cada conexión tarda varios segundos, es Windows buscando `harlequin.local` en la red. Reserva una IP fija para la Raspberry Pi en el router (reserva DHCP para la MAC de su wifi) y pon esa IP en `HostName`. En nuestra prueba, cada conexión pasó de 4,6 s a 0,5 s. Si llevas la Pi a otra red, vuelve a `harlequin.local`.

Si tu Raspberry Pi tiene otro nombre, cámbialo en `.vscode/settings.json` (`explorerHat.host`).

**Parar el robot si se corta la wifi** (en la Raspberry Pi, una sola vez). Si se pierde la conexión SSH, la Pi tarda mucho en darse cuenta y el programa sigue con los motores en marcha. Con esto cierra la sesión en unos 15–30 s y el programa para los motores:
```bash
printf 'ClientAliveInterval 5\nClientAliveCountMax 3\n' | sudo tee /etc/ssh/sshd_config.d/10-explorerhat.conf
sudo systemctl reload ssh
printf 'net.ipv4.tcp_retries2 = 6\n' | sudo tee /etc/sysctl.d/90-explorerhat.conf
sudo sysctl --system
```

### Ejecutar y depurar

* **Ejecutar:** `Terminal > Run Task... > Ejecutar en la Pi` y elige el programa. Para las lecciones, *Ejecutar una lección en la Pi*. Para pararlo, **Ctrl+C** en el terminal: el programa para los motores antes de salir.
* **Depurar BasicSample:** pestaña *Run and Debug*, configuración **Depurar BasicSample en la Pi** y F5.
* **Depurar programas que leen el teclado** (como ObstacleAvoidance): primero *Ejecutar en la Pi*, después **Adjuntar a un programa en la Pi** y elige el proceso `dotnet` del programa.
* **Parar el robot:** la tarea **Parar el robot** para los programas del robot y pone a nivel bajo los pines de motores y luces, y la salida OUT4.

> ⚠️ **Puntos de interrupción y motores:** la velocidad de los motores se controla desde el propio programa (PWM por software). Cuando el depurador se detiene en un punto de interrupción congela todo el programa, y cada motor se queda como estuviera en ese instante: **parado o a toda velocidad**. Depura siempre con las ruedas en el aire.

> ⚠️ **Seguridad:** el botón de parar del depurador cierra el programa de golpe, sin que pueda parar los motores. Por eso, al terminar cada depuración se ejecuta sola la tarea *Parar el robot*. Aun así, prueba siempre los programas nuevos con las ruedas en el aire.
