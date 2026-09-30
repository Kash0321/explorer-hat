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
- [x] `Directory.Build.props` común: `net10.0`, `ImplicitUsings`, `Nullable`.
- [x] Actualizar paquetes: `Iot.Device.Bindings` y `System.Device.Gpio` 4.2.0, Serilog 4.4.0,
      Serilog.Sinks.Console 6.1.1.
- [x] Cambios de API: no hubo que tocar nada (`Hcsr04.Distance` sigue existiendo y lanza excepción si no
      hay eco; `DCMotor` controla bien el DRV8833 marcha atrás). Solo avisos de nulabilidad, corregidos.
- [x] Compila en la Pi: 0 errores, 0 avisos (primera compilación ~3 min, siguientes ~20 s).
- [x] Prueba de LEDs en hardware como usuario `pi` (sin `root`): funciona.
- [x] **Fallo al liberar el HAT** (ver Fase 6): `ExplorerHat.Dispose()` hace que el proceso muera con
      `InvalidOperationException: Can not write to pin 19 because it is not open` desde `SoftwarePwmChannel.Run()`.
      Solución provisional: proyecto `src/ExplorerHat.Common` con `SafeExplorerHat` (para motores, apaga luces
      y libera los pines en orden). Los dos ejemplos lo usan. Retirarla cuando se publique el arreglo en dotnet/iot.
- [x] Probar motores con las ruedas en el aire (BasicSample): sentidos, parada final y salida limpia correctos.
- [x] Migrar `ExplorerHatSandbox.sln` a `ExplorerHatSandbox.slnx` (formato XML simple, .NET 10 / VS 2022 17.13+).

## Fase 2: Seguridad física del robot (parada de emergencia)
- [x] Comprobado en .NET 10: ante Ctrl+C (SIGINT), `kill` (SIGTERM) o cierre de SSH (SIGHUP) el proceso termina
      **sin ejecutar `using` ni `finally`**, y los pines se quedan como estaban (prueba de control con el
      `ExplorerHat` original: LEDs encendidos tras Ctrl+C). Con una excepción no capturada el `using` sí se ejecuta.
- [x] `SafeExplorerHat` registra esas señales (y SIGQUIT) con `PosixSignalRegistration` y se libera antes de terminar:
      motores a `Speed = 0`, luces apagadas y pines liberados. Probado con LEDs y `pinctrl`: todos los pines a nivel bajo.
- [x] ObstacleAvoidance: `Main` espera a la tarea del `Runner` antes de salir.
- [x] ObstacleAvoidance: los bucles de giro atienden la orden de parada y no vuelve a avanzar tras ella.
- [x] Sonar: una medición cada vez (`AutoReset = false` + `lock`), `Dispose` protegido y `_running` es `volatile`.
- [x] AGENTS.md: la regla de liberación de pines pide usar `SafeExplorerHat`.
- [x] Condición de carrera: el programa sigue ejecutándose durante la parada y puede volver a escribir un pin
      justo antes de liberarlo. Arreglo: tras liberar, `SafeExplorerHat` fuerza a nivel bajo los 8 pines de salida.
      Prueba de estrés con LEDs (30 paradas aleatorias): sin arreglo 30/30 pines en alto; con arreglo 0/30.
- [x] Parada de emergencia con motores girando (ruedas en el aire, motor 2 marcha atrás) con SIGINT, SIGTERM
      y SIGHUP: pines de motores y LEDs a nivel bajo en los tres casos.
- [x] Nuevo ejemplo `ExplorerHat.SonarDashboard`: panel de consola (Spectre.Console) con la distancia de cada
      sensor, sin motores. Montaje verificado: los tres sensores responden (Izquierda 20/20, Centro 17/20, Derecha 20/20).
- [x] Probar ObstacleAvoidance en hardware con los tres sensores HC-SR04 (robot sin cables, con batería y wifi).
      Arranca, esquiva y para bien, pero choca con algunos obstáculos. Con batería hay bajadas de tensión
      (`Undervoltage detected`, ~12 s en la prueba) al mover los motores; la wifi aguanta (2/86 pings perdidos).
- [ ] Mejorar la alimentación (separar la de la Pi y la de los motores o batería de más amperios).
      Con cargador (5 V, 3 A) la causa de las bajadas de tensión eran los cables micro-USB: con tres
      combinaciones de cargador y cable había `Undervoltage` constante, incluso sin el HAT. Con un cable bueno:
      `throttled=0x0` en reposo, con la CPU al 100 % y con los motores en marcha (BasicSample, ruedas en el aire).
      El LED rojo PWR fijo indica que la tensión es correcta. Falta revisar la batería (bajadas al mover los motores).
      Pruebas con ObstacleAvoidance (ruedas en el aire, tensión cada 0,2 s), arranque de golpe (N) / progresivo (S):
      - Placa de 2×18650 en paralelo con elevador (DIY MORE V8): bajadas de tensión incluso sin motores
        (arranque de la Pi); con motores, tensión baja el 98 % (N) / 92 % (S) del tiempo.
      - Batería externa Redmi 10000 mAh (5,1 V 2,4 A): sin avisos en reposo ni compilando; con motores 68 % (N) /
        54 % (S), 6 caídas en ambos casos, una por cada salida o maniobra (marcha atrás y giro).
      - Batería externa Xiaomi Mi Power Bank 2 PLM10ZM (5000 mAh, 5,1 V 2,1 A), con poca carga (~3 h de uso):
        caídas sueltas incluso en reposo (4 en 4 min); con motores 97 % (N) / 91 % (S). Descartada para motores
        salvo que con carga completa mejore mucho (pendiente repetir). Para trabajar, de momento, la Redmi.
      - El arranque progresivo no evita las caídas; se queda como opción (tecla S al iniciar) por estética.
      - Propuesta: Waveshare UPS HAT (B) (2×18650 en serie + reductor, 5 V hasta 5 A, contactos por debajo de la
        Pi sin usar el GPIO, INA219 en I2C 0x42). Las Samsung 25R (64,9 mm) caben (límite 67 mm).
        Descartadas: UPS HAT original (2,5 A) y Geekworm X728 (usa los GPIO 5, 6, 12, 16 y 20 del Explorer HAT).
- [x] Revisar por qué ObstacleAvoidance choca a veces. Causa: con el obstáculo delante no giraba (solo giraba
      mientras el sensor del lado elegido estuviera cerca), retrocedía y volvía a chocar. Arreglado: gira mientras
      haya algo delante o en ese lado, límite de 20 a 30 cm, sonar cada ~0,27 s en vez de ~0,5 s y pausas de
      100 ms antes de cambiar el sentido de los motores. En el suelo giraba hacia el lado del obstáculo
      (el motor One es la rueda derecha): invertidos los giros y giro mínimo de 300 ms. Probado en el suelo:
      esquiva de forma aceptable, aunque a veces duda cuando las medidas no se actualizan a tiempo.
- [ ] Mejorar la estabilidad de ObstacleAvoidance: filtrar lecturas falsas del HC-SR04 (saltos a ~277/361 cm)
      y decidir con medidas tomadas después de cada maniobra.

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
  - [x] Autenticación SSH con clave (`ssh-keygen` + copiar la clave pública a la Pi); nada de contraseñas.
        Pasos en el README; nombre de host `harlequin` en `~/.ssh/config` del PC. Hecho en el PC (Windows, clave ED25519).
  - [~] `tasks.json`: `dotnet publish -r linux-arm64 --self-contained false` en el PC → `scp` a `~/apps/<Proyecto>`
        → `ssh -t pi@<host> dotnet ~/apps/<Proyecto>/<Proyecto>.dll`. El nombre del host, configurable (sin datos personales).
        Hecho: tareas *Desplegar en la Pi*, *Ejecutar en la Pi* (eligen el programa) y *Parar el robot*; el host está
        en `.vscode/settings.json`. Probado desde el PC: *Ejecutar en la Pi* con SonarDashboard funciona.
        Arreglos: `nuget.config` solo con nuget.org (el PC tenía feeds de empresa que pedían login) y `chmod -R go-w`
        tras copiar (scp desde Windows crea las carpetas con escritura para todos).
  - [x] `launch.json` para depurar con F5: `pipeTransport` con `ssh` y `vsdbg` instalado en `~/vsdbg` de la Pi.
        `vsdbg` instalado. *Depurar BasicSample en la Pi* (F5) y *Adjuntar a un programa en la Pi* (para los que leen
        el teclado, como ObstacleAvoidance: con el depurador no hay teclado). Funciona desde el PC: se detiene en los
        puntos de interrupción. Problemas resueltos: vsdbg no traduce `~` en `args` (ruta del .dll relativa a `cwd`);
        la extensión de C# 2.160.x envía checksums SHA384/SHA512 que vsdbg rechaza ("Formato de solicitud de punto de
        interrupción incorrecto", dotnet/vscode-csharp#9802) → usar la 2.140.9 sin actualización automática (en el README).
        `PathMap` a `/_/` + `sourceFileMap` para que los símbolos no dependan de la carpeta del PC.
        Aviso: un punto de interrupción congela el PWM por software y cada motor queda parado o a toda velocidad.
  - [x] Ejecutar como `pi`, no como `root` (el usuario ya está en los grupos `gpio` e `i2c`).
  - [~] Seguridad: con `ssh -t`, Ctrl+C llega al programa y se paran los motores (depende de la Fase 2).
        Sin `-t`, cerrar el terminal puede dejar el programa corriendo en la Pi con los motores en marcha.
        Parar el depurador mata el programa (SIGKILL) sin parar los motores: `postDebugTask` *Parar el robot*
        (`pkill -INT` + `pinctrl set ... op dl`). Probado desde el PC: detenido en un punto de interrupción y Shift+F5,
        no queda ni el programa ni vsdbg en la Pi y todos los pines a nivel bajo. (Esa vez el PWM se congeló con los
        motores parados; falta el caso de motores congelados a toda velocidad.)
  - [ ] Cada conexión SSH desde el PC tarda ~5 s (el despliegue abre 3 o 4). Probablemente la resolución de
        `harlequin.local` en Windows; probar `HostName` con la IP o `AddressFamily inet` en `~/.ssh/config`.
  - [ ] Seguridad ante cortes de wifi: `ClientAliveInterval 5` / `ClientAliveCountMax 3` en el sshd de la Pi (requiere
        sudo) para que cierre la sesión caída y el programa reciba SIGHUP (parada de emergencia) en ~15 s.
  - [ ] Cambiar la contraseña de `pi` si sigue siendo la de los scripts antiguos (quedó en el historial público de git).
  - [x] Versionar `launch.json`/`tasks.json` directamente (quitarlos de `.gitignore`) y borrar los scripts y plantillas antiguos.
        Los scripts antiguos tenían la contraseña de `pi` en claro y siguen en el historial de git (repositorio público).
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
- [ ] 10 Siguelíneas con dos sensores infrarrojos TCRT5000. La rama `features/line-tracker` (2020–2022),
      ya borrada de GitHub, solo tenía el esqueleto: un `Program.cs` sin lógica y un ejemplo en Python copiado
      de un tutorial. Hacerlo desde cero.
- [ ] 11 Odometría: contar vueltas con los discos de 20 ranuras del chasis (vienen en el kit) y dos sensores
      ópticos de horquilla (p. ej. LM393, no incluidos) en las entradas digitales del HAT (requiere Fase 6).
      Ideas: medir velocidad, avanzar una distancia exacta y corregir la diferencia entre motores para ir recto.

## Fase 6: Binding `Iot.Device.ExplorerHat` en dotnet/iot
Estado: el binding sigue en el repositorio (activo, último cambio en el binding en julio de 2026), pero
solo cubre motores y las 4 luces. Falta:
- [ ] 4 entradas digitales (GPIO 23, 22, 24, 25, tolerantes a 5 V).
- [ ] 4 salidas de colector abierto (GPIO 6, 12, 13, 16).
- [ ] 4 entradas analógicas (ADS1015, I2C 0x48). Comprobar si sirve el binding `Ads1115` existente.
- [ ] 8 pads táctiles capacitivos (CAP1208, I2C 0x28). No hay binding CAP1xxx en dotnet/iot.
- [ ] **Prioritario, confirmado en 4.2.0:** `ExplorerHat` pasa su `GpioController` a `Motors`, `Lights`, cada
      `Led` y cada `DCMotor` con `shouldDispose = true` (valor por defecto), así que el primero que se libera
      cierra todos los pines. Los hilos de `SoftwarePwmChannel` de los motores siguen escribiendo y el
      proceso muere. Arreglo: pasar `shouldDispose: false` a los hijos, liberar los motores antes que las
      luces y el controlador el último. Comprobar también que al liberar un motor que iba marcha atrás no quede el pin de dirección en alto.
- [x] Control del DRV8833 marcha atrás: correcto (`DCMotor2PinNoEnable`, pin de dirección + PWM invertido).
- [ ] Preparar PR(s) a dotnet/iot.
