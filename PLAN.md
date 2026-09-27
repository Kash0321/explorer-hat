# Plan de trabajo: puesta al día y ampliación

Objetivo: recuperar el repositorio para volver a hacer talleres con niños (programación, electrónica
y robótica) con Raspberry Pi 3 B+, Pimoroni Explorer HAT Pro y .NET 10, y revisar/ampliar el binding
`Iot.Device.ExplorerHat` de dotnet/iot.

Leyenda: `[ ]` pendiente · `[x]` hecho · `[~]` en curso

## Fase 0: Preparar la Raspberry Pi y el repositorio
- [x] Habilitar I2C (`dtparam=i2c_arm=on` en `/boot/firmware/config.txt`; ahora existe `/dev/i2c-1`).
      Hace falta para los pads táctiles (CAP1208) y las entradas analógicas (ADS1015) del HAT.
- [x] Comprobar con `i2cdetect -y 1` que aparecen los chips del HAT: 0x28 (táctil) y 0x48 (analógico).
- [x] Hacer que `dotnet` funcione en sesiones SSH no interactivas: `~/.bashrc` solo lo añade al `PATH`
      en shells interactivas, así que se creó el enlace `/usr/local/bin/dotnet -> /home/pi/.dotnet/dotnet`.
- [x] Confirmar (commit) el README.md reescrito, el AGENTS.md nuevo y este PLAN.md.

## Fase 1: Migración a .NET 10
- [ ] `Directory.Build.props` común: `net10.0`, `Nullable`, `ImplicitUsings`, `LangVersion` por defecto.
- [ ] Actualizar paquetes: `Iot.Device.Bindings` y `System.Device.Gpio` 4.2.0, Serilog 4.x
      (BasicSample está en `netcoreapp3.1` + Iot 1.1.0; ObstacleAvoidance en `net6.0` + Iot 2.2.0).
- [ ] Corregir cambios de API (p. ej. `Hcsr04.Distance` → `TryGetDistance`).
- [ ] Valorar migrar `ExplorerHatSandbox.sln` a `.slnx`.
- [ ] Compilar en la Pi y probar en hardware con las ruedas en el aire.

## Fase 2: Seguridad física del robot (parada de emergencia)
- [ ] Ctrl+C (`Console.CancelKeyPress`) y excepciones deben dejar los dos motores a `Speed = 0` y liberar pines.
- [ ] ObstacleAvoidance: `Main` termina sin esperar a la tarea del `Runner`, así que el `using` del HAT
      puede no llegar a ejecutarse y los motores quedar encendidos. Esperar a la tarea antes de salir.
- [ ] ObstacleAvoidance: los bucles de giro (`while (... <= 20d)`) ignoran la orden de parada y pueden no terminar.
- [ ] Sonar: las lecturas del temporizador se pueden solapar (el `lock` está comentado) y `_running` no es `volatile`.

## Fase 3: Despliegue y ejecución (sustituir `.vscode/`)
Qué hacía lo antiguo: desde un PC Windows, `publish.bat` publicaba para `linux-arm` y copiaba el
resultado con `pscp` (PuTTY, contraseña en claro) a `~/Work/dotnet/explorerhat`; `republish` copiaba
también las dependencias. Las plantillas de `launch.json`/`tasks.json` lanzaban la DLL por `plink`
como `root` y depuraban con `vsdbg`. Hoy no funcionaría: la Pi es de 64 bits, las rutas apuntan a
`netcoreapp3.1`/`net6.0`, `tasks.json` apunta a un proyecto inexistente (`samples/ExplorerHat.LightingSample`)
y solo servía en Windows.

Propuesta, dos formas de trabajar:
- **En la propia Pi** (teclado/pantalla o terminal SSH): `dotnet run --project ...`. La más sencilla para el taller.
- **Desde el PC con VS Code** (Windows, macOS o Linux), usando el OpenSSH que ya traen los tres sistemas:
  - [ ] Autenticación SSH con clave (`ssh-keygen` + copiar la clave pública a la Pi); nada de contraseñas.
  - [ ] `tasks.json`: `dotnet publish -r linux-arm64 --self-contained false` en el PC → `scp` a `~/apps/<Proyecto>`
        → `ssh -t pi@<host> dotnet ~/apps/<Proyecto>/<Proyecto>.dll`. El nombre del host, configurable (sin datos personales).
  - [ ] `launch.json` para depurar con F5: `pipeTransport` con `ssh` y `vsdbg` instalado en `~/vsdbg` de la Pi.
  - [ ] Ejecutar como `pi`, no como `root` (el usuario ya está en los grupos `gpio` e `i2c`).
  - [ ] Seguridad: con `ssh -t`, Ctrl+C llega al programa y se paran los motores (depende de la Fase 2).
        Sin `-t`, cerrar el terminal puede dejar el programa corriendo en la Pi con los motores en marcha.
  - [ ] Versionar `launch.json`/`tasks.json` directamente (quitarlos de `.gitignore`) y borrar los scripts y plantillas antiguos.
- Descartado como opción principal: VS Code Remote-SSH ejecutándose en la Pi (1 GB de RAM se queda corto
  con la extensión de C#).
- [ ] Guía de compilación ligera para 1 GB de RAM y ~3,5 GB libres en la microSD.

## Fase 4: Documentación
- [ ] README: montaje, cableado de los sensores HC-SR04 (niveles de 5 V → entradas del HAT), pinout,
      habilitar I2C, normas de seguridad en el taller.
- [ ] README propio de cada ejemplo (el de BasicSample está vacío).
- [ ] Corregir AGENTS.md (espacio libre real, desarrollo en la propia Pi, requisito de I2C).
- [ ] Decidir si hace falta CLAUDE.md (Claude Code ya lee AGENTS.md).

## Fase 5: Itinerario didáctico (nuevos ejemplos graduados)
Propuesta; cada lección es un proyecto pequeño con un único `Program.cs` legible por niños.
- [ ] 01 Luces: encender y apagar LEDs, bucles.
- [ ] 02 Semáforo: secuencias y tiempos con `Thread.Sleep`.
- [ ] 03 Motores: adelante, atrás, girar.
- [ ] 04 Dibujar un cuadrado: bucles con movimiento.
- [ ] 05 Botones: entradas digitales (condiciones `if`).
- [ ] 06 Distancia: un sensor HC-SR04, frenar ante un obstáculo.
- [ ] 07 Pads táctiles: control remoto del robot (requiere Fase 6).
- [ ] 08 Sensores analógicos: luz o potenciómetro (requiere Fase 6).
- [ ] 09 Robot autónomo: versión simplificada de ObstacleAvoidance.

## Fase 6: Binding `Iot.Device.ExplorerHat` en dotnet/iot
Estado: el binding sigue en el repositorio (activo, último cambio en el binding en julio de 2026), pero
solo cubre motores y las 4 luces. Falta:
- [ ] 4 entradas digitales (GPIO 23, 22, 24, 25, tolerantes a 5 V).
- [ ] 4 salidas de colector abierto (GPIO 6, 12, 13, 16).
- [ ] 4 entradas analógicas (ADS1015, I2C 0x48). Comprobar si sirve el binding `Ads1115` existente.
- [ ] 8 pads táctiles capacitivos (CAP1208, I2C 0x28). No hay binding CAP1xxx en dotnet/iot.
- [ ] Revisar posibles fallos: `Motors`, `Lights` y `Led` reciben el `GpioController` compartido con
      `shouldDispose = true` y lo liberan varias veces; comprobar el control del DRV8833 marcha atrás.
- [ ] Preparar PR(s) a dotnet/iot.
