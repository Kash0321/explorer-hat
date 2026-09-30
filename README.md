# 🤖 Proyecto Robot Educativo - Explorer Hat & .NET 10

¡Bienvenidos al repositorio del robot educativo basado en **Raspberry Pi 3 Model B+** y el complemento **Pimoroni Explorer Hat Pro**! Este proyecto está diseñado para que los niños aprendan lógica de programación, electrónica básica y robótica utilizando **C# y .NET 10 (LTS)**.

> 💡 **Nota Histórica:** Este proyecto utiliza las librerías oficiales `System.Device.Gpio` e `Iot.Device.Bindings`, las cuales incluyen soporte nativo para el Explorer Hat gracias a las contribuciones directas realizadas en el repositorio oficial de Microsoft .NET IoT.

---

## 🛠️ Requisitos del Sistema (Hardware & Software)

* **Placa:** Raspberry Pi 3 Model B+ (ARM64 de 64 bits).
* **OS:** Raspberry Pi OS 64-bit (Debian Moderno).
* **Hardware IoT:** Pimoroni Explorer Hat Pro (Conexión I2C habilitada).
* **Entorno:** .NET SDK 10.0.401 instalado localmente en la Raspberry Pi.

---

## 📂 Estructura del Proyecto (`src/`)

El entorno está organizado en soluciones independientes según el nivel de aprendizaje:

* 🟢 **`ExplorerHat.BasicSample/`**: El laboratorio de inicio. Ideal para enseñar bucles, hilos con `Thread.Sleep`, encendido de luces LED secuenciales y movimientos básicos de los dos motores (Adelante, Atrás, Giros).
* ⚙️ **`ExplorerHat.Common/`**: Código compartido por los ejemplos. `SafeExplorerHat` se usa igual que `ExplorerHat`, pero al terminar para los motores, apaga las luces y libera los pines sin errores.
* 📡 **`ExplorerHat.SonarDashboard/`**: Panel en la consola con la distancia que mide cada sensor de ultrasonidos, sin mover los motores. Sirve para comprobar el montaje de los sensores. La tecla **F** activa o desactiva el mismo filtro de lecturas falsas que usa el robot autónomo. Con `--registro <archivo> [segundos]` no muestra el panel: guarda cada lectura en un archivo CSV para estudiar las lecturas falsas.
* 🔵 **`ExplorerHat.ObstacleAvoidance/`**: El robot autónomo. Integra lecturas de sensores de distancia por ultrasonidos (HC-SR04) para calcular proximidad y tomar decisiones de esquiva en tiempo real.

---

## 🧰 Herramientas (`tools/`)

Scripts para la Raspberry Pi: `parar-robot.sh` (parada de emergencia), `vigilar-pines.sh` (registra los pines de los motores y si hay un programa en marcha) y `vigilar-tension.sh` + `comparar-tension.sh` (registran y resumen las bajadas de tensión). Cada script explica su uso al principio.

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

* **Ejecutar:** `Terminal > Run Task... > Ejecutar en la Pi` y elige el programa. Para pararlo, **Ctrl+C** en el terminal: el programa para los motores antes de salir.
* **Depurar BasicSample:** pestaña *Run and Debug*, configuración **Depurar BasicSample en la Pi** y F5.
* **Depurar programas que leen el teclado** (como ObstacleAvoidance): primero *Ejecutar en la Pi*, después **Adjuntar a un programa en la Pi** y elige el proceso `dotnet` del programa.
* **Parar el robot:** la tarea **Parar el robot** para los programas del robot y pone a nivel bajo los pines de motores y luces.

> ⚠️ **Puntos de interrupción y motores:** la velocidad de los motores se controla desde el propio programa (PWM por software). Cuando el depurador se detiene en un punto de interrupción congela todo el programa, y cada motor se queda como estuviera en ese instante: **parado o a toda velocidad**. Depura siempre con las ruedas en el aire.

> ⚠️ **Seguridad:** el botón de parar del depurador cierra el programa de golpe, sin que pueda parar los motores. Por eso, al terminar cada depuración se ejecuta sola la tarea *Parar el robot*. Aun así, prueba siempre los programas nuevos con las ruedas en el aire.
