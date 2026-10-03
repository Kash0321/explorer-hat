# Plan de trabajo: puesta al día y ampliación

Objetivo: recuperar el repositorio para volver a hacer talleres con niños (programación, electrónica
y robótica) con Raspberry Pi 3 B+, Pimoroni Explorer HAT Pro y .NET 10, y revisar/ampliar el binding
`Iot.Device.ExplorerHat` de dotnet/iot.

Leyenda: `[ ]` pendiente · `[x]` hecho · `[~]` en curso

## Estado actual (03/10/2026)
- **Fase 5 en curso:** lecciones 01 a 04 en `lessons/` (luces, semáforo, motores y cuadrado; PR #15).
  Las cuatro están probadas en la Pi (la 03 y la 04, también con las ruedas en el aire) y la 04 está calibrada en el
  suelo con transportador (250 ms = 90°; tiene un modo calibración que se para tras cada giro).
- **Lección 06 (Distancia), 03/10/2026, rama `fase-5-leccion-06`:** probada sin motores, con las ruedas en el
  aire y en el suelo (frena a 58 cm, lee 27 cm y queda a ~20 cm de la pared).
- **Nueva Fase 7 (02/10/2026, PR #14):** robot con IA. El robot es el cuerpo y un portátil con un LLM local es el
  cerebro. Análisis, decisiones y pasos en la Fase 7. Va después de la Fase 5.
- **Cerradas:** fases 0 a 4. Solo queda abierta en la Fase 3 la guía de compilación ligera en la Pi (poco urgente:
  se compila en el PC). *Ejecutar en la Pi* se usa a diario con todos los programas (probado ya en el PR #7).
- **Alimentación resuelta (01/10/2026):** la Waveshare UPS HAT (B) está instalada y probada: ninguna caída de
  tensión, ni con los motores (comparativa de todas las fuentes en la Fase 2).
- **Montaje terminado (01/10/2026):** la UPS y la Pi forman un bloque (con el montaje que trae la UPS) que encaja
  en el hueco del chasis entre los soportes de los motores y se atornilla en dos ranuras. Ya están los 10 sensores
  LM393 para la lección 11.
- **Fase 4 cerrada (01/10/2026):** README de montaje, pinout, I2C y seguridad, y README de cada ejemplo.
- **Depuración (Fase 3, 01/10/2026):** la depuración paso a paso funciona deshabilitando C# Dev Kit en este repositorio,
  y Shift+F5 para los motores aunque se hayan congelado a toda velocidad.
- **Nuevo ejemplo:** `ExplorerHat.UpsDashboard`, panel con los datos de la UPS (PR #11).
- **Lección 09 (Robot autónomo), 03/10/2026, rama `fase-5-leccion-09`:** probada con las ruedas en el aire (los
  tres sensores, 10 pasos de giro sin salida, final por tiempo y Ctrl+C) y en el suelo con cajas (15 obstáculos en
  60 s; solo choca con algunas esquinas, que el sensor no ve).
- **Lección 05 (Botones), 03/10/2026, rama `fase-5-leccion-05`:** pulsador y LED en una protoboard, probada; sin
  rebotes. Arreglada la parada de emergencia desde el PC (finales de línea CRLF en los `.sh`).
- **Lección 11 (Contar vueltas), 03/10/2026, rama `fase-5-leccion-11`:** primera parte, con un LM393 en la rueda
  derecha (IN4): mide la velocidad y avanza una distancia exacta; probada en el aire y en el suelo. El sensor salta
  unos µs en cada borde de ranura: leer cada ~1 ms lo evita.
- **Lección 11, segunda parte (Ir recto), 03/10/2026, rama `fase-5-leccion-11-recto`:** dos sensores de velocidad
  (el izquierdo en IN3, en lugar del ECHO del HC-SR04 izquierdo) y control proporcional: de más de 45 cm de desvío
  en 1 m a 2–10 cm. Protección si un sensor deja de contar. **Al terminar, volver a conectar el HC-SR04 izquierdo.**
- **Siguiente paso propuesto:** la propuesta 12 (pantalla LCD; antes, revisar la tensión del bus I2C), girar ángulos
  exactos con los pulsos, o los PR a dotnet/iot de la Fase 6 (arreglo de `Dispose` de `ExplorerHat` y lectura con
  signo en `Ina219`).
- **Sin prisa:** cuando el usuario tenga un multímetro, medir si la pull-up del Trig de los HC-SR04P es una
  resistencia de la placa o la interna del chip (método en el README).
- Actualiza esta sección al final de cada sesión de trabajo.

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
- [x] Mejorar la alimentación (separar la de la Pi y la de los motores o batería de más amperios).
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
  - **Decisión (30/09/2026):** comprar la Waveshare UPS HAT (B).
- [x] Probar la Waveshare UPS HAT (B) cuando llegue: montaje con el Explorer HAT encima, `i2cdetect -y 1`
      (0x28, 0x42 y 0x48) y ObstacleAvoidance con `tools/vigilar-tension.sh`, como en las pruebas anteriores.
      Probada el 01/10/2026, solo con baterías (cargador de la UPS desconectado, 2×18650 a 7,9 V al empezar).
  - `i2cdetect -y 1` ve 0x28, 0x42 (INA219 de la UPS) y 0x48. Por SSH no interactivo, `i2cdetect` e `i2cget`
    están en `/usr/sbin` (fuera del `PATH`).
  - `tools/vigilar-tension.sh` registra también la tensión y la corriente de las baterías si encuentra la UPS
    (INA219 con resistencia de 0,1 Ω; corriente negativa = descarga), y `tools/comparar-tension.sh` resume
    la tensión mínima y la corriente máxima de cada tramo.
  - Resultado: **ninguna caída de tensión** en ninguna fase; `throttled=0x0` al final, también en los bits que
    recuerdan lo ocurrido desde el arranque.

    | Tramo | Tensión baja | Caídas | Baterías (mín.) | Corriente máx. |
    |---|---|---|---|---|
    | Reposo (~1,5 min) | 0 % | 0 | 7,84 V | 0,65 A |
    | Compilando la solución (2 min, CPU al 100 %) | 0 % | 0 | 7,83 V | 0,70 A |
    | ObstacleAvoidance N, ruedas en el aire (~1 min) | 0 % | 0 | 7,73 V | 1,15 A |
    | ObstacleAvoidance S, ruedas en el aire (~1 min, 8 maniobras) | 0 % | 0 | 7,72 V | 1,11 A |

    Esta prueba se hizo con el giro a pasos, que arranca los motores más veces que el programa de las pruebas
    anteriores: para la UPS fue una prueba algo más dura.
  - **Comparativa de todas las fuentes probadas** (ObstacleAvoidance con las ruedas en el aire, % del tiempo
    con tensión baja):

    | Fuente | Reposo | Compilando | Motores N | Motores S |
    |---|---|---|---|---|
    | Cargador 5 V 3 A con cables micro-USB malos | caídas constantes | caídas | — | — |
    | Cargador 5 V 3 A con cable bueno | sin caídas | sin caídas | sin caídas (BasicSample) | — |
    | Placa 2×18650 en paralelo con elevador (DIY MORE V8) | caídas al arrancar la Pi | — | 98 % | 92 % |
    | Batería externa Redmi 10000 mAh (5,1 V 2,4 A) | sin caídas | sin caídas | 68 % (6 caídas) | 54 % (6 caídas) |
    | Batería externa Xiaomi PLM10ZM 5000 mAh (5,1 V 2,1 A), poca carga | 4 caídas en 4 min | — | 97 % | 91 % |
    | **Waveshare UPS HAT (B), 2×18650 en serie** | **0 %** | **0 %** | **0 %** | **0 %** |

    La Redmi en el suelo con el giro a pasos (30/09/2026, modo S, batería ya usada) llegó al 91 % (20 caídas).
  - Conclusión: la UPS HAT (B) es la alimentación del robot. Las baterías externas USB quedan para trabajar con
    el robot quieto.
- [x] Nuevo ejemplo `ExplorerHat.UpsDashboard`: panel de consola con todo lo que da la UPS (su única conexión de
      datos es el INA219 en I2C 0x42): tensión de las baterías, carga estimada (6,0 V = 0 %, 8,4 V = 100 %, como el
      ejemplo de Waveshare), corriente con signo, potencia, estado (cargando o descargando), mínimos de la sesión y
      registros en bruto; además, `get_throttled` de la Pi y su temperatura. Solo lee, no cambia la configuración del
      chip. Lee los registros con `I2cDevice` porque el binding `Ina219` de 4.2.0 tiene un fallo (ver Fase 6).
      Probado: 7,88 V (79 %), −0,43 A descargando en reposo.
- [x] Revisar por qué ObstacleAvoidance choca a veces. Causa: con el obstáculo delante no giraba (solo giraba
      mientras el sensor del lado elegido estuviera cerca), retrocedía y volvía a chocar. Arreglado: gira mientras
      haya algo delante o en ese lado, límite de 20 a 30 cm, sonar cada ~0,27 s en vez de ~0,5 s y pausas de
      100 ms antes de cambiar el sentido de los motores. En el suelo giraba hacia el lado del obstáculo
      (el motor One es la rueda derecha): invertidos los giros y giro mínimo de 300 ms. Probado en el suelo:
      esquiva de forma aceptable, aunque a veces duda cuando las medidas no se actualizan a tiempo.
- [x] Mejorar la estabilidad de ObstacleAvoidance: filtrar lecturas falsas del HC-SR04 (saltos a ~277/361 cm)
      y decidir con medidas tomadas después de cada maniobra.
  - Diagnóstico con el robot quieto y sin motores (`SonarDashboard --registro`, 30 s, 142 lecturas por sensor):
    Centro (objeto a ~115 cm) salta 7 veces a 283–310 cm (5 %); Izquierda (~280 cm) tiene 4 lecturas sin eco y
    3 saltos (152, 255 y 362 cm); Derecha (~34 cm) es estable. Los saltos son aislados (nunca dos seguidos) y casi
    siempre a más distancia: el sensor pierde el eco del objeto y mide la pared de detrás. Son los peligrosos,
    porque el robot cree que el camino está libre.
  - Además, `Hcsr04.Distance` reintenta hasta 10 veces en silencio (hasta 160 ms cada intento) y lanza una excepción
    si todos fallan: por eso a veces las medidas no se actualizaban a tiempo.
  - Arreglo: `DistanceSensor` hace una sola lectura con `TryGetDistance` (sin eco = 400 cm) y su distancia es
    la más cercana de las dos últimas lecturas: ignora un salto aislado sin retraso cuando el obstáculo se acerca
    (solo tarda una lectura más en dar el camino por libre). `Sonar` mide en un hilo continuo (ciclo de ~0,2 s)
    y `WaitForNewReadings()` espera dos lecturas nuevas de cada sensor: el robot la usa al arrancar, antes de
    elegir el lado del giro y antes de volver a avanzar (si aún hay obstáculo, repite la maniobra).
  - Probado con las ruedas en el aire (mano delante del centro y de los lados, 6 maniobras, 218 lecturas por
    sensor): retrocede, espera, gira mientras hay obstáculo y vuelve a avanzar; sin errores y todos los pines
    a nivel bajo al terminar. Con los motores en marcha siguen los saltos (p. ej. Centro 282 cm con la mano a 6 cm)
    y el filtro los ignora. Falta probarlo en el suelo.
- [x] Probar ObstacleAvoidance con el filtro en el suelo (batería Redmi, 30/09/2026).
  - Primera prueba (N): esquiva bien, pero los giros eran excesivos (a veces una vuelta entera). Causa: giraba sin
    parar y miraba mientras giraba; tras ver el camino libre seguía ~0,4 s (una lectura más por el filtro y hasta
    0,2 s de espera del bucle). Además, el giro mínimo de 300 ms ya era un giro grande.
  - Arreglo: **giro a pasos**: gira 150 ms, se para, espera lecturas nuevas con el robot quieto y repite mientras
    haya algo delante o en el lado del que se aleja. Quitado el giro mínimo.
  - Frenadas bruscas: el robot levantaba la rueda trasera al frenar (la batería y la Pi no iban sujetas al chasis).
    Nuevo método `SlowDown`: con el modo S, frena en 3 pasos de 75 ms al ver un obstáculo y tras la marcha atrás.
    Los pasos del giro siempre arrancan y paran de golpe (son demasiado cortos).
  - Segunda prueba (S, ~2,5 min): 41 maniobras sin errores; giros de 1 paso (30), 2 (7), 3 (2) y 4 (1); 7 veces
    seguía el obstáculo tras el giro y repitió la maniobra. El usuario lo ve "más listo", con salidas y frenadas
    más elegantes. Pines a nivel bajo al terminar. Tensión baja el 91 % del tiempo (20 caídas): la batería ya
    llevaba un rato en uso y el giro a pasos arranca los motores más veces. Lo resolverá la UPS HAT (B).
  - SonarDashboard: la tecla F activa y desactiva el mismo filtro (y `--registro` guarda también la distancia
    filtrada). Con el filtro, el usuario nota que las medidas ya no dan saltos grandes de repente. No suaviza el
    temblor normal (±8 cm en el centro) ni dos saltos seguidos; para el panel se podría añadir una media.
  - Sujeción: hecha el 01/10/2026 (la UPS y la Pi, en un bloque atornillado al chasis). En campo libre, a veces no avanza en línea recta (los dos
    motores no giran igual): se corregirá con los sensores de velocidad (lección 11).

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
  - [x] `tasks.json`: `dotnet publish -r linux-arm64 --self-contained false` en el PC → `scp` a `~/apps/<Proyecto>`
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
        **Paso a paso arreglado (01/10/2026):** se detenía en el punto de interrupción, pero no respondía a F10 ni a F5.
        Diagnóstico con el registro de los dos lados: `vsdbg` en la Pi con `--engineLogging=/tmp/vsdbg.log` (a través
        de un script en `debuggerPath`) y `"logging": {"engineLogging": true}` en `launch.json` para `vsdbg-ui` del PC.
        VS Code enviaba `next`/`continue` a `vsdbg-ui`, pero `vsdbg-ui` no los reenviaba a la Pi (sí el `disconnect`);
        antes había pedido a C# Dev Kit el servicio `ManagedEditAndContinueRemoteDebuggerService2`. Causa: **C# Dev Kit
        3.40.210** (se actualizó sola) con la extensión de C# 2.140.9. Solución: deshabilitar C# Dev Kit en el área de
        trabajo (en el README y en `.vscode/extensions.json` como no recomendada). Descartados: la versión de `vsdbg`
        (con la 18.7.10521.2, la misma de la extensión, fallaba igual; la 18.10.10709.3 de `~/vsdbg` funciona) y el
        `ssh.exe` de Windows (reenvía bien los mensajes sin salto de línea). Probado: F10 y F5 hasta el final.
  - [x] Ejecutar como `pi`, no como `root` (el usuario ya está en los grupos `gpio` e `i2c`).
  - [x] Seguridad: con `ssh -t`, Ctrl+C llega al programa y se paran los motores (depende de la Fase 2).
        Sin `-t`, cerrar el terminal puede dejar el programa corriendo en la Pi con los motores en marcha.
        Parar el depurador mata el programa (SIGKILL) sin parar los motores: `postDebugTask` *Parar el robot*
        (`pkill -INT` + `pinctrl set ... op dl`). Probado desde el PC: detenido en un punto de interrupción y Shift+F5,
        no queda ni el programa ni vsdbg en la Pi y todos los pines a nivel bajo. (Esa vez el PWM se congeló con los
        motores parados.) Caso de motores congelados a toda velocidad probado el 01/10/2026 (ruedas en el aire):
        detenido tras `Thread.Sleep` con los motores en marcha, pines 19 y 21 fijos en `hi`; Shift+F5 → *Parar el robot*
        deja los 8 pines en `lo` y ni el programa ni vsdbg en la Pi.
  - [x] Cada conexión SSH desde el PC tardaba ~5 s (el despliegue abre 3 o 4). Medido en Windows: por nombre 4,6 s,
        con `AddressFamily inet` 2,95 s, por IP 0,48 s (`Resolve-DnsName harlequin.local` ~1,1 s). Solución: reserva DHCP
        en el router (MAC wifi `b8:27:eb:0b:5b:db` → 192.168.0.236) y `HostName` con la IP en `~/.ssh/config` del PC. Hecho: 0,52 s por conexión.
  - [x] Seguridad ante cortes de wifi (probado desconectando el portátil con ObstacleAvoidance en marcha):
    - `/etc/ssh/sshd_config.d/10-explorerhat.conf`: `ClientAliveInterval 5`, `ClientAliveCountMax 3`. No basta: sshd
      solo comprueba al cliente cuando la sesión está en silencio, y ObstacleAvoidance escribe sin parar.
    - `/etc/sysctl.d/90-explorerhat.conf`: `net.ipv4.tcp_retries2 = 6` (por defecto 15, ~15 min). La Pi da la
      conexión por muerta en ~20 s, sshd cierra la sesión y el programa recibe SIGHUP.
    - **Fallo encontrado en `SafeExplorerHat`:** al recibir SIGHUP escribía en la consola antes de parar los motores;
      con el terminal perdido la escritura fallaba, el programa terminaba y los motores se quedaban **al 100 %**
      (pines de PWM fijos en alto). Corregido: primero parar, después escribir (ignorando errores). Añadido también
      parar los motores ante cualquier excepción no controlada. Reproducido con luces y un terminal cerrado de golpe:
      antes 3/3 luces encendidas, después 3/3 apagadas.
    - Prueba real con la corrección: corte ~19:36:09, sesiones en silencio cerradas a los 15 s (ClientAlive), la
      del robot a las 19:36:38 (tcp_retries2), programa terminado y motores a nivel bajo a las 19:36:40 (~30 s).
    - Si la Pi se reinstala, hay que volver a crear los dos archivos (en el README).
  - [x] Cambiar la contraseña de `pi` si sigue siendo la de los scripts antiguos (quedó en el historial público de git).
        Cambiada el 30/09/2026; el acceso por clave SSH desde el PC sigue funcionando.
  - [x] Versionar `launch.json`/`tasks.json` directamente (quitarlos de `.gitignore`) y borrar los scripts y plantillas antiguos.
        Los scripts antiguos tenían la contraseña de `pi` en claro y siguen en el historial de git (repositorio público).
- Descartado como opción principal: VS Code Remote-SSH ejecutándose en la Pi (1 GB de RAM se queda corto
  con la extensión de C#).
- [ ] Guía de compilación ligera para 1 GB de RAM y ~3,5 GB libres en la microSD.

## Fase 4: Documentación
- [x] README: montaje, cableado de los sensores HC-SR04 (niveles de 5 V → entradas del HAT), pinout,
      habilitar I2C, normas de seguridad en el taller. Datos confirmados con el usuario (01/10/2026): sensores
      HC-SR04P, cada uno en su soporte; VCC al 5V del HAT y GND al GND del HAT; TRIG directo a OUT1–OUT3, sin
      resistencia: como las salidas OUT son de colector abierto, la pull-up del TRIG debe estar en el módulo (en la
      foto de la placa no se ve si es una resistencia o la interna del chip U3, RCW9006; se puede comprobar con un
      multímetro: ~5 V entre Trig y Gnd con el robot encendido y sin programa), y el
      pulso de disparo llega invertido (bajo en vez de alto) y aun así funciona. UPS: interruptor deslizante OFF/ON
      (en OFF al colocar las baterías) y pulsador *boot* para activar la protección (wiki de Waveshare).
- [x] README propio de cada ejemplo: BasicSample (estaba vacío), Common (`SafeExplorerHat`), ObstacleAvoidance
      (cómo decide: filtro, lecturas nuevas, giro a pasos, modos N y S), SonarDashboard (tecla F y `--registro`)
      y UpsDashboard.
- [x] Corregir AGENTS.md (espacio libre real, desarrollo en la propia Pi, requisito de I2C). Ampliado como memoria del
      proyecto para los asistentes: entorno, hardware, seguridad, cómo trabajar desde el PC por SSH y flujo de ramas.
- [x] Decidir si hace falta CLAUDE.md: una línea `@AGENTS.md`, para que cualquier instalación de Claude Code lo cargue.

## Fase 5: Itinerario didáctico (nuevos ejemplos graduados)
Cada lección es un proyecto pequeño en `lessons/LessonNN.Nombre`, con un único `Program.cs` legible por niños
(*top-level statements*, código y comentarios en inglés) y un README en español con retos. Índice en
`lessons/README.md`. Tarea de VS Code *Ejecutar una lección en la Pi*; `tools/parar-robot.sh`, `vigilar-pines.sh` y
la tarea *Parar el robot* reconocen los procesos `LessonNN.*`.
- [x] 01 Luces (`Lesson01.Lights`): instrucciones en orden, `Thread.Sleep`, bucle `for`. Probada en la Pi
      (02/10/2026): termina con código 0 y los 8 pines a nivel bajo.
- [x] 02 Semáforo (`Lesson02.TrafficLight`): variables y ciclos con `for`, modo noche. Probada en la Pi
      (02/10/2026), también la parada: con SIGINT a los 5 s escribe "Parada de emergencia (SIGINT)", termina y deja
      los pines a nivel bajo; con `tools/parar-robot.sh`, ningún proceso y pines a nivel bajo.
- [x] 03 Motores (`Lesson03.Motors`): velocidad de cada motor, avanzar, retroceder y girar, con una luz por
      movimiento. Probada con las ruedas en el aire (02/10/2026), registrando los pines cada 0,2 s: los pines de
      dirección (20 = `One`, 26 = `Two`) dan el sentido correcto en cada movimiento (girar a la derecha: 20 alto y
      26 bajo), código 0, pines a nivel bajo al terminar y `throttled=0x0`.
- [x] 04 Dibujar un cuadrado (`Lesson04.Square`): métodos propios (`MoveForwards`, `TurnRight`, `Stop`), bucle con
      movimiento, `if`/`else` y calibrar `turnTime`. Al empezar pregunta si se usa el **modo calibración** (`y`): se
      para tras cada giro y espera una tecla para medir la esquina. Probada con las ruedas en el aire (02/10/2026):
      4 veces avanzar (luz verde) y girar a la derecha (luz amarilla, pin 20 alto), código 0 y pines a nivel bajo.
      Calibrada en el suelo con un transportador (velocidad 0,8, UPS HAT (B)): 240 ms → ~85°, **250 ms → casi 90°**,
      320 ms → ~120°; las cuatro esquinas, casi iguales. Las estimaciones a ojo fallaron mucho (se confunden 45° y
      90°): en el taller, medir con transportador. El giro no es proporcional al tiempo (arranque del motor).
- [x] 05 Botones (`Lesson05.Buttons`, 03/10/2026): pulsador de 12 mm entre 5V e IN4 (GPIO 25) y LED rojo con 330 Ω
      entre 5V y OUT4 (GPIO 16), en una protoboard (esquema SVG en el README). El programa lee y escribe con
      `GpioController` (el binding no tiene entradas ni salidas: Fase 6). Tres partes: la luz roja y el LED siguen al
      botón (10 s), contar pulsaciones (cambio de suelto a pulsado, `wasPressed`, 10 s) y el LED parpadea una vez por
      pulsación. Bucles `for` que cuentan milisegundos (sin `while` ni `Stopwatch`, que llegan en la 06).
  - Sin programa: IN4 lee 0 en 50 de 50 lecturas y sigue al botón (5 pulsaciones de ~0,2 s y una de 2,1 s): las
    entradas del HAT tienen pull-down; no hace falta la resistencia de 10 kΩ. `pinctrl set 16 op dh` enciende el LED.
  - Con la lección: en la parte 1, la luz roja y el LED siguen al botón; en la parte 2, el programa contó 16 y el usuario también. Rebotes: ninguno, ni leyendo cada 1 ms (copia de
    diagnóstico con el tiempo entre pulsaciones: mínimo 475 ms). Pines a nivel bajo al terminar.
  - `tools/parar-robot.sh` y la tarea *Parar el robot* ponen también OUT4 (16) a nivel bajo.
  - **Fallo encontrado:** en el PC con Windows, git guardaba los `.sh` con CRLF y `ssh harlequin 'bash -s' <
    tools/parar-robot.sh` no paraba nada (`pinctrl`: `Unknown argument "dl^M"`; el patrón de `pkill` no
    coincide). Arreglado con `.gitattributes` (`*.sh text eol=lf`) y probado desde el PC. La tarea de VS Code no
    estaba afectada (lleva el comando dentro).
      Notas previas sobre el material. Alternativas si faltara algo: un LM393 de horquilla como "botón sin contacto" (meter
      una tarjeta en la ranura; además prepara la lección 11, que usa el mismo sensor en IN4) o dos cables dupont que
      se tocan (5V → IN4). Para comprar: módulo de botón con 3 pines (VCC, GND, salida; ya trae la resistencia) o
      botones arcade de 30 mm, más fáciles de pulsar para los niños.
- [x] 06 Distancia (`Lesson06.Distance`): el HC-SR04 del centro, luces por zonas (verde > 60 cm, amarilla, roja
      < 30 cm), `if`/`else if`/`else`, bucle `while` con cronómetro (`Stopwatch`, 10 s como máximo) y `break` al
      pararse. Filtro sencillo: la más cercana de las dos últimas lecturas (`Math.Min`), como `DistanceSensor` de
      ObstacleAvoidance pero sin hilo. `moveMotors = false` por defecto: primero solo mide; el niño lo cambia a
      `true`. Mide ~15 veces por segundo (pausa de 60 ms). Probada en la Pi (03/10/2026):
  - Sin motores, por SSH: código 0, pines a nivel bajo y `throttled=0x0`. Con la mano, el usuario ve el cambio
    verde → amarilla → roja y el final del programa.
  - Con motores y las ruedas en el aire (una copia con `moveMotors = true`), registrando los pines cada 0,2 s:
    pines de dirección (20 y 26) siempre bajos (hacia delante), PWM en los dos motores mientras no hay obstáculo,
    y los dos pines de velocidad a nivel bajo desde que la mano se acerca hasta el final (2 s con la luz roja).
    Ningún pin en alto sin programa, `throttled=0x0` (UPS con el cargador conectado).
  - En el suelo frente a una pared a ~115 cm (03/10/2026, UPS HAT (B) sin cargador, suelo liso), dos veces:
    se ve muy claro el cambio a la marcha lenta (0,6 basta para mover el robot), primera lectura amarilla a 58 cm,
    lectura de parada a 27 cm y el robot **quieto a unos 20 cm** de la pared en las dos pruebas. Unos 3 cm (rápido) o 2,5 cm (lento) entre
    dos lecturas; los ~7 cm restantes, desde la última lectura hasta pararse del todo. `dotnet` tarda ~3 s en
    arrancar el programa en la Pi (motores parados). Sin caídas de tensión (`0x0`), baterías a 7,95 V como
    mínimo y 1,02 A como máximo.
- [ ] 07 Pads táctiles: control remoto del robot (requiere Fase 6).
- [ ] 08 Sensores analógicos: luz o potenciómetro (requiere Fase 6).
- [x] 09 Robot autónomo (`Lesson09.Autonomous`): versión simplificada de ObstacleAvoidance, sin hilos ni clases.
      Mirar, pensar y actuar en un bucle de 60 s como máximo: `Look()` mide los tres sensores uno detrás de otro
      (60 ms entre lecturas, ~5 veces por segundo) con el filtro de la 06 en cada uno. Con algo a menos de 30 cm:
      para, retrocede 300 ms, mira dos veces con el robot quieto (`LookTwice`, para que el filtro solo use lecturas
      nuevas) y gira a pasos de 150 ms hacia el lado con más sitio hasta que el centro y el lado del que se aleja
      quedan libres (`||` y `&&`, `for` con `break`); tras 10 pasos sin salida, vuelve a retroceder. Velocidad 0,8,
      luces como en la 03. Probada con las ruedas en el aire (03/10/2026), registrando los pines cada 0,2 s:
  - Mano en el sensor izquierdo (3 veces): retrocede (20 y 26 altos) y gira a la derecha (20 alto). En el derecho
    (4 veces): gira a la izquierda (26 alto). Termina de girar al quitar la mano.
  - Mano fija en el centro: gira hacia el lado con más sitio; tras 10 pasos (23 líneas entre dos `Obstacle!`)
    retrocede y repite.
  - Final por tiempo (`Time is up!`) y con Ctrl+C (`Parada de emergencia (SIGINT)`; el bucle aún escribe una
    lectura): pines a nivel bajo, ningún proceso, `throttled=0x0` en toda la sesión.
  - El sensor derecho da a veces 46 cm en lugar de ~76 cm, en parejas: probablemente un eco real que solo
    detecta a veces. No afecta (es más de 30 cm).
  - En el suelo (03/10/2026, 60 s, ~2 × 2 m con cajas y una pared, UPS HAT (B)): 15 obstáculos. Casi siempre los
    ve a 20–30 cm y queda a ~15–25 cm; giros de 1 a 3 pasos; ~10–13 cm de avance entre dos miradas. El usuario lo
    ve bien en general. Un par de choques con **esquinas de cajas**: el sensor no las ve hasta tenerlas encima
    (centro de 115 a 8 cm, izquierda de 40 a 4 cm): la cara inclinada desvía el eco (reflexión especular) o la
    esquina queda entre dos conos. Explicado en el README como límite del sensor; no se corrige en la lección.
    Sin caídas de tensión (`0x0` en 857 muestras; baterías a 7,77 V como mínimo, 1,15 A como máximo) y pines a
    nivel bajo al terminar.
- [ ] 10 Siguelíneas con dos sensores infrarrojos TCRT5000. La rama `features/line-tracker` (2020–2022),
      ya borrada de GitHub, solo tenía el esqueleto: un `Program.cs` sin lógica y un ejemplo en Python copiado
      de un tutorial. Hacerlo desde cero. Hay dos módulos TCRT5000 (03/10/2026, por confirmar). Necesita dos
      entradas digitales: mismo problema que el segundo sensor de la lección 11 (solo IN4 está libre).
- [x] 11 Odometría: contar vueltas con los discos de 20 ranuras del chasis (vienen en el kit) y dos sensores
      ópticos de horquilla en las entradas digitales del HAT.
      Ideas: medir velocidad, avanzar una distancia exacta y corregir la diferencia entre motores para ir recto.
  - [x] Comprados 10 sensores de velocidad LM393 (horquilla óptica con salida digital) para probarlos.
  - [x] Probar un LM393 con un disco en IN4 (GPIO 25) (03/10/2026). Montado en la rueda derecha (motor One), con
        VCC y GND en la protoboard pequeña del HAT y D0 a INPUT 4 (en la primera prueba estaba en OUTPUT 4: no llegaba
        nada aunque el LED de señal parpadeaba).
    - A mano: 41 cambios por vuelta en dos vueltas (20 ranuras = 20 pulsos = 40 cambios; el de más, por la posición
      de la marca). Rueda de 6,5 cm: 20,4 cm por vuelta, **1,02 cm por pulso**.
    - Diagnóstico en C# (programa aparte, no versionado; rueda derecha en el aire, 3 s por velocidad): la salida
      **salta 2 o 3 veces en unos µs justo en cada borde de ranura** (el comparador no tiene histéresis); con el
      motor parado no hay ningún cambio. Contando solo los niveles que duran ≥ 0,5 ms (igual con 1, 2 o 3 ms):
      0,4 → 37,3 pulsos/s (112 rpm), 0,6 → 46,3 (139), 0,8 → 51,3 (154), 1,0 → 55,3 (166). A 0,8, ~52 cm/s,
      coherente con lo estimado en el suelo en la lección 06. Una ranura dura ≥ ~7 ms.
    - Los eventos de GPIO (`RegisterCallbackForPinValueChangedEvent`) cuentan también los saltos (621 en 3 s a 1,0
      frente a ~166 reales): no sirven sin filtro. Leer la entrada cada ~1 ms (`Thread.Sleep(1)`) no ve los saltos
      y no pierde ranuras: es lo que usa la lección.
    - Fallo de `System.Device.Gpio` 4.2.0 en la Pi 3 (`RaspberryPi3Driver`): `UnregisterCallbackForPinValueChangedEvent`
      lanza `GpiodException: Device or resource busy` (intenta volver a abrir la línea). Registrar una sola vez y no
      quitarlo. Con la excepción, `SafeExplorerHat` paró el motor (`Error inesperado: motores parados`).
  - [x] Lección `Lesson11.Odometry` con un sensor (03/10/2026): parte 1 mide la velocidad (2 s), espera una tecla para
        medir con la cinta; parte 2 avanza 50 cm (`while` con distancia `&&` tiempo máximo de 5 s, por si falla el
        sensor); después de cada parte cuenta 0,5 s más lo que resbala. Con las ruedas en el aire (versión sin tecla):
        85 pulsos en 2 s (43 cm/s), parada a 49 pulsos (50 cm) y 4 pulsos más por la inercia; código 0 y pines a nivel
        bajo. En el suelo (7 pruebas a 0,6): parte 1 de 72 a 77 pulsos (37–39 cm/s; una vez 58, quizá un roce);
        parte 2 siempre parada a 49 pulsos y 4–7 pulsos más al frenar (una vez 0), unos 55 cm en total; con la cinta,
        cuando fue recto, 55 cm, igual que lo contado (55,1 cm). **Se desvía a la izquierda** a menudo: la rueda
        derecha gira más rápido que la izquierda.
  - [x] **Dónde conectar el segundo sensor** (03/10/2026): de momento, en la lección se desconecta el HC-SR04
        izquierdo y su D0 va a IN3 (GPIO 24), con el VCC y el GND que deja libres. Más adelante: las entradas
        analógicas (ADS1015, ~500 lecturas/s por canal: bastaría para ranuras de ≥ 7 ms, pero necesita la Fase 6) o
        un ESP32 que cuente los dos sensores. La Pi no tiene GPIO libres accesibles con el Explorer HAT encima.
  - [x] Corregir la desviación en línea recta: `Lesson11.Straight` (03/10/2026). Cuenta los dos sensores cada ~1 ms
        y corrige con un control proporcional sobre la diferencia de pulsos (`speed ∓ difference * correction`,
        `correction` 0,02, `Math.Clamp` 0–1); `correct` permite compararlo sin corrección. Luces: verde = iguales,
        amarilla = derecha por delante, azul = izquierda por delante. Avanza 100 cm (media de las dos ruedas).
    - Diagnóstico en el aire, las dos ruedas a la misma velocidad: casi iguales (derecha/izquierda 1,037 a 0,4;
      1,007 a 0,6; 1,006 a 0,8; 0,969 a 1,0). La diferencia aparece en el suelo (peso, patinaje, rueda loca).
    - En el suelo, 1 m a 0,6: **sin corrección** (7 pruebas) diferencia final de 4 a 11 pulsos y más de 45 cm a la
      izquierda; **con corrección** (17 pruebas) de 0 a 4 pulsos y de 2 a 10 cm de la línea, a uno u otro lado. Las
      diferencias incluyen 0,5 s de inercia sin corregir. Queda un error pequeño, propio del control proporcional.
    - En 3 pruebas el sensor izquierdo se salió de su sitio (0 y 13 pulsos frente a 28–30): la corrección paró la
      rueda derecha y puso la izquierda a tope (el robot giraba sobre una rueda). Añadida una protección: con la
      corrección, si la diferencia pasa de `maxDifference` (10 pulsos), se para y avisa. Probada con el sensor fuera
      del disco y las ruedas en el aire: parada a los 11 pulsos con el aviso.
  - [~] Girar ángulos exactos con los pulsos y el cuadrado de la lección 04 con pulsos: `Lesson11.Square`
        (03/10/2026, rama `fase-5-giros-exactos`). Lados de 30 cm con la corrección de la parte 2; giros sobre sí
        mismo en los que cada rueda se para por separado al llegar a sus pulsos, menos `brakePulses` por lo que resbala;
        modo calibración (se para tras cada giro y muestra lo que ha resbalado cada rueda). 90° = 10 pulsos (9° por
        pulso: la resolución es gruesa).
    - En el aire (`brakePulses` 1, objetivo 9): la rueda que va hacia delante cuenta 11–12 (sigue rodando mientras la
      otra llega) y 1–3 más de inercia; lados de 29 pulsos y 4–5 de inercia. Código 0, pines a nivel bajo.
    - En el suelo (`brakePulses` 1): cada rueda 9–10 pulsos y **3–4 resbalando** (~12,5 en total ≈ 112°); el usuario
      midió **~115°**: el cálculo con la distancia entre ruedas encaja, sobra lo que resbala. Lados: 29 pulsos y 5–7
      más (~36 cm en lugar de 30). Cambiado a `brakePulses` 3 (objetivo 7 pulsos): pendiente de probar.
    - La UPS se quedó sin batería durante las pruebas (la Pi se reinició): cargar antes de seguir. Distancia entre las ruedas: **13 cm** (medida por el usuario): una vuelta sobre sí mismo son 40,8 cm de
        cada rueda y 90° unos 10 pulsos.
- [ ] 12 (propuesta) Pantalla: LCD de 20×4 caracteres (2004A) con adaptador I2C (PCF8574). Ideas: escribir texto y
      variables, encajar un mensaje en 20 columnas, y un panel del robot sin terminal (distancias, batería de la UPS,
      obstáculos esquivados) para las pruebas en el suelo. `Iot.Device.Bindings` 4.2.0 trae `Lcd2004` y `Pcf8574`.
      Antes de conectarla:
  - Dirección I2C: 0x27 (PCF8574T) o 0x3F (PCF8574AT), según el chip; los puentes A0–A2 la cambian. No choca con
    0x28, 0x42 ni 0x48.
  - **Tensión del bus I2C:** la pantalla necesita 5 V, y el adaptador suele llevar resistencias de pull-up de SDA y
    SCL a su VCC: con VCC a 5 V, SDA y SCL de la Pi (3,3 V) quedarían por encima de 3,3 V. Medir esas resistencias
    y decidir: quitarlas (la Pi ya tiene pull-ups a 3,3 V), un conversor de niveles o alimentar el adaptador a 3,3 V
    (el contraste puede no bastar).
  - Ver por dónde se llega a SDA (GPIO 2) y SCL (GPIO 3) con el Explorer HAT encima.

### Componentes disponibles (03/10/2026)
- 10 sensores de velocidad LM393 (horquilla óptica, 4 pines: VCC, GND, D0, A0): lección 11 y, si hace falta, 05.
- 2 módulos TCRT5000 (infrarrojo por reflexión, con potenciómetro; por confirmar): lección 10.
- Pantalla LCD 2004A con adaptador I2C PCF8574: lección 12 (propuesta).
- 4 pulsadores táctiles de 12 mm (4 patas, 2 contactos): lección 05.
- Resistencias de 10 kΩ (pull-down si hiciera falta) y de 330 Ω (para LED), y LED rojos y amarillos de 5 mm.
- 3 módulos láser KY-008 (650 nm, pines S, centro y −). **Peligro para los ojos:** solo con el monitor, nunca
  apuntando a una persona ni a superficies que reflejen. Idea: alarma de haz cortado con una fotorresistencia (no hay).
- Módulo L298N (HW-095): doble puente H para dos motores, con radiador y regulador de 5 V. El robot no lo necesita
  (el Explorer HAT lleva un DRV8833) y necesitaría 6 GPIO que no hay libres. Pierde ~2 V (transistores bipolares).
  Sirve para explicar qué es un puente H.
- 2 protoboards de 400 puntos (30 filas, columnas a–j y dos líneas de alimentación + y − a cada lado) y muchos cables
  dupont (macho-hembra, hembra-hembra y algunos macho-macho).
- Una placa ESP32 DevKit (módulo ESP-WROOM-32, 30 pines, wifi y Bluetooth, lógica de 3,3 V). Ideas: mando a distancia
  inalámbrico del robot, o contar los pulsos de los LM393 (tiene contadores de pulsos por hardware) y pasárselos a la
  Pi, porque a la Pi le faltan entradas. Se puede programar en C# con .NET nanoFramework.
- IN4 (GPIO 25) con nada conectado lee 0 en 50 de 50 lecturas (pull de la Pi desactivado): la entrada del HAT
  parece tener pull-down o un búfer que la mantiene baja. Confirmar con el pulsador.

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
- [ ] **Fallo en el binding `Ina219` (4.2.0 y rama principal en octubre de 2026):** `ReadShuntVoltage()` y
      `ReadCurrent()` leen el registro como número sin signo (`ReadRegister` devuelve `ushort`), así que las tensiones
      y corrientes negativas (por ejemplo, baterías descargándose en la UPS HAT (B)) salen como valores enormes.
      Arreglo: convertir a `short` antes de escalar. No hay ninguna incidencia abierta: posible PR a dotnet/iot.
- [x] Control del DRV8833 marcha atrás: correcto (`DCMotor2PinNoEnable`, pin de dirección + PWM invertido).
- [ ] **Posible incidencia en `System.Device.Gpio` 4.2.0 (Pi 3, `RaspberryPi3Driver`):** quitar un aviso de eventos
      con `UnregisterCallbackForPinValueChangedEvent` lanza `GpiodException: Device or resource busy` (detalles en la
      lección 11 de la Fase 5). Buscar si ya está abierta antes de informar.
- [ ] Preparar PR(s) a dotnet/iot.

## Fase 7: Robot con IA (agente)
Idea (02/10/2026): en los últimos talleres, el robot funciona como un agente. Entiende órdenes en lenguaje natural
(primero escritas y después habladas), mira sus sensores y decide qué hacer con los motores y las luces. Va después
de la Fase 5: las herramientas del agente son los métodos que los niños escriben en las lecciones
(`Avanzar(segundos)`, `Girar(...)`, `EncenderLuz(...)`).

Análisis (a partir de una conversación del usuario con Gemini, revisada con el contexto del proyecto):
- **El LLM no puede ir en la Pi.** Con 1 GB de RAM solo caben modelos de 0,5B o menos (~0,5–2 tokens/s según
  Gemini, sin medir), que fallan a menudo al dar JSON o llamadas a herramientas. Además, con los 4 núcleos al 100 %
  se retrasan el hilo del PWM por software de los motores y la medida del eco del HC-SR04, y la Pi baja su frecuencia
  al llegar a 60 °C.
- **Arquitectura:** la Pi es el cuerpo (ejecuta las acciones y tiene la capa reactiva de seguridad) y un portátil de
  la red local es el cerebro (el LLM elige las acciones).
- **Todo en C#**, no en Python con la librería `explorerhat` de Pimoroni que proponía Gemini: se perdería
  `SafeExplorerHat` (y su ejemplo tenía mal los colores de las luces). En el portátil, `Microsoft.Extensions.AI`
  con Ollama, que admite llamadas a herramientas (*tool calling*).
- **Seguridad, siempre en la Pi y nunca en el LLM:**
  - El LLM solo elige acciones de una lista cerrada; nunca toca un pin.
  - La Pi valida y recorta los valores (velocidad, duración, luces que existen).
  - Cada movimiento tiene una duración máxima (por ejemplo, 3 s) y termina con `Speed = 0.0`. Si se corta la red,
    el robot se para al terminar la acción en curso (en la Fase 3, un corte de SSH dejó los motores al 100 %).
  - Capa reactiva: parada por obstáculo a 30 cm con `Sonar` y `DistanceSensor` (con su filtro de lecturas falsas),
    no a 10 cm como proponía Gemini: el sonar mide cada ~0,2 s y el filtro añade una lectura de retraso.
- **Privacidad:** son niños de 10 a 14 años. Nada de Web Speech API de Chrome (envía la voz a Google). La voz y el LLM
  van en local; la nube, solo de reserva y sin voz. Antes de usar la nube, revisar con la organización del taller
  el consentimiento y la protección de datos de menores.
- **Voz:** con *push-to-talk* (botón que se mantiene pulsado para hablar), porque en el taller hablan varios niños a la
  vez. El micrófono va en el portátil: la Pi 3B+ no tiene entrada de micrófono (su conector de 3,5 mm es solo de salida).
- **Latencia:** Gemini promete menos de 1 s por orden. Solo es realista con tarjeta gráfica: hay que medirlo.

Decisiones (02/10/2026):
- LLM local en el portátil (Ollama), con una API en la nube de reserva si el portátil no da la velocidad suficiente.
- Primera versión con órdenes escritas; la voz, después.
- Portátil del taller: sin decidir. Equipo candidato: MSI Prestige 15 A12UD (i7-1280P de 14 núcleos, 32 GB de RAM,
  NVIDIA RTX 3050 Ti Laptop con 4 GB de VRAM, 299 GB libres). En 4 GB de VRAM caben enteros los modelos de 3B
  cuantizados a 4 bits (~2 GB); uno de 7B (~4,7 GB) no cabe entero y una parte va por la CPU. Su controlador NVIDIA es
  de enero de 2022 (30.0.15.1165 = 511.65) y Ollama pide la 531 o superior para usar la tarjeta gráfica: hay que
  actualizarlo.

Pasos:
- [ ] Medir el LLM en el portátil candidato: Ollama con `qwen2.5:3b` y `llama3.2:3b`, y `qwen2.5:7b` para comparar.
      Unas 20 órdenes de prueba en español: tiempo de respuesta y aciertos en la herramienta y en sus valores.
      Repetir con la API de reserva.
- [ ] Decidir el protocolo entre el portátil y la Pi (HTTP sencillo, WebSocket o MQTT, que necesita un servidor más)
      y cómo detecta la Pi que se ha perdido la conexión.
- [ ] Servicio de órdenes en la Pi (C#, con `SafeExplorerHat`): lista cerrada de acciones, validación, duración
      máxima, parada al perder la conexión y parada por obstáculo. Probarlo con las ruedas en el aire y con órdenes
      escritas a mano, sin LLM. Repetir la prueba de corte de wifi de la Fase 3.
- [ ] Cerebro en el portátil (C#): orden escrita → LLM con las herramientas → acciones → Pi. El LLM recibe como texto
      el estado de los sensores (distancias y batería).
- [ ] Voz: Whisper local en el portátil (por ejemplo, whisper.cpp) con *push-to-talk*. Probar con ruido de fondo.
- [ ] Opcional: respuesta hablada (un altavoz con amplificador propio en el conector de 3,5 mm de la Pi, o el portátil).
- [ ] Opcional: cámara en el conector CSI de la Pi y un modelo de visión en el portátil, para que el robot "vea".
- [ ] Opcional, como experimento para el taller: `llama.cpp` con un modelo de 0,5B en la Pi, con los motores parados.
      Medir tokens/s, RAM y temperatura, para que los niños vean con datos por qué el cerebro va fuera. Ocupa unos
      400 MB (la microSD tiene ~2,6 GB libres).
- [ ] Lecciones: sentir, pensar y actuar; editar el *system prompt* y la lista de herramientas; capa reactiva frente a
      capa deliberativa; comparar su programa fijo con el agente. La parte compleja (servidor, JSON y validación) es
      código del monitor, no de los niños.
