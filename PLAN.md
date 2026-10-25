# Plan de trabajo: puesta al día y ampliación

Objetivo: recuperar el repositorio para volver a hacer talleres con niños (programación, electrónica
y robótica) con Raspberry Pi 3 B+, Pimoroni Explorer HAT Pro y .NET 10, y revisar/ampliar el binding
`Iot.Device.ExplorerHat` de dotnet/iot.

Leyenda: `[ ]` pendiente · `[x]` hecho · `[~]` en curso

## Estado actual (25/10/2026)
- **Cierre de la sesión de la tarde del 25/10/2026: Fase 7, medida del LLM y servicio de órdenes hechos** (desde el PC; el robot
  solo se movió en las pruebas con las ruedas en el aire, con la confirmación del usuario).
  * **PR #63 (medida del LLM) y #64 (servicio de órdenes) fusionados** (`ed66d71` en `main`, 25/10/2026); las ramas
    `fase-7-medir-llm` y `fase-7-servicio-ordenes`, borradas (local y remota). `main` ya tiene todo el trabajo de la Fase 7.
  * **Medida del LLM** (ver "Medida del 25/10/2026", Fase 7; herramienta en [`tools/medir-llm/`](tools/medir-llm/README.md)):
    `qwen2.5:7b` acierta la herramienta en 24/24 órdenes pero tarda 3 s de mediana (45 % en la tarjeta gráfica); `qwen2.5:3b`
    tarda 0,7 s pero confunde "Para." con `avanzar`; `Qwen3-4B-Instruct-2507` queda en medio (1,1 s) pero no ejecuta `¡¡PARA
    YA!!`. **Decisión del modelo: el usuario la dejó para más tarde** (nada depende de ella hasta escribir el cerebro). Tampoco
    se probó la API de reserva (necesita clave y revisar la protección de datos de menores).
  * **Servicio de órdenes** (`src/ExplorerHat.Orders`; ver "Servicio de órdenes (25/10/2026)", Fase 7): protocolo HTTP,
    decidido por el usuario. Probado sin mover las ruedas (29 casos en el PC, 23 en la Pi, 4 de 4 en obstáculos con los sensores
    reales, señales y códigos de salida) y con las ruedas en el aire (7 de 7, incluido el Ctrl+C, y el corte de red: el robot se
    para solo a los 3 s). **No se probó en el suelo.**
  * **Portátil:** controlador NVIDIA 621.01; Ollama 0.40.11 con cinco modelos (los de Hugging Face; **la red a la que estaba
    conectado el portátil, por cable y con o sin VPN, bloquea el servidor de archivos de Ollama**, `*.r2.cloudflarestorage.com`;
    ver el README de `tools/medir-llm`). Quedan ~8 GB de descargas incompletas (`*-partial`) en `~\.ollama\models\blobs`, que
    sirven para reanudar `ollama pull` desde otra red.
  * **Pi:** actualizada a `main` en `ed66d71` (con el #63 y el #64) con `git pull --ff-only`, y `~/apps/tools-orders` borrada:
    su `tools/parar-robot.sh` y su `tools/vigilar-pines.sh` ya conocen `Orders`. En reposo: 8 pines en `lo`, `get_throttled` =
    `0x0`, ningún programa del robot en marcha. Tiene desplegado `~/apps/ExplorerHat.Orders` (compilación Release del
    código del #64) y la clave en `~/.explorerhat-clave`.
  * **dotnet/iot:** sin cambios mirados por la tarde (al empezar la sesión: #2625 en `CHANGES_REQUESTED` esperando a pgrawehr,
    comprobaciones en verde; clon `C:\work\iot` en `explorerhat-io`, `c4119033`, limpio). La red bloqueada del portátil no
    afectó a GitHub.
- **Sesión del 25/10/2026** (desde el PC; solo dotnet/iot, sin mover el robot; la Pi, en `main` (`a68e53c`), pines en `lo`, `get_throttled` = `0x0`):
  - **#2610 fusionado** el 22/10/2026 (`efe9e367` en `upstream/main`). La incidencia #2614 sigue abierta (pgrawehr dijo que
    la arreglaba el #2610): se puede pedir que la cierren, o comentar que ya está arreglada. Queda el fallo 1 de nuestro
    comentario (quitar un aviso de otro tipo de evento), que ellos ven poco realista.
  - **#2614: comentario publicado** (25/10/2026, con el visto bueno del usuario y la consulta al asesor):
    [comentario](https://github.com/dotnet/iot/issues/2614#issuecomment-6396860453), comprobado igual que el borrador.
    Dice que el #2610 ya la arregla (probado en la Pi con `c6fcf210`) y que se puede cerrar. Solo cita dos pruebas del
    registro (quitar uno de dos avisos y `WaitForEvent` repetido): la de "registrar otra vez" no salió bien en ese registro.
  - **#2617: pgrawehr contestó** (22/10/2026): "implementa lo que mejor encaje con nuestra API, no somos estrictos con el
    diseño". No hay que contestar a sus preguntas. **Queda posible un PR aparte** con el motor de PWM en las dos entradas
    (el `HatMotor` de `Common` es el modelo), cuando el #2625 deje de ocupar la cola.
  - **#2625 (entradas y salidas): revisado el 22/10/2026.** Las comprobaciones pasaban. pgrawehr pidió cambios
    (`CHANGES_REQUESTED`): devolver `GpioPin` (o una derivada) en vez de `DigitalInput`/`DigitalOutput`, y sin clases de
    lista artificiales (`IList<T>` o `Get(int)`). Copilot dio dos hallazgos: las entradas y salidas se quedaban con un pin
    que el programa ya había abierto y lo cerraban al liberarse.
  - **Decisión del usuario (opción A′):** `DigitalInput` y `DigitalOutput` **derivan de `GpioPin`** (cumple a pgrawehr) y
    **conservan `On()`, `Off()`, `IsOn` y `One` a `Four`**, porque el código tiene que ser lo más intuitivo posible para
    niños de 10 a 14 años (`hat.Outputs.One.On()` se lee como `hat.Lights.Blue.On()`; `Write(PinValue.High)` no dice que en
    una salida de colector abierto significa "encendido"). Las colecciones implementan `IReadOnlyList<T>`. `Pin` pasa a
    ser `PinNumber`. Si pgrawehr insiste, **nos ajustamos y hacemos una envolvente en este repositorio** (se lo dijimos).
  - **Commit `c4119033`** en `explorerhat-io` (sin *force push*), con: pin abierto antes = `InvalidOperationException` y
    `Close`/`Dispose` solo cierran lo que abrieron; una entrada rechaza `Write`, `Toggle` y el modo de salida (su pin lo
    maneja el búfer); una salida rechaza el modo de entrada. 16 pruebas en `ExplorerHat` y 10 en `DCMotor`; las pruebas del
    pin abierto fallan sin el arreglo (comprobado con dos fallos metidos a propósito). Respuestas publicadas en los tres
    hilos, iguales a los borradores: [pgrawehr](https://github.com/dotnet/iot/pull/2625#discussion_r4370528600),
    [Copilot, entrada](https://github.com/dotnet/iot/pull/2625#discussion_r4370528667) y
    [Copilot, salida](https://github.com/dotnet/iot/pull/2625#discussion_r4370528783).
  - **Integración continua de `c4119033` (25/10/2026): pasan todas** (las 6 compilaciones de Linux, macOS y Windows, el CLA,
    `markdownlint` y `markdown-link-check`).
  - **Pendiente del #2625:** que pgrawehr revise otra vez (el PR sigue en `CHANGES_REQUESTED`: un commit
    nuevo no lo quita; no hay que pedir nada, las respuestas ya les avisan). **La rama `explorerhat-analog-v2` NO se ha rehecho a propósito**: `AnalogInputs`
    usa el mismo patrón de lista (`List<AnalogInput>` + `IEnumerable`); si pgrawehr rechaza `IReadOnlyList` + `One` a `Four`,
    habría que hacer el trabajo dos veces. Al rehacerla (cherry-pick en una rama nueva desde `explorerhat-io`, porque el
    #2625 sigue sin fusionar), alinear `AnalogInputs` con `IReadOnlyList<T>`, esperar conflictos en `README.md` y
    `samples/Program.cs`, y buscar `\.Pin\b` después. Comprobar también que `AnalogInputs` y `TouchPads` de `Common` siguen la
    forma del binding.
- **Cierre de la sesión del 25/10/2026:** PR #60 y #61 fusionados y sus ramas borradas (local y remota). `main` y la Pi
  en `442ef87` (la Pi, con `git pull --ff-only`; con el PR de este cierre, `main` avanza un commit más y la Pi hay que
  actualizarla). Pi: en reposo, 8 pines en `lo`, `get_throttled` = `0x0`, ningún programa del robot en marcha; **el robot no
  se movió en toda la sesión**. Clon del fork (`C:\work\iot`): en la rama `explorerhat-io` (`c4119033`), sin cambios sin
  confirmar. dotnet/iot: #2625 en `CHANGES_REQUESTED` (espera la nueva revisión de pgrawehr); `upstream/main` en `41086078`.
  Con esto empieza la **Fase 7**: no mueve el robot hasta tener el servicio de órdenes (ver el punto 6 de "Al retomar").
- **Sesión del 19/10/2026** (desde el PC; solo se trató dotnet/iot, sin mover el robot; la Pi, en `main` y en reposo):
  - **#2616 (`Cap1208`) fusionado** el 18/10/2026 (`5cf07458` en `upstream/main`). Rama `cap1xxx-binding` borrada.
  - **#2625 (entradas y salidas):** sin revisiones ni comentarios; las comprobaciones pasan; "Review required".
    Sus ramas siguen en `0078c337` (`explorerhat-io`) y `f39ca85c` (`explorerhat-analog-v2`), sin cambios.
  - **#2610: respuesta de pgrawehr (18/10/2026) a nuestros dos fallos, sin pedirnos nada.** Fallo 1 (quitar un aviso
    de otro tipo de evento): lo conocen; lo ven válido pero poco realista, porque esperan que las aplicaciones
    registren los avisos al arrancar y los quiten al cerrar. Fallo 2 (`WaitForEvent` y aviso en el mismo pin): no se
    espera usar los dos a la vez, y `GpioController` no garantiza seguridad entre hilos en un mismo pin; confirma que
    es lo que Copilot encontró y que Copilot no ha atendido la orden de arreglarlo. **No se responde**: nada que
    añadir. Para nosotros, la consecuencia es de diseño: en el robot, registrar los avisos una vez y no mezclarlos con
    `WaitForEvent` en el mismo pin.
  - Incidencias #2614, #2615 y #2617: sin comentarios nuevos.
- **Sesión 4 del 17/10/2026** (desde el PC, con la Pi y el robot montado, ruedas en el aire o en el suelo; pantalla LCD conectada): dotnet/iot sin novedades al empezar (PR #2616, #2625 y
  #2610 e incidencias #2614, #2615 y #2617 sin comentarios nuevos; `upstream/main` en `1856d767`; las comprobaciones
  pasan; el clon del fork, limpio). Pi en `main` (`59f7741`), pines en `lo`, `get_throttled` = `0x0`.
  - **Decisión del usuario: no hacer la lección 10 (siguelíneas) y renumerar.** Rama `fase-5-renumerar-lecciones`: las
    carpetas, los proyectos, la solución, las tareas de VS Code, `probar-cableado.sh` y los README usan los números
    nuevos (ver "Fase 5, catálogo cerrado", más abajo).
  - **Parada de emergencia: carrera entre el hilo de la señal y el principal, arreglada** (rama
    `fase-5-parada-carrera-ctrl-c`, `SafeExplorerHat`). Detalles, pruebas y reglas nuevas en la Fase 2.
    **Hay que volver a desplegar todo lo que usa `Common`** cuando se fusione el PR (ver `AGENTS.md`).
  - **Lección 07 en el suelo, con la pantalla conectada:** a 0,6 el robot se seguía con dificultad; con 0,4 arrancan los
    motores y con 0,3 no. La lección usa **0,45** y la prueba en el suelo a 0,45 salió bien (ver la lección 07, en la
    Fase 5). También la probó una niña de 10 años en el suelo: sin problemas.
- **Sesión 3 de la noche del 17/10/2026** (desde el PC, con la Pi y el robot montado, ruedas en el aire; **pantalla LCD
  desconectada**, potenciómetro en Analog 1):
  - **Lección 07 hecha** (`lessons/Lesson07.Touch`, rama `fase-5-leccion-07-pads-tactiles`): mando a distancia con los
    pads 1 a 4 (azul = avanzar, amarilla = retroceder, roja = izquierda, verde = derecha: **cada pad enciende su luz**, en el
    orden del HAT). Solo se mueve con `moveMotors = true`, y solo mientras el dedo está en el pad. Ayudante nuevo
    `TouchPads` en `Common` (`hat.Touch.One.IsTouched()`). Pruebas y decisiones en la Fase 5, lección 07.
  - **dotnet/iot, sin novedades** al empezar (comprobado: ni comentarios ni revisiones nuevas en el #2616, #2625 y
    #2610, ni en las incidencias #2614, #2615 y #2617; `upstream/main` en `1856d767`; las comprobaciones pasan).
- **Sesión de la noche del 17/10/2026** (desde el PC, con la Pi y el robot montado, ruedas en el aire):
  - **Repaso de las lecciones con motores hecho** (04, 06, 10 y 09, con el `Common` nuevo): todas siguen haciendo lo
    mismo y la parada de emergencia funciona con `SIGTERM` y con Ctrl+C en la 09. Resultados, dos lecciones del
    repaso (`| tee` y `pgrep`) y lo que falta comprobar, en la Fase 5, "Repaso con el `Common` nuevo".
  - **dotnet/iot, sin novedades** al empezar: ni comentarios ni revisiones nuevas en el #2616, #2625 y #2610, ni en las
    incidencias #2614, #2615 y #2617; `upstream/main` sigue en `1856d767`; las comprobaciones pasan. El clon del fork
    está limpio, con sus tres ramas iguales a las del fork.
  - **Experimento de modelos:** esta fue la sesión 2 de 3. `/usage` al empezar y al cerrar (2,49 $ en total; el
    asesor, una llamada, 0,70 $) en "Experimento de modelos", más abajo. PR #52 fusionado; `main` y la Pi en `36a11bc`.
- **Sesión del 17/10/2026** (desde el PC, con la Pi y el robot montado, ruedas en el aire):
  - **Pi actualizada** a `main` (`2da35a5`) con `git pull --ff-only`. Sin programas del robot en marcha, pines en `lo`
    y `get_throttled` = `0x0`.
  - **Multímetro llegado:** ExtraStar DT830B, barato y sencillo. Manual y marcas de la carcasa transcritos en
    castellano y en inglés en [`docs/multimetro-DT830B.md`](docs/multimetro-DT830B.md) (la foto del manual venía con
    cinco idiomas; solo se guardan esos dos). Con él se pueden medir las LDR y comprobar el potenciómetro de la
    lección 08 sin el ADS1015 (ver la Fase 5). **Límite:** el último rango de resistencia es de **2 MΩ**, no de
    20 MΩ como dice la tabla del manual (con 1999 cuentas y 1 kΩ de resolución, el fondo de escala es de unos 2 MΩ;
    coincide con la marca `2000k` del selector). Una LDR a oscuras puede pasar de 2 MΩ: la pantalla mostrará solo
    "1" (fuera de rango).
  - **Experimento de modelos** (asesor Opus 5.5, subagente `buscador`): reglas en `AGENTS.md` y criterio para
    seguir o quitarlo en "Sin prisa". Configuración comprobada en una sesión nueva.
  - **Medidas con el multímetro** (lección 08, tarde del 17/10/2026): las tres LDR son casi iguales (~12 kΩ con una
    luz fuerte, ~23 kΩ con la luz de la sala, ~110 kΩ con un dedo encima y > 2 MΩ a oscuras) y el potenciómetro
    tiene el cursor en el hilo **negro**. Resultados, tablas y propuestas en la Fase 5 (lección 08). Sin tocar el
    robot ni la Pi.
  - **Potenciómetro probado en la Pi** (17/10/2026): de 0,01 V a 5,31 V con `~/apps/PotCheck`. Fallo del primer
    intento: el 5V iba a una fila de protoboard sin unir al pin 5V del HAT. Detalles en la lección 08.
  - **Pull-up del Trig de los HC-SR04P medida** (17/10/2026, multímetro, mismos resultados en los tres sensores): con
    el robot apagado y en `200k` (en `20k` marcaba "1"), **unos 62–65 kΩ entre Trig y VCC, igual en los dos sentidos**
    (una resistencia, no un diodo: el valor encaja con una de 62 kΩ, 5 %); con el robot encendido y sin programa,
    **4,93 V entre Trig y GND**. El multímetro no dice si es una pieza soldada o está dentro del chip (U3).
    **VCC–GND del mismo sensor, con el robot encendido: 5,29 V en los tres** (coincide con los 5,30 V del ADS1015: el
    multímetro y el ADS1015 se confirman entre sí). El Trig queda 0,36 V por debajo de VCC (4,93 V frente a 5,29 V)
    porque la línea tiene, además de la pull-up de ~62 kΩ, una carga de **~1 MΩ hacia GND** (cálculo con el divisor:
    62 kΩ × 4,93 / 0,36 ≈ 0,85 MΩ en total, ≈ 0,9 MΩ sin los 10 MΩ del multímetro), de la salida del HAT en reposo o
    de la entrada del sensor. No importa: 4,93 V es un nivel alto claro.
  - **Cierre de la sesión del 17/10/2026 (tarde):** PR #44 a #50 de este repositorio fusionados; `main` y la Pi en
    `44c554a` (con `git pull --ff-only`). Quedó pendiente el repaso de las lecciones con motores (hecho por la noche,
    ver arriba). **Experimento de modelos:** esta fue la sesión 1 de 3; el usuario no pasó `/usage`.
    En dotnet/iot, sin respuesta nueva a nuestro comentario del #2610 ni en el #2616, el #2625 y las incidencias.
  - **Lección 08 hecha** (`lessons/Lesson08.Analog`, PR de la rama `fase-5-leccion-08-analogico`): el usuario eligió un
    ayudante `AnalogInputs` en `ExplorerHat.Common` y no el fork (choque de tipos `CS0433`, comprobado). Probada en la
    Pi con el potenciómetro, sin motores. Detalles y lo que falta probar en la Fase 5, lección 08. **Prueba de motores
    con el `Common` nuevo hecha (`Lesson03.Motors`, ejecución completa y parada con `SIGTERM`, 0,24 s hasta tener los
    pines en `lo`)**; PR #48 fusionado y la Pi al día en `main` (`0d6fb5f`).
  - **#2610 probado en la Pi con el último commit del PR** (`c6fcf210`, ya con el recuento de referencias `0f4e5718` y
    los arreglos de Copilot). Mismo método que el 03/10/2026 (GPIO 8 sin nada conectado y flancos con `pinctrl set 8
    pu`/`pd`; no mueve los motores). **Arregla lo que faltaba:** con dos avisos en el pin, quitar uno deja funcionando
    el otro, y `WaitForEvent` repetido y registrar/quitar/registrar un aviso funcionan. **Dos fallos nuevos**, que no
    impiden fusionarlo pero hay que contar a pgrawehr:
    1. Con dos avisos de **tipos de evento distintos** (por ejemplo `Rising | Falling` y solo `Rising`), quitar el
       segundo **no lo quita**: sigue recibiendo flancos. Causa: `LibGpiodV2EventObserver.RemoveCallback` busca solo
       la primera suscripción del `offset` (`FirstOrDefault`), y cada tipo de evento es otra suscripción. El código es
       el mismo que en `main` (el PR solo cambia el tipo del índice de un bucle en ese archivo); en la Pi 3 no se veía porque ni siquiera dejaba registrar un segundo
       aviso.
    2. **Intermitente:** dos `WaitForEvent` seguidos y después registrar un aviso: a veces el hilo que lee los eventos
       muestra `Unhandled exception while handling libgpiodv2 edge events: GpiodException ... epoll_ctl: Error:
       'Success'` y se pierden flancos (2 de 4 en una de las 5 ejecuciones; en otra, el aviso no vio ninguno). Con un
       solo `WaitForEvent` no pasó. Parece una carrera entre la limpieza de la petición tras el `WaitForEvent` y el
       hilo observador: es la zona de los hallazgos que krwq pidió arreglar a Copilot (15/10/2026).

    Programa de prueba (no versionado; fuente en `~/apps/GpioEvents2610/Program.cs` en la Pi), con un modo `seq` (`dotnet GpioEvents.dll 8 seq
    R1,R2,U2,E`: `R1`/`R2` registran el aviso 1 (flancos de subida y de bajada) o el 2 (solo subida), `U1`/`U2` los
    quitan, `E` hace 4 flancos y `W` es un `WaitForEvent`). Registro de la secuencia completa en la Pi:
    `~/gpioevents-2610-c6fcf210.log`.
  - **Comentario publicado en el #2610** (17/10/2026, con el visto bueno del usuario y la consulta al asesor):
    [comentario](https://github.com/dotnet/iot/pull/2610#issuecomment-6232082492), comprobado igual que el borrador.
    Cuenta que el arreglo del recuento de referencias funciona y los dos fallos. Antes de publicar se comprobó que
    el PR seguía en `c6fcf210` y se leyó `RaspberryPi3LinuxDriver.WaitForEvent` (abre el pin, suma un uso y cierra al
    llegar a 0), que sostiene la hipótesis del fallo 2. **Dato extra, no publicado:** `RemoveSubscriptions(Offset)`
    usa el mismo `FirstOrDefault` que `RemoveCallback`, así que `ClosePin` borra una sola suscripción por pin y, con
    dos tipos de flanco, puede dejar otra suelta. Útil si pgrawehr pide un PR de arreglo (la prueba con el modo `seq`
    ya reproduce el fallo 1).
- **Sesión del 16/10/2026** (desde el PC, sin la Pi: el usuario estaba fuera de casa; detalles en la Fase 6):
  - **#2613 fusionado** el 15/10/2026 por pgrawehr (`1856d767` en `upstream/main`, *squash*). Lo aprobó sin contestar
    a la pregunta de las variantes con `PwmChannel`. Rama `explorerhat-dispose` borrada (local y en el fork) después de
    comprobar que su contenido era igual al commit fusionado.
  - **#2616 (`Cap1208`):** Copilot revisó el PR (15/10/2026) con dos hallazgos. pgrawehr arregló el primero con un
    commit en nuestra rama (`73c9225e`: el constructor libera el `I2cDevice` si el chip no es un CAP1208) y nos
    preguntó por el segundo: el simulador no daba la interrupción al soltar un pad, así que un pad soltado seguía
    "tocado" para siempre. Respondido con el commit `985490a8` (una línea en `Release()` del simulador y dos
    comprobaciones nuevas en las pruebas, que fallan sin los arreglos) y una respuesta en cada hilo.
  - **PR de entradas y salidas abierto: [#2625](https://github.com/dotnet/iot/pull/2625)**, rama `explorerhat-io`.
    Claude Code (modo automático) no dejó hacer `git rebase` de las ramas apiladas: los commits se copiaron con
    `cherry-pick` sobre `upstream/main` en **ramas nuevas** (`explorerhat-io` y `explorerhat-analog-v2`), sin *force
    push*. Las antiguas (`explorerhat-inputs-outputs` y `explorerhat-analog`), borradas. Pruebas: 10 de `DCMotor` y 8
    y 14 de `ExplorerHat` en cada rama, sin avisos.
  - **#2610:** pgrawehr añadió el recuento de referencias (`0f4e5718`, el fallo de los dos avisos en un pin de la
    #2614). krwq pidió a Copilot otros dos arreglos. Sin fusionar.
  - **Llegaron los potenciómetros** (3 ALLECIN WH148 B10K) y una **cámara para la Raspberry Pi** (modelo sin anotar;
    Fase 7). La lección 08 sigue en la Pi, en la próxima sesión con el robot.
- **Sesión del 10/10/2026:**
  - **dotnet/iot, sin cambios:** nada nuevo desde nuestras respuestas del 09/10/2026. El #2613 sigue en "Review
    required" (pgrawehr no ha contestado a la pregunta de las variantes con `PwmChannel`); el #2616 y el #2610, aprobados
    y sin fusionar. `upstream/main`, sin commits nuevos.
  - **Lección 08: sensores decididos.** Un **potenciómetro** lineal de 10 kΩ (B10K) y una **fotorresistencia** (LDR,
    modelo sin confirmar) con una resistencia fija de 10 kΩ. **Las fotorresistencias ya las hay** (una bolsa de 20); los
    potenciómetros (3 ALLECIN WH148 B10K) están pedidos y llegan el 11/10/2026. Motivos y descartes en la Fase 5
    (lección 08).
  - **Mientras llegan, no se empieza la lección 10:** antes, decidir con el usuario otra tarea.
  - **Medida de las LDR, preparada y sin hacer:** el usuario no tiene multímetro, así que se miden con el ADS1015 del
    HAT. Programa `~/apps/LdrCheck` en la Pi, probado sin nada conectado (lee el chip, código 0, pines en `lo`).
    Montaje, comando y tabla de modelos en la Fase 5 (lección 08). El usuario lo dejó para otra sesión.
- **Sesión del 09/10/2026** (PR #41 y #42 de este repositorio; detalles en la Fase 6):
  - **#2612 (`Ina219`) fusionado** el 08/10/2026 (`d84c9407` en `upstream/main`). Rama `ina219-signed-readings` borrada.
  - **#2613, #2616 y #2610, aprobados** por raffaeler (el #2612 también por pgrawehr). Las pruebas de `Button` ya no
    fallan (#2608 fusionado): pasan todas las comprobaciones.
  - **Revisión del #2613 respondida** con un commit nuevo (`15efc1ae`): excepción si no hay controlador y
    `shouldDispose` es false (lo pidió pgrawehr) y el pin de dirección en bajo antes de liberar el canal PWM (hallazgo
    de Copilot). Ramas apiladas rehechas encima. Con el commit nuevo pasan las 10 comprobaciones, pero se anuló la
    aprobación de raffaeler: el PR está en "Review required" hasta que la repitan (no hay que pedir nada).
  - **#2610:** en el *triage* decidieron fusionarlo así y arreglar aparte el fallo de los dos avisos en un pin. Según
    pgrawehr, el #2610 arregla la #2614.
- **Sesión del 04/10/2026, tarde** (PR #38 y #39; detalles en las fases):
  - **Lecciones en VS Code:** *Desplegar en la Pi* y *Ejecutar en la Pi* tienen las lecciones y los programas en una
    sola lista, y **Depurar en la Pi el programa abierto** depura con F5 el proyecto del archivo abierto. Fase 3.
  - **Rueda derecha parada:** `Lesson10.Odometry` contaba 0 pulsos. El sensor estaba bien: el cable azul del motor
    derecho se había soltado de la borna MOTOR 1. Diagnóstico, mensajes de las lecciones 10 y README. Fase 5.
  - **Lección 08 (sensores analógicos), decidida como siguiente tarea:** con `ProjectReference` al binding de la rama
    `explorerhat-analog` del fork (`C:\work\iot`). Faltaba saber qué sensor analógico se usa: decidido el 10/10/2026
    (potenciómetro y fotorresistencia, por comprar). **Cambiado el 17/10/2026 (opción Z, elegida por el usuario):** sin
    `ProjectReference` al fork, porque su tipo `ExplorerHat` choca con el del paquete 4.2.0 (`CS0433`); la lección usa un
    ayudante `AnalogInputs` en `ExplorerHat.Common`, con la forma del binding futuro (ver la Fase 5, lección 08).
- **Sesión del 04/10/2026, mañana** (PR #33 a #36 de este repositorio; detalles en las fases):
  - El usuario desmontó y volvió a montar todo el cableado. Revisado con `tools/probar-cableado.sh` (nuevo): todo
    bien salvo los motores, cambiados de borna y con el derecho al revés. Colores de los cables, en el README.
  - **Marcha atrás más lenta** con el `DCMotor` del binding (a 0,4, al ~36 % de la velocidad hacia delante): marcha
    atrás, el DRV8833 frena en la parte apagada del PWM. **Nuevo `HatMotor`** en `ExplorerHat.Common` (PWM en los dos
    pines, como Pimoroni): los dos sentidos al 93–102 %. Informado en dotnet/iot (#2617). Fase 6.
  - **Giros recalibrados en el suelo:** lección 04, `turnTime` 200 ms; `Lesson10.Square`, `brakePulses` 4. La 09 y
    ObstacleAvoidance, sin cambios (límite de obstáculo en 30 cm, decisión del usuario). Fase 5.
  - **Alimentación:** la Pi va solo por los contactos de la UPS, **sin cable micro-USB** (con y sin cable, igual).
    Pin de 5 V: 5,30 V en reposo y 5,22 V como mínimo con los motores (0,6 V de margen). Al compilar en la Pi, la CPU
    llega a 60 °C y baja su frecuencia (`0x80000`). Fase 2.
  - **ADS1015 del HAT a 5 V:** entradas analógicas de 0 a 5 V, sin divisor; `Ads1115` lo lee bien. Fase 6.
  - **Binding `ExplorerHat` ampliado** en dos ramas del fork, probadas en la Pi: entradas y salidas digitales
    (`explorerhat-inputs-outputs`) y entradas analógicas (`explorerhat-analog`). Esperan al #2613. Fase 6.
  - Todos los programas de `~/apps` están desplegados desde `main` con `HatMotor`.
- **Fase 5, lecciones hechas y probadas** (`lessons/`): 01 Luces, 02 Semáforo, 03 Motores, 04 Cuadrado, 05 Botones,
  06 Distancia, 07 Pads táctiles (17/10/2026, ruedas en el aire y en el suelo a 0,45), 08 Sensores analógicos (17/10/2026, sin motores),
  09 Robot autónomo, 10 Contar vueltas (odometría, ir
  recto y el cuadrado con pulsos) y 11 La pantalla. Todas con las ruedas en el aire y, las que se mueven, también en
  el suelo.
- **Fase 5, catálogo cerrado** (decisión del usuario, 17/10/2026): ya no falta ninguna lección. El siguelíneas (dos
  TCRT5000) pasa a "Ideas para ampliar las lecciones" (Fase 5). **Renumeración del 17/10/2026:** `Lesson11.*` (Contar
  vueltas, Ir recto y El cuadrado perfecto) es ahora `Lesson10.*`, y `Lesson12.Screen` es `Lesson11.Screen`. Los PR, las
  ramas y los commits anteriores usan los números antiguos (por ejemplo, la rama `fase-5-leccion-12`); el texto de este
  documento ya usa los nuevos.
  - **Limpieza de la Pi hecha (17/10/2026, tras fusionar el PR #55):** sin programas del robot en marcha, `git pull
    --ff-only` (`530f441`), borradas **por su nombre** las cuatro carpetas viejas en `lessons/` (solo tenían `bin/` y
    `obj/`) y en `~/apps`, y desplegadas con los nombres nuevos `Lesson10.Odometry`, `Lesson10.Straight`,
    `Lesson10.Square` y `Lesson11.Screen` (con los mismos comandos que la tarea *Desplegar en la Pi*). Comprobado: los
    cuatro `.dll` existen, los ocho pines están en `lo` y `get_throttled` = `0x0`. No se movieron los motores ni se
    ejecutó ninguna lección. **Regla para otra renumeración:** borrar las carpetas viejas por su nombre, nunca con un
    comodín como `Lesson11.*` (borraría también la lección nueva con ese número).
- **Montaje actual** (a las 22:00 del 17/10/2026, **la pantalla LCD estaba desconectada y se volvió a conectar esa noche**, con `i2cdetect` mostrando 0x27, 0x28, 0x42 y 0x48; el resto, como se describe):
  todo conectado a la vez. Tres HC-SR04 (IN1–IN3), LM393 derecho en IN4 (a 5 V), LM393 izquierdo
  a **3,3 V** en GPIO 18 (fila lateral "3.3V ONLY") y pantalla LCD 2004 en I2C 0x27 (a 3,3 V). El LED de la lección 05
  no está montado. El pulsador de la lección 05 necesita IN4: idea, moverlo al GPIO 8 (CS) con el pull-down de la Pi.
  **Desde el 17/10/2026, además:** el **potenciómetro** de la lección 08 (rojo → 5V, amarillo → GND, negro → Analog 1)
  comparte con el LM393 derecho el nodo de 5V de la protoboard de la Pi. El nodo de 3,3 V del LM393 izquierdo y de la
  pantalla sigue separado. **Comprobado con `tools/probar-cableado.sh` el 17/10/2026** (Pi en `main`, ruedas en el
  aire): luces bien; LM393 derecho 89 cambios (y 0 en el izquierdo) y izquierdo 110 (y 0 en el derecho); motores en el
  sentido esperado; sensores de distancia 20/20. **La pantalla falló** (`IOException: Error 5 (Input/output error)`
  en la primera escritura al PCF8574; `i2cdetect` no veía el 0x27, y sí el 0x28, 0x42 y 0x48): el usuario había
  desconectado los cuatro cables de la pantalla para trabajar y los volvió a conectar con **SDA y SCL intercambiados**.
  Los cuatro cables son Dupont independientes (amarillo GND, verde VCC → 3v3, azul SDA, blanco SCL): colores, foto y
  reglas para conectarla y desconectarla, en el README. Al corregirlos, `i2cdetect` vuelve a mostrar 0x27, y **el script completo salió bien en la segunda pasada** (LM393
  derecho 81 y 0, izquierdo 0 y 129; motores en el sentido esperado; texto en la pantalla; sensores de distancia
  20/20). **Lección:** con la pantalla apagada, `Error 5` en el
  constructor de `Screen` quiere decir que nadie contesta en 0x27 (alimentación, SDA/SCL cruzados o cable suelto);
  con la luz de fondo encendida, la alimentación está bien y se mira SDA/SCL. Si VCC fuera a 5V en vez de a 3,3 V, las
  resistencias de subida de la placa de la pantalla pondrían 5 V en los pines I2C de la Pi. **Medido el 17/10/2026**
  (pantalla desconectada, multímetro en `20k`): **4,73 kΩ entre SDA y VCC y 4,67 kΩ entre SCL y VCC**, así que la
  placa sí lleva resistencias de subida de 4,7 kΩ y la regla "VCC solo a 3v3" es necesaria.
- **Cerradas:** fases 0 a 4 y la Fase 5 (catálogo cerrado el 17/10/2026; la lección 07, probada también por una niña de 10 años). De la Fase 3 solo queda la guía de compilación ligera en la Pi (poco urgente).
- **Fase 7 (robot con IA):** analizada y decidida; la Fase 5 está cerrada (17/10/2026), así que puede empezar.
- **Fase 6, en dotnet/iot (25/10/2026):**

  | Qué | Estado | Pendiente |
  |---|---|---|
  | [PR #2612](https://github.com/dotnet/iot/pull/2612): `Ina219` con signo ("Fixes #1659") | **Fusionado** el 08/10/2026 | Nada. Llegará en la próxima versión de `Iot.Device.Bindings` |
  | [PR #2613](https://github.com/dotnet/iot/pull/2613): `Dispose` de `DCMotor` y `ExplorerHat` | **Fusionado** el 15/10/2026 (`1856d767`) | Nada. Llegará en la próxima versión de `Iot.Device.Bindings` |
  | [PR #2616](https://github.com/dotnet/iot/pull/2616): binding nuevo `Cap1208` (pads táctiles) | **Fusionado** el 18/10/2026 (`5cf07458`, *squash*), tras aprobarlo pgrawehr. Rama `cap1xxx-binding` borrada (local y en el fork) después de comprobar que sus 15 archivos eran iguales a los del commit fusionado | Nada. Desbloquea los pads táctiles en `ExplorerHat` (PR 4) |
  | [PR #2625](https://github.com/dotnet/iot/pull/2625): entradas y salidas en `ExplorerHat` (rama `explorerhat-io`, `c4119033`) | Abierto el 16/10/2026; revisado el 22/10/2026 (cambios pedidos por pgrawehr y Copilot), respondido con `c4119033` el 25/10/2026 | Que pgrawehr lo revise otra vez (ver el "Estado actual"). De él depende la rama del analógico |
  | [Incidencia #2614](https://github.com/dotnet/iot/issues/2614): eventos de GPIO con libgpiod v2 | Abierta (`untriaged`) | pgrawehr: la arregla el #2610 |
  | [Incidencia #2615](https://github.com/dotnet/iot/issues/2615): `QueryComponentInformation` en la Pi 3 | Abierta (`untriaged`), sin respuesta | Solo incidencia |
  | [Incidencia #2617](https://github.com/dotnet/iot/issues/2617): marcha atrás de `DCMotor2PinNoEnable`; propuesta de motor con PWM en las dos entradas | Abierta (`untriaged`); pgrawehr contestó el 22/10/2026: decidimos nosotros la API | Nada que contestar. Posible PR aparte con el motor de PWM en las dos entradas |
  | Rama `explorerhat-analog-v2` del fork (`f39ca85c`, encima de `explorerhat-io`): entradas analógicas en `ExplorerHat` | Lista y probada en la Pi, sin PR | Abrir el PR cuando fusionen el #2625 |
  | [PR #2610](https://github.com/dotnet/iot/pull/2610) de pgrawehr: arreglos de libgpiod v2 | **Fusionado** el 22/10/2026 (`efe9e367`). Probado en la Pi el 17/10/2026: arregla la #2614 y tiene dos fallos nuevos, [comentados](https://github.com/dotnet/iot/pull/2610#issuecomment-6232082492); pgrawehr no pidió nada | Nada. Llegará en la próxima versión de `System.Device.Gpio` |

  `upstream/main` (25/10/2026, `41086078`): desde el #2616 solo ha cambiado, en nuestras áreas, el #2610 (libgpiod v2). Si los mantenedores comentan, se responde en el PR y los cambios van en commits nuevos (sin *force push*).
  **Los mantenedores pueden subir commits a nuestras ramas** (pgrawehr lo hizo en el #2616): antes de confirmar nada en
  la rama de un PR, `git fetch origin` y `git merge --ff-only`. Para mirar el
  estado: `gh pr checks <PR> -R dotnet/iot`, los comentarios con `gh api repos/dotnet/iot/issues/<n>/comments` y, en
  un PR, también las revisiones (`.../pulls/<n>/reviews`) y los comentarios en el código (`.../pulls/<n>/comments`).
- **Cierre de la sesión 4 (17/10/2026, noche):** PR #55, #56 y #57 fusionados; `main` y la Pi en `e65fc9f`. **Hecho tras
  fusionar el #57:** desplegados de nuevo con el `Common` nuevo los 15 proyectos que lo usan (13 lecciones,
  `ExplorerHat.BasicSample` y `ExplorerHat.ObstacleAvoidance`; `Lesson07.Touch` ya va a 0,45), borradas las copias de
  prueba `Lesson07.TouchFloor` y `Lesson07.TouchStill`, y comprobada la parada sin motores (`Lesson07.Touch` con SIGINT
  da 130 y `Lesson08.Analog` con SIGTERM da 143, con mensaje y sin errores). Pi: ningún programa en marcha, 8 pines en
  `lo`, `get_throttled` = `0x0`, `i2cdetect` con 0x27, 0x28, 0x42 y 0x48, 2,4 GB libres. dotnet/iot sin novedades (PR
  #2616, #2625 y #2610 e incidencias #2614, #2615 y #2617 sin comentarios nuevos; `upstream/main` en `1856d767`).
  **La Fase 5 está cerrada** (catálogo de 11 lecciones; la 07 probada también por una niña de 10 años).
- **Al retomar (siguiente sesión):**
  El repaso de las lecciones con motores y la lección 07 están hechos (17/10/2026, noche); la 07 también
  en el suelo a 0,45. Empezar por el punto 1 de abajo (5 minutos, dotnet/iot). **El checklist del taller está hecho**
  (25/10/2026, [`docs/checklist-taller.md`](docs/checklist-taller.md): material, portátiles, red del aula, comprobaciones del
  día antes, ritmo de la sesión, normas y fallos frecuentes). **Tarea principal propuesta:** la Fase 7 (el siguelíneas
  ya no es una lección, es una idea guardada). Antes de un taller con más de un robot hay que resolver que **cada robot
  tenga un nombre distinto en la red** y que cada portátil apunte al suyo (hoy las tareas de `.vscode/` y el alias SSH
  apuntan a `harlequin`): ver el apartado "Portátiles" del checklist.
  **Experimento de modelos: decidido** (17/10/2026, ver "Experimento de modelos"): se mantiene como está.
  1. Mirar comentarios y revisiones en el PR #2625 (los #2610 y #2616 ya están fusionados) y en las incidencias #2614, #2615 y #2617. Si hay
     comentarios, contárselos al usuario antes de responder. En el #2625, mirar también las comprobaciones y si
     pgrawehr contestó a nuestra respuesta del 25/10/2026 (ver el "Estado actual"): según lo que pida, ajustar o hacer la
     envolvente en este repositorio. Con los cambios pedidos, `git fetch origin` y `git merge --ff-only` en `explorerhat-io`.
  2. Las ramas van apiladas: `explorerhat-io` (#2625) → `explorerhat-analog-v2`. Cuando fusionen el #2625 (con
     *squash*, como el #2613), copiar el commit del analógico encima de `upstream/main` en una rama nueva, sin `rebase`
     ni *force push*: `git switch -c explorerhat-analog upstream/main`, `git cherry-pick <commit de
     explorerhat-analog-v2>` (el nombre `explorerhat-analog` está libre). Pasar las pruebas de `DCMotor` y de
     `ExplorerHat`, comprobar que el contenido es igual (`git diff explorerhat-analog-v2 explorerhat-analog --
     src/devices/ExplorerHat`), subirla, abrir su PR y borrar `explorerhat-io` y `explorerhat-analog-v2` (local y en
     el fork). Si piden cambios en el #2625, van en commits nuevos en `explorerhat-io`; después, copiar otra vez el
     commit del analógico encima en otra rama nueva, igual. **Al rehacerla, alinear `AnalogInputs` con el #2625**
     (misma forma: `IReadOnlyList<T>` con `One` a `Four`, sin lista propia): hoy usa `List<AnalogInput>` +
     `IEnumerable`. Esperar conflictos en `README.md` y `samples/Program.cs`, y buscar `\.Pin\b` después. **No se rehizo
     antes a propósito:** si pgrawehr rechaza el `IReadOnlyList` + `One` a `Four`, habría que hacerlo dos veces.
  3. ~~Probar en la Pi el #2610 con el recuento de referencias~~ (hecho el 17/10/2026, ver arriba) y ~~comentar sus
     dos fallos~~ (publicado el 17/10/2026). El #2610 se fusionó el 22/10/2026 y pgrawehr no pidió ningún PR. Si algún
     día piden un PR de arreglo para el fallo 1, el dato extra de arriba ayuda. ~~Comentar en la #2614 que el #2610 ya la
     arregla~~ (publicado el 25/10/2026, ver el "Estado actual").
  4. Siguiente pieza del binding: los pads táctiles en `ExplorerHat` (PR 4). **El `Cap1208` (#2616) ya está
     fusionado (18/10/2026), así que se puede empezar**, pero el #2625 sigue sin revisar.
     **Recomendación del 04/10/2026:** no alargar la cadena de ramas sin revisar; mientras tanto, trabajar en otra cosa.
     **Lección 07 hecha el 17/10/2026** (con el ayudante `TouchPads` de `Common`: ver la Fase 5, lección 07).
     **Lección 08 hecha el 17/10/2026** (opción Z, sin fork: ver la Fase 5, lección 08). **La lección 08 usa solo el
     potenciómetro** (decisión del usuario, 17/10/2026: sin LDR). Los otros dos potenciómetros se midieron con el
     multímetro (10,35 y 9,83 kΩ en total); falta ver con el programa que tienen el mismo orden de hilos. **La
     lección de siguelíneas se descartó** (decisión del usuario, 17/10/2026; es una idea guardada en la Fase 5).
     Después, la Fase 7 (medir el LLM en el portátil).
  5. **#2617 (motor con PWM en las dos entradas):** pgrawehr dejó la API a nuestro criterio (22/10/2026). Es un PR
     aparte en `C:\work\iot` (rama nueva desde `upstream/main`); el modelo es `HatMotor` de `Common`. Mejor después de que
     revisen el #2625, para no tener tres ramas abiertas a la vez.
  6. **Fase 7. La medida del LLM y el servicio de órdenes están hechos (25/10/2026, ver "Medida del 25/10/2026" y "Servicio
     de órdenes" en la Fase 7), con las pruebas con motores en el aire (7 de 7 y corte de red).** Siguiente: (a) decidir con
     el usuario el modelo (`qwen2.5:3b`, rápido pero con la parada fuera del modelo, o `qwen2.5:7b`, fiable pero de 3 s) y
     si se prueba la API de reserva; (b) el cerebro en el portátil (C#), con la parada por palabra ("para", "stop") sin
     pasar por el modelo (propuesta sin hacer); (c) más adelante, el servicio en el suelo (pedir la confirmación).
  6b. **Fusión de la Fase 7 hecha** (PR #63 y #64, 25/10/2026): la Pi ya está en `main` y sin la copia provisional
     `~/apps/tools-orders`. Si cambia el código de `src/ExplorerHat.Orders` (o `Common`), hay que volver a desplegar
     `~/apps/ExplorerHat.Orders` en la Pi (cada carpeta lleva su copia de `Common`). **Al escribir el
     cerebro:** `tools/medir-llm/herramientas.json` dice "hasta 3 s" en `retroceder` y el servicio recorta a **1 s**
     (la medida de los modelos se hizo con 3 s); cambiar la descripción de la herramienta a 1 s y repetir la medida si
     importa. El README del servicio dice "la misma lista cerrada": es la lista, no los límites.
  7. Ideas opcionales: `Brake()` en `HatMotor` (el DRV8833 frena con los dos pines en alto: paradas más cortas),
     medir la corriente de cada motor con el INA219 (Pimoroni dice 200 mA por canal), condensadores de 100 nF en los
     motores, un disipador o ventilador para la Pi y repetir en el suelo la medida del pin de 5 V. Para la pantalla, que
     se quita a menudo (17/10/2026): un conector con llave (por ejemplo, de tipo JST) en vez de cuatro Dupont sueltos, y
     comprobar si su placa tiene un puente de la luz de fondo (sin él se apagaría; sin comprobar en esta placa).
- **Sin prisa:** la guía de compilación ligera en la Pi (Fase 3); repetir con carga completa la batería Xiaomi
  (Fase 2). La pull-up del Trig de los HC-SR04P ya está medida (17/10/2026, ver el README y el "Estado actual").
  Duda pequeña que queda: si la pull-up de unos 62 kΩ es una pieza suelta o está dentro del chip (se ve en la
  placa). El 4,93 V del Trig frente a los 5,29 V de VCC ya está explicado (ver el "Estado actual").
- **Experimento de modelos (desde el 17/10/2026):** **decidido el 17/10/2026, al cerrar la sesión 3: el usuario
  elige la opción 1, mantener las reglas de `AGENTS.md` como están** (el asesor en las consultas de siempre y en las
  del proyecto, y el subagente `buscador`). No se quita ni se recorta nada. Lo de abajo es el historial de la
  decisión. Sonnet 5.5 con asesor Opus 5.5 y el subagente `buscador`
  (reglas en `AGENTS.md`, archivos en `.claude/`). Tras tres sesiones, revisar con `/usage` el coste, cuántas
  veces el asesor cambió una decisión y si `buscador` ahorró contexto. Si no aporta, quitar
  `.claude/settings.json`, `.claude/agents/buscador.md` y la sección de `AGENTS.md`. Se decidió **no usar JEV**
  (modelo de decisiones de TypeSafe AI, por API y de un tercero; no es determinista y los hooks de Claude Code ya
  resuelven las bifurcaciones simples). **Comprobado (17/10/2026, sesión nueva):** `/model` dice Sonnet 5.5 (la
  configuración global dice Opus, así que el proyecto se lee), `/advisor` marca Opus y la configuración global no
  define `advisorModel`. El comando `/agents` ya no existe en Claude Code (se editan los archivos de
  `.claude/agents/`). Primera prueba de `buscador`: 3 consultas y 9 s para una búsqueda en un archivo de
  `C:\work\iot`, con resultado correcto; para un solo archivo, leerlo directamente cuesta lo mismo.
  **Datos de `/usage`** (los pega el usuario; las tres sesiones están):
  - **Sesión 1** (la larga del 17/10/2026; 11 h de reloj, 1 h de API): **36,39 $** en total. **Sonnet 5.5: 18,26 $**
    (106,5 M de lectura de caché, 339 k de salida). **Opus 5.5, el asesor: 18,11 $** (4,2 M de entrada, 72,8 k de
    salida, **0 de caché**): **el asesor costó casi la mitad (50 %)**. Haiku (`buscador`): 0,02 $, una sola consulta.
    El 95 % del uso fue con más de 150 k de contexto. Semana: 9 % usado.
  - **Sesión 2, al empezar** (prompt de retomar leído): 0,29 $ (Sonnet 5.5: 259 k de lectura de caché y 56 k de
    escritura). Semana: 9 %.
  - **Sesión 2, qué hizo el asesor** (una llamada, al final): corrigió dos notas de `PLAN.md` con causa equivocada (el
    registro de pines sí servía; el `salida=` faltaba por bash y no por el pegado). Cambió el contenido, no el
    plan.
  - **Sesión 2, al cerrar** (31 min de reloj, 8 min de API, 65 peticiones): **2,49 $** en total. **Sonnet 5.5: 1,78 $**
    (71 %; 8,8 M de lectura de caché, 41 k de salida). **Opus 5.5, el asesor: 0,70 $** (28 %; una llamada con 139 k de
    entrada, 7 k de salida y 0 de caché). Haiku: 0,0007 $ (no se usó `buscador`: no hizo falta buscar en `C:\work\iot`).
    Semana: 10 % usado (un punto más que al empezar). El 88 % del uso de las últimas 24 h fue con más de 150 k de
    contexto; el dato incluye la sesión 1.
  - **Sesión 3, al empezar** (prompt de retomar leído): 0,00 $ en `/usage` (17 s de reloj, sin peticiones aún); ventana
    de sesión al 40 % y semana al 10 %.
  - **Sesión 3, al cerrar** (32 min de reloj, 11 min de API, 56 peticiones): **4,46 $** en total. **Sonnet 5.5: 2,20 $**
    (49 %; 9,7 M de lectura de caché, 168 k de escritura, 56 k de salida). **Opus 5.5, el asesor: 2,25 $** (50 %; tres
    llamadas, 460 k de entrada en total, unos 153 k por llamada, 21 k de salida y 0 de caché). Haiku: 0,0007 $ (no se
    usó `buscador`). Ventana de sesión: 47 %. Semana: 11 % (un punto más que al empezar). El 85 % del uso de las últimas
    24 h fue con más de 150 k de contexto (incluye las sesiones 1 y 2).
  - **Sesión 3, qué hizo el asesor** (tres llamadas: antes de fijar el plan, antes de mover los motores y al cerrar):
    1. *Antes del plan:* confirmó el plan y añadió comprobaciones: copiar la lógica del chip desde el fork y no
       reescribirla de memoria, no cambiar la configuración del chip, el orden de las pruebas (orden de los pads, luces,
       motores, parada) y preguntar qué pads usar.
    2. *Antes de mover los motores:* aprobó `PhantomCheck`; añadió que la lección con `moveMotors = true` debía
       probar la suelta con el motor girando (un pad "pegado" no lo ve `PhantomCheck`), usar una copia fuera del
       repositorio en vez de editar el `Program.cs`, y la lista de documentos que faltaban.
    3. *Al cerrar:* encontró un fallo real: los programas de prueba que mueven los motores no los reconoce la
       parada de emergencia (ver la Fase 5, lección 07), y que faltaba este registro. **Cambió una decisión de
       seguridad** (borrar los programas y fijar la regla de nombres); las dos primeras llamadas añadieron
       comprobaciones sin cambiar el plan.
  - **Lectura provisional (sesiones 1 y 2):** el coste lo dominan las sesiones largas con mucho contexto: la sesión 1
    costó 14 veces más que la 2. **El asesor lee todo el historial en cada llamada y sin caché, así que su coste
    crece con la longitud de la sesión** (0,70 $ por una llamada con 139 k de entrada; 18,11 $ en la sesión 1, con 4,2 M
    de entrada en total). Una sola llamada al final, como en la sesión 2, es barata y dio dos correcciones útiles.
    Decidir al cerrar la 3.
  - **Lectura de las tres sesiones (para decidir; la decisión es del usuario):**

    | Sesión | Total | Asesor | Llamadas | Lo que cambió el asesor |
    |---|---|---|---|---|
    | 1 (11 h) | 36,39 $ | 18,11 $ (50 %) | muchas | No medido |
    | 2 (31 min) | 2,49 $ | 0,70 $ (28 %) | 1 | Dos notas de `PLAN.md` con causa equivocada (cambió el contenido, no el plan) |
    | 3 (32 min) | 4,46 $ | 2,25 $ (50 %) | 3 | Un fallo de seguridad real (la parada no reconocía los programas de prueba) y varias comprobaciones |

    El asesor cuesta unos 0,70 a 0,75 $ por llamada con 140 a 150 k de entrada, sin caché, y su coste crece con la
    sesión. Con una o dos llamadas por sesión corta, sigue siendo barato; la sesión larga es lo que más gasta
    (la sesión 1 fue el 84 % del coste de las tres). `buscador` no se ha usado más que una vez (0,02 $).
    **Decisión del usuario (17/10/2026): la opción 1, mantener las reglas como están.** Las tres opciones eran:
    mantener las reglas de `AGENTS.md` como están; mantener el asesor solo en lo físico (antes de mover
    motores con código nuevo y al publicar fuera) y quitarlo del resto; o quitar el experimento (borrar
    `.claude/settings.json`, `.claude/agents/buscador.md` y la sección de `AGENTS.md`).
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
      **Corrección (04/10/2026):** el sentido es correcto, pero marcha atrás va más lento (Fase 6).
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
- [x] **¿Hace falta el cable micro-USB con la UPS? (04/10/2026)** Desde el 01/10/2026, la Pi tenía además un cable
      del puerto USB "5V OUT" de la UPS a su micro-USB (el 03/10/2026, el usuario cambió ese cable por uno más corto y
      grueso). Según Waveshare, la UPS alimenta la Pi por sus contactos de muelle (*pogo pins*, los pines de 5 V del
      GPIO) y "5V OUT" es para otros aparatos: todas las pruebas con la UPS se habían hecho con los dos caminos.
      Prueba con el método de esta fase (solo baterías, `tools/vigilar-tension.sh`), con cable y después sin él (la Pi
      apagada para quitarlo: arrancó solo con los contactos):

  | Tramo | Con cable: baterías (mín.) / corriente máx. | Sin cable: baterías (mín.) / corriente máx. |
  |---|---|---|
  | Reposo (1,5 min) | 7,88 V / 0,55 A | 7,82 V / 0,56 A |
  | Compilando la solución en la Pi (`--no-incremental`, ~4,5–5 min) | 7,80 V / 0,75 A | 7,72 V / 0,74 A |
  | ObstacleAvoidance N, ruedas en el aire (1 min) | 7,72 V / 1,14 A | 7,64 V / 1,11 A |
  | ObstacleAvoidance S, ruedas en el aire (1 min) | 7,70 V / 0,95 A | 7,65 V / 1,12 A |

  **Tensión baja el 0 % del tiempo en todos los tramos de los dos casos.** Las baterías bajan ~0,06 V de una prueba
  a otra por la descarga. Decisión: **sin cable**. Los dos caminos salen del mismo regulador, así que tenerlos a
  la vez no es peligroso; el micro-USB de la Pi tiene un fusible rearmable y los 5 V del GPIO no (es lo normal en
  las placas que alimentan la Pi por el GPIO). La prueba no mide el margen: la Pi solo avisa por debajo de ~4,63 V.
  - **Margen del pin de 5 V (04/10/2026, sin cable, solo baterías):** medido con el ADS1015 del HAT (Analog 1 → 10 kΩ
    → pin 5V; programa aparte `~/apps/RailLogger`, ~640 medidas por segundo con `DataRate.SPS860`) y
    `tools/vigilar-tension.sh` a la vez:

    | Tramo | 5 V: media | 5 V: mínimo | Medidas < 5,0 V | Baterías (mín.) | Corriente máx. |
    |---|---|---|---|---|---|
    | Reposo (30 s) | 5,302 V | 5,292 V | 0 de 17 841 | 8,24 V | 0,52 A |
    | ObstacleAvoidance N, ruedas en el aire (1 min) | 5,295 V | 5,220 V | 0 de 37 845 | 8,06 V | 1,18 A |
    | ObstacleAvoidance S, ruedas en el aire (1 min) | 5,296 V | 5,226 V | 0 de 37 771 | 8,00 V | 1,05 A |

    La caída mayor es de ~80 mV, al arrancar o frenar los motores: ~0,6 V de margen hasta el aviso de la Pi. Falta,
    si hiciera falta, repetirlo en el suelo (los motores piden más corriente al arrancar con el robot parado).
  - **Temperatura:** al compilar, en los dos casos, `get_throttled` = `0x80000` (bit 19): la CPU llegó al límite
    suave de 60 °C y bajó su frecuencia (52,6 °C al terminar; 46,7 °C al arrancar en frío). Compilar en la Pi tarda
    más por eso. Un disipador o un ventilador lo evitaría; importa también para la Fase 7. Además, con 1 GB de RAM la
    compilación llegó a usar la memoria de intercambio y la Pi tardó más de 20 s en aceptar una conexión SSH. Después
    de compilar, `dotnet build-server shutdown` libera ~300 MB (los procesos de MSBuild y del compilador se quedan
    esperando).
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
  - Con `HatMotor` (04/10/2026, modo S, 1 min en el suelo): 16 maniobras, las 15 completas con giros de **1 paso**
    (antes, 30 de 40) y ninguna repetida por seguir el obstáculo (antes, 7). Detecta a 20–30 cm (`OBSTACLE_DISTANCE`) y
    tras retroceder queda a 30–40 cm. Al usuario le pareció algo cerca; decidió dejar 30 cm.
  - Sujeción: hecha el 01/10/2026 (la UPS y la Pi, en un bloque atornillado al chasis). En campo libre, a veces no avanza en línea recta (los dos
    motores no giran igual): se corregirá con los sensores de velocidad (lección 10).

### Carrera en la parada de emergencia (17/10/2026, noche)
- **Fallo:** con Ctrl+C en la lección 07 (`TouchFloor`, que escribe en los motores cada 50 ms), el programa terminaba con
  `Unhandled exception. System.ObjectDisposedException ... 'HatMotor'` y `Aborted` (código 134), sin el mensaje
  "Parada de emergencia". Los pines quedaron en `lo` esa vez.
- **Causa:** `OnSignal` corre en otro hilo y llama a `Dispose()`. El hilo principal sigue en su bucle y, al escribir en
  un `HatMotor` ya cerrado, lanza la excepción. `Dispose()` había quitado el aviso de excepciones no controladas al
  empezar, así que nadie la recogía y el proceso se abortaba **a mitad de la limpieza** (sin liberar el controlador,
  sin forzar los ocho pines a bajo, sin cerrar el I2C). Riesgo real: en otra carrera, un pin habría podido quedar sin
  forzar. Los motores quedaban seguros porque `HatMotor.Dispose` deja sus pines en bajo antes de que el hilo principal
  pueda fallar.
- **Reproducción sin mover los motores:** una copia de la lección que escribe siempre velocidad 0 (`Lesson07.TouchStill`,
  no versionada) falló **10 de 10** veces con `timeout -s INT 6`. La lección con luces (`moveMotors = false`) **no** lo
  reproduce: las luces no lanzan nada tras cerrarse.
- **Arreglo en `SafeExplorerHat`:** (1) el aviso de excepciones y el registro de señales se quitan **al final** de
  `Dispose`: si el hilo principal falla, o llega una segunda señal, esperan al mismo `lock` y el proceso no se aborta
  antes de que los pines estén en `lo`; (2) `OnSignal` termina el proceso con `Environment.Exit(128 + señal)` después
  del mensaje (130 con INT, 143 con TERM, 129 con HUP).
- **Pruebas:** sin motores, 10 de 10 con Ctrl+C sin error y con mensaje; códigos 130, 143 y 129; dos `SIGTERM` seguidos
  (5 de 5): siempre sale el mensaje (a veces dos) y no hay errores. Con motores y **ruedas en el aire**: `SIGTERM` con
  el pad 1 tocado (a 0,45), proceso muerto en 0,15 s, código 143, 8 pines en `lo`; Ctrl+C del usuario con el pad
  tocado, mensaje y código 130; lección 09 con `SIGTERM`, 0,18 s, 8 pines en `lo`. Después: chip táctil en fábrica
  (`0x2A` = `0x80`, `0x1F` = `0x2F`) y `get_throttled` = `0x0`.
- **Reglas aprendidas** (también en `AGENTS.md`): (1) al cambiar `Common`, **volver a desplegar todo lo que lo usa**: cada
  programa de `~/apps` lleva su copia; (2) un trabajo en segundo plano de un `bash` no interactivo **ignora SIGINT**
  (`cmd & kill -INT $!` no hace nada): usar `timeout -s INT N cmd` o `kill -TERM`; (3) `pkill -f` y `pgrep -f` también
  encuentran el comando de la propia sesión SSH si contiene la ruta o el nombre (por ejemplo en un `cd`), aunque se use
  el truco de los corchetes: matar por PID.

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
  - [x] **Lecciones en las tareas y en la depuración (04/10/2026).** Las lecciones solo estaban en la tarea *Ejecutar una
        lección en la Pi*, y para depurar solo estaba BasicSample. Ahora:
    - *Desplegar en la Pi* y *Ejecutar en la Pi* tienen una sola lista con las lecciones y los programas, por su carpeta
      (`lessons/Lesson11.Screen`, `src/ExplorerHat.BasicSample`). En la Pi, el nombre de la carpeta lo da `$(basename ...)`:
      lo calcula el shell de la Pi (en macOS y Linux, antes el del PC, con el mismo resultado). Quitadas *Ejecutar una
      lección en la Pi* y *Desplegar BasicSample*.
    - **Depurar en la Pi el programa abierto** (sustituye a *Depurar BasicSample en la Pi*): despliega y depura el proyecto
      del archivo abierto en el editor (`${relativeFileDirname}` y `${fileDirnameBasename}`). Así la tarea previa y el
      depurador usan el mismo programa sin preguntarlo dos veces (las variables `${input:...}` de `launch.json` y de
      `tasks.json` se preguntan por separado).
    - La consola de depuración no tiene teclado: los programas que leen teclas (ObstacleAvoidance, SonarDashboard y las
      lecciones 04, 10 Odometry y 10 Square) se depuran con *Ejecutar en la Pi* y *Adjuntar a un programa en la Pi*.
    - Probado desde el PC (comandos de las tareas en `cmd.exe`, con la lección 11): los dos despliegues borran la carpeta
      anterior y copian la nueva; *Ejecutar en la Pi*, código 0. El usuario probó F5 con la lección 11 en VS Code: se
      detiene en el punto de interrupción y responde a F10 y F5.
    - Para probar una tarea fuera de VS Code: el comando, en un archivo `.cmd` ejecutado desde PowerShell. Desde Git Bash,
      `cmd.exe` encuentra el `ssh` de Git (sin la clave) y las comillas llegan mal.
- Descartado como opción principal: VS Code Remote-SSH ejecutándose en la Pi (1 GB de RAM se queda corto
  con la extensión de C#).
- [ ] Guía de compilación ligera para 1 GB de RAM y ~3,5 GB libres en la microSD.

## Fase 4: Documentación
- [x] README: montaje, cableado de los sensores HC-SR04 (niveles de 5 V → entradas del HAT), pinout,
      habilitar I2C, normas de seguridad en el taller. Datos confirmados con el usuario (01/10/2026): sensores
      HC-SR04P, cada uno en su soporte; VCC al 5V del HAT y GND al GND del HAT; TRIG directo a OUT1–OUT3, sin
      resistencia: como las salidas OUT son de colector abierto, la pull-up del TRIG debe estar en el módulo (en la
      foto de la placa no se ve si es una resistencia o la interna del chip U3, RCW9006; **medido el 17/10/2026 con el
      multímetro: unos 62–65 kΩ entre Trig y VCC en los tres sensores, igual en los dos sentidos, y 4,93 V entre Trig
      y GND con el robot encendido y sin programa**: hay una pull-up de unos 62 kΩ; si es pieza suelta o del chip no lo
      dice el multímetro), y el
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
`lessons/README.md`. Tareas de VS Code *Desplegar en la Pi* y *Ejecutar en la Pi* (eligen la lección) y depuración con F5
(Fase 3); `tools/parar-robot.sh`, `vigilar-pines.sh` y
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
      **Recalibrada con `HatMotor` (04/10/2026):** con 250 ms, las cuatro esquinas de ~120° (la rueda que va hacia
      atrás ya no frena); con **200 ms, ~90°** (calculado con el arranque del motor de ~40 ms y acertado a la primera).
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
- [x] 07 Pads táctiles (`Lesson07.Touch`): control remoto del robot. **Hecha y probada el 17/10/2026** (ruedas en el
      aire, con el visto bueno del asesor antes de mover los motores; y en el suelo, a 0,45, con la pantalla conectada).
  - **Decisión (17/10/2026): ayudante `TouchPads` en `ExplorerHat.Common`**, como `AnalogInputs` en la 08: habla por
    I2C con el CAP1208 (0x28), con la lógica de `Cap1208` del fork (`cap1xxx-binding`, PR #2616) copiada, porque el
    binding no está en `Iot.Device.Bindings` 4.2.0. API `hat.Touch.One.IsTouched()` a `Eight` (la forma del binding
    futuro); el I2C se abre al primer uso (comprueba producto `0x6B` y fabricante `0x5D`); se libera en `Dispose` de
    `SafeExplorerHat`, al final y en un `try`, igual que `Analog`. Se borra cuando el binding publicado tenga los pads.
    **No cambia la configuración del chip** (solo borra el bit INT del registro `0x00` tras leer): un toque a la
    vez, y no hay nada que deshacer en la parada de emergencia.
  - **Programa:** un solo `Program.cs`. Lee los pads 1 a 4 en cada vuelta del bucle (pausa de 50 ms, `maxTime` 60 s) y,
    con `if`/`else if`, enciende la luz del pad y fija la velocidad de las ruedas (pad 1 azul = avanzar, 2 amarilla =
    retroceder, 3 roja = izquierda, 4 verde = derecha; derecha = `One` atrás y `Two` delante). `moveMotors = false` por
    defecto (como la 06). **Colores:** al principio los puse como en la 03 (verde = avanzar) y el usuario pidió que
    cada pad encienda su luz, en el orden del HAT.
  - **Pruebas en la Pi:**
    1. **Orden de los pads** (`PadCheck`, no versionado, sin motores): los ocho pads salen con el número impreso en el
       HAT (1 a 8). Con dos pads a la vez, sale uno solo (el chip bloquea los toques múltiples de fábrica).
    2. **Luces** (la lección con `moveMotors = false`): cada pad enciende su luz, la luz se apaga **al instante** al
       soltar, un golpecito rápido parpadea y uno **rapidísimo a veces no llega a encenderse** (el programa
       pregunta ~15–20 veces por segundo): límite explicado en el README.
    3. **Toques fantasma** (`PhantomCheck`, no versionado; el asesor lo pidió antes de mover los motores): con nadie
       tocando, 0 toques en las ocho fases (quieto; motor `One` y motor `Two` en los dos sentidos a 0,6; los dos
       adelante a 0,6 y a 1,0; girar a la derecha). **El ruido de los motores no afecta a los pads.**
    4. **Motores** (copia de la lección con `moveMotors = true`): cada pad gira hacia el lado correcto, las ruedas
       paran al instante al soltar y ningún pad se queda "pegado". **Parada de emergencia con los motores girando:**
       `SIGTERM` (0 pines en alto a los 0,4 s, proceso muerto) y Ctrl+C (sin `| tee`), las dos bien.
    5. **Después de todo:** pines en `lo`, `get_throttled` = `0x0` y el chip con la configuración de fábrica
       (`0x2A` = `0x80`, `0x1F` = `0x2F`).
  - **Fallo encontrado por el asesor al cerrar (17/10/2026):** los dos programas de prueba que mueven los motores
    (`Lesson07Motors` y `PhantomCheck`, en `~/apps`) no los reconoce la parada de emergencia (`[L]esson[0-9]+[.]` pide
    un punto después del número). **Se borraron de la Pi** y `AGENTS.md` tiene ahora la regla: un programa de prueba
    que mueve los motores se llama `LessonNN.Algo`. Los usé con `SafeExplorerHat`, así que Ctrl+C y `SIGTERM` los
    paraban (probado), pero `tools/parar-robot.sh` no. Los fuentes (en el scratchpad de la sesión) no se versionan.
  - **Prueba en el suelo con la pantalla conectada (17/10/2026, noche):** el bus I2C mostró 0x27, 0x28, 0x42 y 0x48; con
    la lección original (`moveMotors = false`) y con la copia de motores a 0,6, todo bien con la pantalla puesta. El
    usuario dijo que a 0,6 el robot se seguía con dificultad. **Velocidad mínima en el suelo de pruebas:** con **0,4**
    arrancan los motores siempre (los cuatro pads); con **0,3** no arranca ninguno. Por eso la lección usa **0,45**
    (0,4 no tiene margen; en una alfombra u otro suelo podría calarse) y su README avisa ("las ruedas zumban y no se
    mueven: sube `speed`") y propone medir la velocidad mínima como reto. **Probado en el suelo a 0,45 (el usuario, con
    2 m libres):** los cuatro pads arrancan y todo fue bien; no hace falta una velocidad de giro aparte. Después: pines
    en `lo`, `get_throttled` = `0x0` y chip táctil en fábrica.
  - **Fallo encontrado: Ctrl+C abortaba el programa** (`Unhandled exception. System.ObjectDisposedException ... 'HatMotor'`
    y `Aborted`, sin el mensaje de la parada). Causa y arreglo en la Fase 2, "Carrera en la parada de emergencia".
  - **Prueba con una niña de 10 años (17/10/2026, antes de fusionar el #57):** en el suelo, a 0,45, lo hizo todo sin
    problemas y se adaptó a mantener el dedo en los pads. Queda por mirar con más niños (y con la edad del taller) si es
    cómodo con pads tan juntos, pero no hay ningún fallo conocido.
- [x] 08 Sensores analógicos (`Lesson08.Analog`): potenciómetro. **Hecha y probada el 17/10/2026.**
  - **Decisión del usuario (17/10/2026): la lección usa solo el potenciómetro, sin la LDR.** Montar la LDR pide una
    protoboard y complica el montaje, y el potenciómetro ya explica bien el tema (en el montaje del 17/10/2026 falló
    justo la protoboard: el 5V iba a una fila sin unir al HAT). Se quitaron del README y del programa la sección de
    la LDR y los retos de la luz. **Las medidas de las LDR, el divisor de 10 kΩ, y el texto de lección
    se guardaron en [`docs/fotorresistencias-LDR.md`](docs/fotorresistencias-LDR.md)** (índice de `docs/` en
    [`docs/README.md`](docs/README.md)); el programa `LdrCheck` pasó a `src/ExplorerHat.LdrCheck`, y las medidas están
    también aquí abajo para una lección futura (por ejemplo, con dos LDR en la Fase
    7: un vehículo de Braitenberg que sigue o huye de una linterna, "sentir, pensar y actuar"). Las 20 LDR siguen en
    el material.
  - **Qué es:** un único `Program.cs` con *top-level statements*: lee `hat.Analog.One.ReadVoltage().Volts`, lo pone en
    una escala de 0 a 1 (`minVolts` 0 y `maxVolts` 5,3, con `Math.Clamp`) y enciende de 0 a 4 luces (azul, amarilla,
    roja y verde) con cuatro `if` seguidos. Escribe los voltios y una barra en la terminal. Sin motores (el reto del
    motor va solo como texto, con aviso de las ruedas en el aire). 30 s y 10 lecturas por segundo. README con montaje,
    la trampa del cable negro, tabla de síntomas y los retos (sin LDR desde la decisión de arriba).
  - **Cómo lee las entradas (opción Z, elegida por el usuario el 17/10/2026):** ayudante `AnalogInputs` y `AnalogInput`
    en `ExplorerHat.Common`, con la forma del binding de dotnet/iot (`hat.Analog.One.ReadVoltage()`), sobre `Ads1115`
    de `Iot.Device.Bindings` 4.2.0. `SafeExplorerHat.Analog` abre el I2C al leer por primera vez y se libera al final del
    `Dispose`, dentro de un `try` y sin esperar más de 0,5 s (que un bus colgado no retrase la parada de emergencia).
    Cuando una versión publicada del binding tenga las entradas analógicas, se borran las dos clases y la lección no
    cambia. La decisión del 04/10/2026 (`ProjectReference` al fork) se descartó porque el tipo `ExplorerHat` del fork
    choca con el del paquete (`CS0433`) y habría obligado a pasar todo `Common` al fork y a repetir las 11 lecciones.
  - **Pruebas en la Pi (17/10/2026):** potenciómetro de 0,01 a 5,31 V (`PotCheck`); la lección con el mando al mínimo
    lee 0,01 V; parada con `SIGTERM` (mensaje, 8 pines en `lo`, ningún proceso) y con Ctrl+C del usuario con luces
    encendidas (bien); las luces siguen el mando según los umbrales de la tabla del README (0,66, 1,99, 3,31 y
    4,64 V); `Lesson01.Lights` con el `Common` nuevo termina con salida 0 y los 8 pines en `lo`;
    `get_throttled` = `0x0`. Solución compilada con 0 avisos.
  - **Prueba de motores con el `Common` nuevo (17/10/2026, `Lesson03.Motors`, ruedas en el aire, velocidad 0,8):**
    hecha con la consulta al asesor y la confirmación explícita del usuario. **(1) Ejecución completa:** adelante,
    atrás, giro a la derecha y giro a la izquierda; el usuario vio las ruedas girar como se esperaba y las luces en
    el orden verde, roja, amarilla y azul (la primera ejecución empezó sin aviso y no le dio tiempo a verla: se
    repitió con una cuenta atrás de 8 s); salida 0, 8 pines en `lo` y los pines grabados con `tools/vigilar-pines.sh`
    (en `~/pines-prueba03.log` de la Pi) coinciden con cada fase (adelante: 19 y 21; atrás: 20 y 26; derecha: 20 y
    21; izquierda: 19 y 26). **(2) Parada de emergencia:** `SIGTERM` enviado 0,3 s después de que el programa
    escribiera `Backwards` (esperando el texto en el registro, no con un tiempo fijo, porque .NET tarda de 3 a 8 s en
    arrancar): salida 143, mensaje "Parada de emergencia (SIGTERM): motores parados", **los cuatro pines de los
    motores en `lo` 0,24 s después de la señal** (registro en `~/pines-prueba03-term.log`), ningún proceso y
    `get_throttled` = `0x0`; el usuario vio que las ruedas se pararon en el acto. **Quedan sin probar con el `Common`
    nuevo** las lecciones con motores 04, 06, 09 y 10 (usan el mismo `SafeExplorerHat`; solo cambia la propiedad
    `Analog` y el final del `Dispose`, y la lección 03 ya ejerce los dos caminos de la parada).
  - **Sensores decididos (10/10/2026):**
    - **Potenciómetro** lineal de 10 kΩ (B10K). Pedidos 3 **ALLECIN WH148 B10K** (película de carbono, con mando y
      cable con conector XH2.54 de 3 pines), **ya llegaron** (confirmado el 16/10/2026). Mirar qué hay en el otro extremo del
      cable (hembra dupont o cables sueltos) y qué color va a la pata central (la del medio en el cuerpo del
      potenciómetro): el color de los cables no lo dice. Los extremos a 5V y GND y la pata central a Analog 1: al girar el mando, da de 0 a 5 V (divisor de tensión). Es la
      mejor forma de ver qué es "analógico": el niño cambia la tensión con la mano. Retos: la velocidad de un motor
      (ruedas en el aire), las 4 luces como barra de nivel, los voltios en la pantalla. **La pata central nunca va a
      5V ni a GND:** al final del recorrido uniría 5V y GND casi sin resistencia.
    - **Fotorresistencia** (LDR) en serie con una resistencia fija de 10 kΩ (ya las hay): el niño construye el
      divisor. Retos: luces que se encienden solas a oscuras y, con dos LDR, un robot que sigue una linterna o huye
      de ella (vehículo de Braitenberg; prepara "sentir, pensar y actuar" de la Fase 7). Los módulos KY-018 ya traen la
      resistencia fija, pero así se pierde construir el divisor. **Ya las hay:** una bolsa de 20, de modelo sin
      confirmar (10/10/2026). La resistencia fija debe parecerse a la de la LDR con la luz de la sala, para que la
      tensión cambie mucho entre luz y sombra: medir la LDR con el ADS1015 y elegir la resistencia con ese dato.
  - **Resultados de las medidas con el multímetro (17/10/2026).** ExtraStar DT830B, con los componentes sueltos y
    pinchados en una protoboard (así no se tocan con los dedos):
    - **El multímetro:** puntas juntas en `200`: 0,5–0,6 Ω (cables y contactos). Puntas separadas: "1" (fuera de
      rango). Resistencia de 10 kΩ con la cuarta banda dorada (5 %) en `20k`: 9,65 kΩ, dentro de las dos
      tolerancias.
    - **LDR** (se midieron 3, casi idénticas: mismo modelo):

      | Condición | Lectura | Resistencia |
      |---|---|---|
      | Luz de la sala (55–60 lux con el móvil), rango `2000k` | 22–24 | ~23 kΩ (error de ese rango: ±10 kΩ) |
      | Luz de unas gafas con lupa apuntando a la LDR, rango `200k` | 12,1–12,4 | ~12,2 kΩ (~170 lux, estimado) |
      | Dedo encima, rango `200k` | 108,1–112,5 | ~110 kΩ |
      | A oscuras (cuarto sin luz), rango `2000k` | "1" | > 2 MΩ |

      A oscuras da fuera de rango (> 2 MΩ), pero eso **no descarta ningún modelo**: la columna "a oscuras" de la
      tabla de arriba suele ser un mínimo de la hoja de datos (sin confirmar). El dato que sí distingue es el de la
      luz: con 57 lux y 23 kΩ, y una ley de R ∝ lux^−0,6, saldrían unos 65 kΩ con 10 lux: **GL5539 (o GL5537-2)**.
      Para que fuera una GL5528 (10–20 kΩ con 10 lux), la sala tendría que estar a unos 5 lux. Solo orientativo (el
      sensor de luz del móvil es poco exacto). No hace falta el modelo exacto.
    - **Divisor para una lección futura (la lección 08 ya no usa la LDR). Decisión del usuario (17/10/2026):
      empezar con 10 kΩ y dejar 22 kΩ como reto; no hay de 22 kΩ, así que queda por estudiar** (dos de 10 kΩ en serie
      dan 20 kΩ): 5V → LDR → nodo → **10 kΩ** → GND, y el nodo a una entrada analógica: más luz, más voltios. Tensiones esperadas (5 V × Rf / (R_LDR + Rf); con los 5,30 V reales del pin 5V, multiplicar por 1,06):

      | Condición | R de la LDR | Con 10 kΩ | Con 22 kΩ (rojo, rojo, naranja; reto opcional) |
      |---|---|---|---|
      | Oscuridad | > 2 MΩ | < 0,03 V | < 0,06 V |
      | Dedo encima | 110 kΩ | 0,42 V | 0,83 V |
      | Luz de la sala | 23 kΩ | 1,52 V | 2,44 V |
      | Luz de las gafas | 12,2 kΩ | 2,25 V | 3,22 V |
    - **Potenciómetro WH148 B10K** (conector XH de 3 hilos; se midió uno, falta comprobar los otros dos). Con las
      patas hacia arriba, de izquierda a derecha: rojo, negro, amarillo. Rojo–amarillo: 9,33 kΩ siempre (la pista
      entera, dentro del ±20 % habitual y del error del multímetro). **El negro es la pata central (el cursor), no
      un GND:**

      | Mando | Rojo–negro | Negro–amarillo |
      |---|---|---|
      | Mínimo | 9,33 kΩ | 0,00 kΩ |
      | 1/4 | 7,45 kΩ | 1,97 kΩ |
      | 1/2 | 4,76 kΩ | 4,41 kΩ |
      | 3/4 | 2,06 kΩ | 7,31 kΩ |
      | Máximo | 0,00 kΩ | 9,33 kΩ |

      Las dos mitades suman 9,2–9,4 kΩ en todas las posiciones, y negro–amarillo / total da 0, 0,21, 0,47, 0,78 y 1
      (casi lineal, como una B). **Montaje: rojo → 5V, amarillo → GND, negro → Analog 1:** da 0 V con el mando al
      mínimo y 5 V al máximo (~1,1, 2,4 y 3,9 V a 1/4, 1/2 y 3/4). **Trampa:** por el color, el negro parecería ir a
      GND; así el cursor iría a GND y, al girar el mando hacia el rojo, 5V quedaría unido a GND casi sin resistencia.
      Etiquetar el conector ("NEGRO = SEÑAL"). Antes de dar corriente, con el robot apagado y sin los cables al HAT:
      montar el potenciómetro en la protoboard y medir entre las líneas + y −; debe dar ~9,33 kΩ con el mando en
      cualquier posición (si en un extremo da ~0 Ω, el cableado está mal). **Para el código:** el pin 5V del HAT da
      ~5,30 V, no 5,0: con el mando al máximo, Analog 1 leerá ~5,3 V. No suponer 5,0 V como tope al pasar la
      lectura a velocidad o a luces (usar un tope o dividir entre la tensión medida).
    - **Probado en la Pi el 17/10/2026** con `~/apps/PotCheck` (no versionado; fuente en `~/apps/PotCheck/src`; lee
      Analog 1 con `Ads1115` de 4.2.0 y muestra voltios y una barra; `dotnet PotCheck.dll all` muestra las cuatro
      entradas). Adaptador: tres Dupont macho–macho en la carcasa hembra XH2.54 del potenciómetro (sirven los
      contactos de la carcasa, hechos para pines de 0,64 mm), con etiquetas "5V", "SEÑAL" y "GND". Primer intento:
      Analog 1 fija en **0,02 V** (Analog 2–4 flotantes a ~0,5 V): el cable SEÑAL llegaba, pero el 5V iba a una fila de
      una protoboard **sin unir al pin 5V del HAT**. La pista no tenía tensión y el cursor leía 0 V con cualquier giro.
      Solución: el cable 5V al nodo de 5V de la protoboard de la Pi (el del sensor de velocidad LM393 de la rueda
      derecha). Resultado: de **0,01 V** (mando al mínimo) a **5,31 V** (al máximo, igual que los 5,30 V del pin 5V),
      lineal, y bajando igual al girar al revés. `get_throttled` = `0x0` y los 8 pines en `lo`. **Lección para los
      niños:** una protoboard no lleva alimentación por sí sola; una entrada flotante marca ~0,5 V y una entrada atada
      a masa, ~0,02 V. La prueba de continuidad (zumbador del multímetro) entre la fila y el pin 5V comprueba esa
      unión sin dar corriente.
    - **Problema para usar el fork en la lección (17/10/2026):** `SafeExplorerHat` usa `Iot.Device.ExplorerHat.ExplorerHat`
      del paquete `Iot.Device.Bindings` 4.2.0, y el proyecto `ExplorerHat` del fork tiene un tipo con el mismo nombre
      completo. Comprobado con un proyecto que referencia `ExplorerHat.Common` y el `.csproj` del fork: `error CS0433:
      el tipo 'ExplorerHat' existe en 'ExplorerHat' y en 'Iot.Device.Bindings'`. Con el fork, `Common` tendría que dejar
      el paquete y depender del fork para todos sus enlaces (`ExplorerHat`, `DCMotor`, `Hcsr04`, pantalla, ...), y las
      11 lecciones habría que probarlas otra vez. **Decisión del usuario (17/10/2026): opción Z** (ayudante en `Common`, ver
      arriba); descartadas la Y (solo la lección 08 con el fork, sin `SafeExplorerHat` ni parada de emergencia) y la X
      (`Common` entero con el fork).
  - **Medir las LDR con el robot (preparado el 10/10/2026; ya no hace falta: se midieron con el multímetro).** Plan
    antiguo, sin multímetro. Las LDR no llevan marca;
    las de 5 mm suelen ser de la serie GL55xx, iguales por fuera (una bolsa puede ser un surtido de varios modelos).
    Se distinguen por la resistencia con 10 lux y a oscuras (tras 10 s), según sus hojas de datos (aproximado):

    | Modelo | Con 10 lux | A oscuras |
    |---|---|---|
    | GL5506 | 2–5 kΩ | 0,2 MΩ |
    | GL5516 | 5–10 kΩ | 0,5 MΩ |
    | GL5528 | 10–20 kΩ | 1 MΩ |
    | GL5537-1 | 20–30 kΩ | 2 MΩ |
    | GL5537-2 | 30–50 kΩ | 3 MΩ |
    | GL5539 | 50–100 kΩ | 5 MΩ |
    | GL5549 | 100–200 kΩ | 10 MΩ |

    - **Programa `ExplorerHat.LdrCheck`** (en `src/ExplorerHat.LdrCheck` y en la solución desde el 17/10/2026, con su
      README; antes solo existía en la Pi, en `~/apps/LdrCheck`, que se hizo a mano y puede quedarse: vale la de
      `~/apps/ExplorerHat.LdrCheck`; el nodo va a Analog 1, donde está el potenciómetro): `Ads1115` de
      4.2.0, sin el HAT (no toca ningún pin). Para cada LDR pide su nombre, los lux de la sala (opcional, de una
      aplicación del móvil) y tres medidas de 2 s que empiezan con Enter: luz de la sala, tapada con un dedo y a
      oscuras (espera 10 s). Al final, una tabla. Calcula R = 10 kΩ × (Vfuente / V − 1). Cada lectura usa la escala
      más pequeña del ADS1015 que admite la tensión (hasta ±0,256 V: 0,125 mV por paso), para medir bien varios MΩ.
      Fuente: si Analog 2 → 10 kΩ → 5V está conectado, la mide; si no, supone 5,30 V. Avisa si el nodo está casi a 0 V
      o casi a la tensión de la fuente (un cable suelto).
    - **Montaje**, en una protoboard de 400 puntos (no la pequeña del HAT, que tiene el nodo de 3,3 V de los LM393), con
      el robot apagado: 5V del HAT → línea +, GND → línea −, LDR entre la línea + y una fila (el nodo), 10 kΩ entre esa
      fila y la línea − (bandas marrón, negro, naranja; o marrón, negro, negro, rojo con 5 bandas), y un cable del nodo
      a Analog 1. Opcional: Analog 2 → 10 kΩ → 5V. Numerar las LDR (son iguales). Cambiar solo la LDR con el robot
      encendido no es peligroso: limita la corriente ella misma (unos pocos mA como mucho).
    - **Comando** para la terminal del usuario: `cd ~/apps/ExplorerHat.LdrCheck && dotnet ExplorerHat.LdrCheck.dll | tee ~/ldr.log` (en
      `~`, no en `/tmp`: en la Pi, `/tmp` es `tmpfs` y se borra al reiniciar). Después, leer `~/ldr.log`, comparar con
      la tabla y anotar aquí los resultados.
    - Los dos, a 5 V y con menos de 1 mA: el ADS1015 del HAT admite de 0 a 5 V (Fase 6).
  - **Descartados:** el TCRT5000 como primer sensor analógico (su tensión depende de la altura, del color del suelo y
    de la luz ambiente: se ve peor la causa y el efecto; sigue siendo el de la lección 10, y puede ir por las entradas
    analógicas, sin entradas digitales libres); micrófono KY-038 (el sonido cambia miles de veces por segundo y el
    ADS1015 lee ~500 por segundo con el binding); temperatura TMP36 o LM35 (cambia muy despacio); gas serie MQ
    (calentador de ~150 mA y minutos para estabilizarse); distancia por infrarrojos Sharp GP2Y0A21 (curva no lineal y
    picos de ruido; para más adelante); joystick analógico (la lección 07 ya controla el robot con los pads).
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
  - Con `HatMotor` (04/10/2026, misma zona, 60 s): 18 obstáculos, detectados casi siempre a 18–29 cm; giros de 1 paso
    (12 veces), 2 (5) y 3 (1); ninguna maniobra agotó los 10 pasos. El usuario lo vio bien. Sin cambios en la lección.
- [x] **Repaso con el `Common` nuevo (lecciones con motores 04, 06, 09 y 10).** Decidido por el usuario el 17/10/2026
      (opción A). Motivo: `SafeExplorerHat` cambió (propiedad `Analog` y final del `Dispose`) y solo se probó la
      lección 03 con los motores (ejecución completa y parada con `SIGTERM`; ver la lección 08). Las lecciones no
      cambian: se comprueba que **siguen haciendo lo mismo y que la parada de emergencia funciona con sus hilos y
      sensores**. Unos 40 minutos, con el usuario delante.
  - **Antes de empezar (en este orden):**
    1. La Pi en `main` y al día (`git pull --ff-only` en `~/work/explorer-hat`), sin programas del robot en marcha
       (`P=Lesson; pgrep -fc "${P}[0-9]"` en una llamada SSH **que no contenga nombres de lección**: si no, cuenta su
       propio comando), los 8 pines en `lo` (`pinctrl get 19,20,21,26,4,17,27,5`) y `get_throttled` = `0x0`.
    2. `i2cdetect` (`/usr/sbin/i2cdetect -y 1`) con 0x28, 0x42 y 0x48; **el 0x27 (pantalla) solo si está conectada**:
       el usuario la desconecta a menudo y ninguna de estas lecciones la usa. Si el montaje cambió desde el
       17/10/2026, que el usuario ejecute antes `bash tools/probar-cableado.sh`.
    3. **Confirmación explícita del usuario de que el robot tiene las ruedas en el aire** (sobre un bote o un libro,
       sin tocar la mesa) **antes de cada ejecución** (`AskUserQuestion`), aunque ya haya dicho "adelante". El asesor ya
       se consultó para el `Common` nuevo (lección 03); no hace falta otra vez, salvo que el código cambie.
    4. Cada ejecución **con una cuenta atrás de 8 s** para que el usuario pueda mirar (el 17/10/2026 la primera
       ejecución empezó sin aviso y no la vio). Parada de emergencia preparada desde el PC:
       `ssh.exe harlequin 'bash -s' < tools/parar-robot.sh`.
    5. Pines grabados con `tools/vigilar-pines.sh` (`setsid -f ... > /dev/null 2>&1 < /dev/null`, el registro en `~`).
  - **Cómo desplegar y ejecutar:** `dotnet publish lessons/LessonNN.X -c Debug -r linux-arm64 --self-contained false
    -o publish/lessons/LessonNN.X`, borrar `~/apps/LessonNN.X`, `scp.exe -r` y `chmod -R go-w` (o la tarea de VS Code
    *Desplegar en la Pi*). Las lecciones sin teclado (06, 09 y 10 Ir recto) se pueden lanzar desde el PC por SSH con
    `timeout`; **cada `timeout` debe superar la duración de la lección más unos 10 s de arranque de .NET** (por
    ejemplo, `timeout 90` para la 09, que dura 60 s): si no, envía un `SIGTERM` que nadie planeó. **Las que leen el
    teclado (04, 10 Contar vueltas y 10 El cuadrado perfecto) las ejecuta el usuario en su terminal de la Pi** (es lo
    normal: está delante, controla el arranque y tiene Ctrl+C), con `cd ~/apps/X && dotnet X.dll`. Alternativa, si no
    puede: mandarle las teclas por SSH, con una espera de **12 s o más** antes de la tecla (.NET tarda de 3 a 8 s en
    arrancar y la tecla antes de eso se pierde): `(sleep 12; printf 'n'; sleep 30) | timeout 70 ssh.exe -tt harlequin
    'cd ~/apps/X && timeout --foreground 50 dotnet X.dll; exit'` (sin `--foreground` el programa no recibe las teclas).
  - **Orden y qué debe verse** (siempre con las ruedas en el aire):

    | Lección | Teclado | Duración | Qué debe verse |
    |---|---|---|---|
    | 04 Cuadrado (responder `n`) | Sí | ~8 s | Cuatro veces: las dos ruedas adelante 1 s y un giro a la derecha (derecha atrás, izquierda adelante, 200 ms) |
    | 06 Distancia, **tal como está** (`moveMotors = false`) | No | hasta 10 s | Solo mide y enciende luces (verde, amarilla, roja) con la mano delante del sensor central. Los motores no se mueven |
    | 06 Distancia, **con `moveMotors = true`** (ver la receta debajo de la tabla) | No | hasta 10 s | Lejos: verde y rápido. Menos de 60 cm: amarilla y lento. Menos de 30 cm: roja, para y termina |
    | 10 Contar vueltas | Sí (una tecla entre las partes) | ~2 s + hasta 5 s | Parte 1: adelante 2 s contando los pulsos de la rueda derecha. Parte 2: hasta 50 cm o 5 s |
    | 10 Ir recto | No | hasta 6 s | Adelante 100 cm o 6 s, luces verde, amarilla o azul; los dos sensores cuentan |
    | 10 El cuadrado perfecto (responder `n`) | Sí | hasta ~25 s | Cuatro lados y cuatro giros por pulsos |
    | 09 Robot autónomo | No | 60 s o Ctrl+C | Mano en el sensor izquierdo: retrocede y gira a la derecha; en el derecho: gira a la izquierda; sin obstáculo: adelante. **Aquí se prueba la parada de emergencia, en dos ejecuciones**, con tres sensores y los hilos de los motores en marcha: una con `SIGTERM` desde el PC y otra con Ctrl+C del usuario en su terminal, a mitad (ver la receta debajo de la tabla) |

  - **Receta de la lección 06 con motores:** el `.csproj` de la lección hereda `TargetFramework` de
    `Directory.Build.props` y su `ProjectReference` es relativa, así que **una copia fuera del repositorio no compila**.
    Cambiar `moveMotors = true` en `lessons/Lesson06.Distance/Program.cs` del árbol de trabajo, publicar en una
    carpeta de salida propia y desplegar en `~/apps/Lesson06.Distance` (el proceso sigue llamándose
    `Lesson06.Distance`: `parar-robot.sh` lo reconoce). **Después de la prueba:** `git restore
    lessons/Lesson06.Distance/Program.cs`, comprobar que `git status` queda limpio y volver a desplegar la original.
  - **Receta de la parada de la lección 09 con `SIGTERM`:** lanzarla en segundo plano en la Pi con la salida a un
    archivo (en `~`) y guardar su PID; el usuario pone la mano delante del sensor **izquierdo**; esperar a que el
    archivo contenga **`Obstacle!`** (se imprime al ver el obstáculo, justo antes de retroceder 300 ms) con un
    `timeout 30`, esperar unos 0,15 s y enviar `kill -TERM` al PID: cae en la marcha atrás, que es el caso que
    importa. Si no sale `Obstacle!`, el sensor no vio la mano: repetir. El `timeout` general, de 90 s. Después:
    "Parada de emergencia (SIGTERM)", salida 143, pines en `lo`, ningún proceso. El método (esperar un texto del
    programa y no un tiempo fijo) es el de la lección 03 del 17/10/2026, que esperó `Backwards`.
  - **Después de cada ejecución:** salida 0 (o 143 si se paró con `SIGTERM`), 8 pines en `lo`, ningún proceso,
    `get_throttled` = `0x0`, y que el usuario diga si las ruedas y las luces hicieron lo esperado.
  - **Si algo no cuadra** (una rueda al revés, una que no para, un pin que no baja): **no seguir con la lección
    siguiente.** Lanzar `parar-robot.sh`, anotar el síntoma y llamar al asesor (error que no se arregla a la primera).
  - **No hace falta probar en el suelo:** el `Common` nuevo no cambia el movimiento. Solo si el usuario quiere repetir
    la 04 o la 10 Cuadrado perfecto allí.
  - **Al terminar:** anotar los resultados en esta sección (qué lección, qué se vio, tiempos de los pines y de la
    parada), en una rama nueva desde `main` (`fase-5-repaso-motores-common`), con su PR. Pedir al usuario `/usage` (al
    empezar y al terminar la sesión) para el experimento de modelos.
  - **Resultados (17/10/2026, noche, desde el PC; todo con las ruedas en el aire):** los seis programas (unas doce
    ejecuciones) salieron bien y en ninguna quedó un pin de motor en alto sin programa (registro `~/pines-repaso.log` con `vigilar-pines.sh`,
    2400 s). Antes: Pi en `main` (`69254dc`), `i2cdetect` con 0x28, 0x42 y 0x48 (pantalla desconectada) y los programas
    desplegados desde `main` con el `Common` nuevo. Después de cada ejecución: ningún proceso, los 8 pines en `lo` y
    `get_throttled` = `0x0`.
    - **04 Cuadrado** (`n`): cuatro lados y cuatro giros a la derecha, bien.
    - **06 Distancia, sin motores:** dos ejecuciones. El programa **termina** al bajar de 30 cm (`29 cm: stop!`,
      "The end", salida 0), aunque `moveMotors` sea `false`: con la mano a menos de 30 cm al arrancar, termina al
      instante. El usuario esperaba motores y no vio nada en la primera (la mano no se movió).
    - **06 Distancia, con `moveMotors = true`** (receta del árbol de trabajo, restaurada y vuelta a desplegar): lejos
      verde y rápido, entre 30 y 60 cm amarilla y lento, por debajo de 30 cm roja, paran y termina. El usuario la
      ejecutó varias veces, bien.
    - **10 Contar vueltas:** parte 1, 85 pulsos en 2 s (86,8 cm, 43 cm/s) y 4 pulsos de más al parar; parte 2, paró a
      los 49 pulsos (50 cm) y 4 de más.
    - **10 Ir recto:** izquierda 103 pulsos (105,2 cm), derecha 101 (103,1 cm), diferencia de 2 pulsos.
    - **10 El cuadrado perfecto** (`n`): cuatro lados (30 pulsos a la izquierda y 28 a la derecha cada uno) y cuatro
      giros de 6 pulsos en cada rueda; deslizamiento de 2 a 5 pulsos. "Square finished!".
    - **09 Robot autónomo, parada de emergencia:** con `SIGTERM` desde el PC, a mitad de la marcha atrás (se esperó a
      `Obstacle!`, con la mano a 9 cm del sensor izquierdo): "Parada de emergencia (SIGTERM): motores parados", salida
      143 y los pines en `lo`; las dos ruedas pararon. Con Ctrl+C del usuario, a mitad de la marcha: "Parada de
      emergencia (SIGINT): motores parados" y las dos ruedas pararon.
    - **Tiempos de los pines en la parada** (registro de `vigilar-pines.sh`, una muestra cada 0,25 s): con `SIGTERM`,
      la muestra con la marcha atrás (pines 20 y 26 en `hi`) es la de las 21:21:59.4 y la siguiente (21:21:59.6, cuando
      ya no hay proceso de la lección) tiene los 4 pines en `lo`: **menos de 0,25 s**. Con Ctrl+C, en las dos ejecuciones
      los pines pasan a `lo` **en la misma muestra en que desaparece el proceso** (menos de 0,3 s). Para leer el
      registro durante una prueba lanzada por SSH, no filtrar por `procs=1`: el `bash -c`, `setsid`, `timeout` y
      `dotnet` cuentan como programas del robot (`procs=4`). Mirar el tramo completo (por ejemplo,
      `sed -n 2485,2515p ~/pines-repaso.log`).
    - **Dos lecciones del repaso:** (1) **`| tee` esconde el mensaje de parada con Ctrl+C**: Ctrl+C va a todos los
      procesos de la línea y `tee` muere antes de que el programa escriba. Para probar la parada con Ctrl+C, ejecutar el
      programa **sin `tee`** (en `AGENTS.md`). (2) **`pgrep -fc "Lesson[0-9]"` cuenta el propio comando** si este
      contiene un nombre de lección (pasó tres veces al comprobar el despliegue; la regla ya está en `AGENTS.md`).
      Para una ejecución con cuenta atrás, `sleep 8` en el PC y después el `ssh` es suficiente, pero .NET tarda de 3 a
      8 s más en arrancar: el usuario debe esperar a ver moverse las ruedas (o la salida del programa) antes de poner la
      mano.
    - **La línea `salida=` no sale con Ctrl+C, y no es un fallo del robot** (comprobado el 17/10/2026, sin motores): en
      la terminal de la Pi, `sleep 5; echo hecho` y Ctrl+C **no escribe "hecho"**. Bash interactivo no ejecuta el resto de
      la lista (`cmd; echo ...`) cuando el programa en primer plano muere por `SIGINT`. Para ver el código de salida
      tras Ctrl+C, escribir `echo $?` en la línea siguiente (se espera 130); no se midió con la lección 09.
- [x] 10 Odometría: contar vueltas con los discos de 20 ranuras del chasis (vienen en el kit) y dos sensores
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
      quitarlo. Informado en dotnet/iot#2614 (Fase 6). Con la excepción, `SafeExplorerHat` paró el motor (`Error inesperado: motores parados`).
  - [x] Lección `Lesson10.Odometry` con un sensor (03/10/2026): parte 1 mide la velocidad (2 s), espera una tecla para
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
        un ESP32 que cuente los dos sensores. **Corrección (03/10/2026):** el Explorer HAT sí deja GPIO libres
        accesibles, en la fila lateral marcada "3.3V ONLY": SDA (2), SCL (3), PWM (18), MOSI (10), MISO (9), SCK (11),
        CS (8), RX (15), TX (14) y 3v3. Van directos a la Pi, sin protección: solo 3,3 V. Opción mejor: el LM393
        izquierdo alimentado desde 3v3 y su D0 en PWM (GPIO 18), sin quitar el HC-SR04 izquierdo. Pendiente de
        probar que el LM393 funciona bien a 3,3 V.
        **Hecho (03/10/2026, rama `fase-5-todos-los-sensores`):** nodo de 3,3 V en la protoboard pequeña del HAT
        (cable desde el pin 3v3), LM393 izquierdo a 3,3 V con D0 en PWM (GPIO 18) y el HC-SR04 izquierdo de vuelta en
        IN3. Todo conectado a la vez: los tres HC-SR04 (SonarDashboard: 22/24, 24/24 y 23/24 lecturas), los dos LM393 y
        la pantalla (0x27). Diagnóstico con motores en el aire: el LM393 a 3,3 V cuenta igual que el de 5 V
        (0,4 → 34,7 frente a 33,7 pulsos/s; 1,0 → 57,0 frente a 55,7) y con **0 saltos** en GPIO 18, frente a 46–70 en
        IN4: la entrada de la Pi tiene histéresis (disparador de Schmitt) y el búfer de las entradas IN del HAT, por lo
        visto, no. `Lesson10.Straight` y `Lesson10.Square` leen ahora la rueda izquierda en GPIO 18. Probado `Lesson10.Straight` en el aire:
        104 y 102 pulsos en 1 m, código 0 y pines a nivel bajo.
  - [x] Corregir la desviación en línea recta: `Lesson10.Straight` (03/10/2026). Cuenta los dos sensores cada ~1 ms
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
  - [x] Girar ángulos exactos con los pulsos y el cuadrado de la lección 04 con pulsos: `Lesson10.Square`
        (03/10/2026, rama `fase-5-giros-exactos`). Lados de 30 cm con la corrección de la parte 2; giros sobre sí
        mismo en los que cada rueda se para por separado al llegar a sus pulsos, menos `brakePulses` por lo que resbala;
        modo calibración (se para tras cada giro y muestra lo que ha resbalado cada rueda). 90° = 10 pulsos (9° por
        pulso: la resolución es gruesa).
    - En el aire (`brakePulses` 1, objetivo 9): la rueda que va hacia delante cuenta 11–12 (sigue rodando mientras la
      otra llega) y 1–3 más de inercia; lados de 29 pulsos y 4–5 de inercia. Código 0, pines a nivel bajo.
    - En el suelo (`brakePulses` 1): cada rueda 9–10 pulsos y **3–4 resbalando** (~12,5 en total ≈ 112°); el usuario
      midió **~115°**: el cálculo con la distancia entre ruedas encaja, sobra lo que resbala. Lados: 29 pulsos y 5–7
      más (~36 cm en lugar de 30). Cambiado a `brakePulses` 3 (objetivo 7 pulsos).
    - En el suelo (`brakePulses` 3, batería al ~70 %): **las cuatro esquinas, casi 90°** según el usuario; cada rueda
      7 pulsos (una vez 8) y 1–3 resbalando; un lado se desvió muy poco. Margen de ±1 pulso (~9°): el sensor solo
      cuenta el inicio de cada ranura.
    - **Recalibrada con `HatMotor` (04/10/2026):** con `brakePulses` 3, ~120° (la izquierda resbaló 2–4 pulsos y la
      derecha, que va hacia atrás, 5–6; 11–13 pulsos en total); con 5, ~70° (8–10 en total); con **4, ~90°** (cada
      rueda para a los 6–7, la izquierda resbala 2–3 y la derecha 3–6: ~10 de media, ±10° entre esquinas). Lados: 28–30
      pulsos y 6–8 más al frenar, como antes; algo desviados. Sin caídas de tensión en estas pruebas (`0x0`; baterías
      a 7,78 V como mínimo, 1,13 A como máximo).
    - **Avería del 04/10/2026 (tarde): `Lesson10.Odometry` contaba 0 pulsos** en las dos partes (también con F5), sin
      ningún error. Diagnóstico:
      - Grabación de los pines cada ~11 ms durante la lección (script en `/tmp`, como `tools/vigilar-pines.sh` pero con
        las entradas; en `pinctrl get`, el nivel de una entrada es el campo anterior a `//`): PWM en los pines 19 y 21,
        443 cambios en el GPIO 18 (sensor izquierdo) y **0 cambios en el GPIO 25** (siempre `lo`).
      - Sin programa, girando a mano la rueda derecha: 48 y 78 cambios en el GPIO 25. Con el motor derecho a toda
        velocidad (`pinctrl set 19 op dh`): 0 cambios. **La rueda derecha no giraba**: el usuario creía que giraban
        las dos.
      - Causa: **el cable azul del motor derecho, suelto en la borna MOTOR 1.** Apretado, la lección cuenta bien en el
        aire: 92 pulsos en 2 s (47 cm/s, con `HatMotor`) y parada a los 49 pulsos con 5 más de inercia.
      - Cambios: el mensaje `Time is up!` de `Lesson10.Odometry` y `Lesson10.Straight` pregunta también si giran las
        ruedas, y el README de la lección tiene la sección "Si no cuenta ningún pulso".
      - Método para otra vez: si un sensor no cuenta, girar la rueda a mano (descarta el sensor) y mirar si la rueda gira
        con el motor (los pines solo dicen lo que manda la Pi).
    - La UPS se quedó sin batería durante las pruebas (la Pi se reinició). El programa desplegado justo antes quedó
      con `runtimeconfig.json` vacío (el corte llegó antes de escribir la caché en la microSD): las tareas de
      despliegue de VS Code ejecutan ahora `sync` después de copiar. `git fsck` en la Pi, sin daños. Distancia entre las ruedas: **13 cm** (medida por el usuario): una vuelta sobre sí mismo son 40,8 cm de
        cada rueda y 90° unos 10 pulsos.
- [x] 11 Pantalla: LCD de 20×4 caracteres (2004A) con adaptador I2C (PCF8574). Ideas: escribir texto y
      variables, encajar un mensaje en 20 columnas, y un panel del robot sin terminal (distancias, batería de la UPS,
      obstáculos esquivados) para las pruebas en el suelo. `Iot.Device.Bindings` 4.2.0 trae `Lcd2004` y `Pcf8574`.
      Antes de conectarla:
  - Dirección I2C: 0x27 (PCF8574T) o 0x3F (PCF8574AT), según el chip; los puentes A0–A2 la cambian. No choca con
    0x28, 0x42 ni 0x48.
  - **Tensión del bus I2C:** la pantalla necesita 5 V, y el adaptador suele llevar resistencias de pull-up de SDA y
    SCL a su VCC: con VCC a 5 V, SDA y SCL de la Pi (3,3 V) quedarían por encima de 3,3 V. Medir esas resistencias
    y decidir: quitarlas (la Pi ya tiene pull-ups a 3,3 V), un conversor de niveles o alimentar el adaptador a 3,3 V
    (el contraste puede no bastar).
  - Por dónde se llega a SDA (GPIO 2) y SCL (GPIO 3): la fila lateral del Explorer HAT marcada "3.3V ONLY".
  - **Probada a 3,3 V (03/10/2026):** adaptador con VCC al pin 3v3 de esa fila, SDA y SCL a los suyos y GND al
    nodo de GND. `i2cdetect` la ve en **0x27** (PCF8574T, A0–A2 sin puentear). Con `Lcd2004` + `Pcf8574` (RS 0,
    RW 1, E 2, luz 3, datos 4–7) escribe bien las 4 líneas; al principio no se veían las letras y aparecieron,
    perfectas, girando el potenciómetro de contraste. **No hace falta conversor de niveles ni quitar las
    resistencias de 4,7 kΩ:** a 3,3 V el bus no pasa de 3,3 V. Programa de prueba aparte (no versionado).
  - **Lección `Lesson11.Screen` (03/10/2026, rama `fase-5-leccion-12`):** clase `Screen` en `ExplorerHat.Common`
    (`Clear()` y `Write(línea, texto)`, que corta o rellena a 20 caracteres). Tres partes, sin motores: texto con el
    nombre del niño y una línea demasiado larga, contar del 1 al 10 con su doble y un panel de 20 s con la distancia
    del sensor central (filtro de la 06) y una barra de `#` (uno por cada 10 cm) y "Too near!" a menos de 30 cm.
    Probada en la Pi: código 0; el usuario vio la línea cortada en 20 caracteres, la cuenta, la barra siguiendo a
    la mano con fluidez, "Too near!" y la despedida.

### Ideas para ampliar las lecciones
- **Siguelíneas con dos sensores infrarrojos TCRT5000** (antes, lección 10). **Descartada por ahora** (decisión del
  usuario, 17/10/2026): aporta poca variedad (otra entrada digital o analógica, como las lecciones 05, 08 y 10) y obliga
  a cambiar el cableado de un montaje que ya está probado. No se borra, se guarda como idea. Lo que sabemos:
  * La rama `features/line-tracker` (2020–2022), ya borrada de GitHub, solo tenía el esqueleto: un `Program.cs` sin
    lógica y un ejemplo en Python copiado de un tutorial. Hay que hacerla desde cero.
  * Hay dos módulos TCRT5000 (03/10/2026, por confirmar). Necesitan dos entradas digitales: mismo problema que el
    segundo sensor de la lección 10 (solo IN4 está libre).
  * Con la salida analógica (`A0`), la tensión depende de la altura y del color del suelo (ver los motivos del
    descarte en la lección 08). Con la salida digital (`D0`), el potenciómetro del módulo fija el umbral.
  * Hace falta una pista (cinta aislante negra sobre un suelo claro) y, por tanto, un espacio para el taller.

### Componentes disponibles (03/10/2026)
- 10 sensores de velocidad LM393 (horquilla óptica, 4 pines: VCC, GND, D0, A0): lección 10 y, si hace falta, 05.
- 2 módulos TCRT5000 (infrarrojo por reflexión, con potenciómetro; por confirmar): idea de la lección siguelíneas (ver "Ideas para ampliar las lecciones").
- Pantalla LCD 2004A con adaptador I2C PCF8574: lección 11 (propuesta).
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
- 20 fotorresistencias (LDR, modelo sin confirmar; encontradas el 10/10/2026): lección 08.
- **Pedidos (10/10/2026), para la lección 08:** 3 potenciómetros ALLECIN WH148 B10K (10 kΩ lineales, con mando y
  cable con conector XH2.54 de 3 pines). Llegan el 11/10/2026.
- IN4 (GPIO 25) con nada conectado lee 0 en 50 de 50 lecturas (pull de la Pi desactivado): la entrada del HAT
  parece tener pull-down o un búfer que la mantiene baja. **Confirmado con el pulsador en la lección 05:** las
  entradas del HAT llevan pull-down por dentro. Su valor no se ha medido (con el multímetro, entre IN4 y GND con el
  HAT apagado y sin nada conectado a IN4; opcional).

## Fase 6: Binding `Iot.Device.ExplorerHat` en dotnet/iot
Estado: el binding sigue en el repositorio (activo, último cambio en el binding en julio de 2026), pero
solo cubre motores y las 4 luces. Falta:
- [~] 4 entradas digitales (GPIO 23, 22, 24, 25, tolerantes a 5 V) y 4 salidas de colector abierto (GPIO 6, 12, 13,
      16). **Hechas el 04/10/2026** en la rama `explorerhat-inputs-outputs` del fork (sobre `explorerhat-dispose`, la
      del #2613). **PR abierto el 16/10/2026: [dotnet/iot#2625](https://github.com/dotnet/iot/pull/2625)**, rama
      `explorerhat-io` (el mismo commit, copiado con `cherry-pick` encima de `upstream/main` tras fusionar el #2613).
  - API: `hat.Inputs.One`…`Four` (`DigitalInput`: `Read()` → `PinValue`, `Pin`) y `hat.Outputs.One`…`Four`
    (`DigitalOutput`: `On()`, `Off()`, `IsOn`, `Pin`). `On()` = la salida une el cable a 0 V (ULN2003A), como Pimoroni.
    Cada pin se abre la primera vez que se usa (`Read`, `On` u `Off`), no al crear `ExplorerHat`; `Dispose` apaga las
    salidas usadas y cierra solo los pines que abrió. Usarlas después de `Dispose` lanza `ObjectDisposedException`.
    Quitadas del `.csproj` las carpetas que no existían. README y ejemplo del binding al día.
  - 6 pruebas nuevas (`InputsOutputsTests`, con `SetInputValue` nuevo en `FakeGpioDriver`); las 8 de `ExplorerHat` y
    las 7 de `DCMotor` pasan. Con tres fallos metidos a propósito (la salida no se apaga al liberar, las entradas se
    abren al crear el HAT, se cierra un pin abierto por otro), falla alguna prueba en cada caso.
  - Prueba en la Pi sin motores (programa aparte `~/apps/IoCheck`, no versionado): al crear el HAT, ninguno de los 8
    pines abierto; el HC-SR04 central (`Hcsr04` con su propio controlador en OUT1 + IN1) mide bien con el HAT creado
    (51 cm y ~7 cm con la mano); IN4 contó 48 pulsos del LM393 derecho girando la rueda a mano 10 s; OUT4: `On()` →
    pin 16 `hi`, `Off()` → `lo`, y al liberar con la salida encendida, `lo` y los 8 pines cerrados. El LED de la
    lección 05 no estaba montado: el programa mostraba `pinctrl get 16` tras cada paso (que el pin 16 en alto
    enciende ese LED se comprobó el 03/10/2026).
- [~] 4 entradas analógicas (ADS1015, I2C 0x48). **Hechas el 04/10/2026** en la rama `explorerhat-analog` del fork;
      desde el 16/10/2026, rama `explorerhat-analog-v2` (encima de `explorerhat-io`, la del #2625). Pendiente: abrir el
      PR cuando fusionen el #2625 (método en el "Estado actual").
  - API: `hat.Analog.One`…`Four` (`AnalogInput.ReadVoltage()` → `ElectricPotential`); Analog 1–4 = canales 3, 2, 1 y 0
    del ADS1015. Usa `Ads1115` con ±6,144 V, una medida cada vez y `DataRate.SPS128` (en el ADS1015, 1600 por segundo, su
    velocidad de fábrica; en modo de una medida, `Ads1115` consulta el chip hasta que acaba, sin esperar según la
    velocidad). Constructor nuevo `ExplorerHat(GpioController?, I2cBus?, bool shouldDispose)`: sin bus, abre el bus 1 la
    primera vez que se lee una entrada analógica (servirá también para el CAP1208 de los pads, en el mismo bus).
    El constructor anterior se mantiene. README y ejemplo del binding al día.
  - 6 pruebas nuevas con un ADS1015 y un bus I2C simulados (`SimulatedAds1015`, `SimulatedI2cBus`); pasan las 14 de
    `ExplorerHat`. Con cuatro fallos metidos a propósito (canales en el orden del chip, el chip creado al crear el HAT,
    rango de 4,096 V y liberar siempre el bus del que llama), falla alguna prueba en cada caso.
  - Prueba en la Pi sin motores (programa aparte `~/apps/AnalogHatCheck`, no versionado; Analog 1 al nodo de 3,3 V):
    3,294 V en las 1000 lecturas seguidas, **508 lecturas por segundo**, las entradas sueltas a 0,52–0,57 V, y un
    segundo `ExplorerHat` después de liberar el primero lee bien (el bus y el chip quedan liberados).
  - **Medido en la Pi (04/10/2026)** con `Ads1115` de 4.2.0 (±6,144 V, una medida cada vez; programa aparte
    `~/apps/AnalogCheck`) y una resistencia de 10 kΩ en serie, para no dañar el chip si fuera a 3,3 V (con 5 V, la
    corriente por sus diodos de protección no pasaría de ~0,14 mA). Analog 1 → 10 kΩ → 3,3 V: **3,294 V**; → 5 V:
    **5,301 V**, sin recortar. **El ADS1015 va alimentado a 5 V: las entradas admiten de 0 a 5 V**, y no hay divisor de
    tensión (lo que llega al conector es lo que mide el chip). Pimoroni no lo documenta.
  - **`Ads1115` lee bien el ADS1015:** los 12 bits van en la parte alta (`0x6E70` = 28 272 / 32 768 × 6,144 V =
    5,301 V; los 4 bits bajos, a 0). Con ±6,144 V, 3 mV por paso.
  - Una entrada sin nada conectado marca ~0,6 V (flota), no 0; con un cable suelto, entre 0,3 y 1,1 V.
  - Al escribir la configuración, el chip hace una medida (bit OS = 1) y, mientras mide, el bit 15 se lee como 0
    (`0x0583` justo después de escribir `0x8583`). Configuración de fábrica: `0x8583`.
- [~] 8 pads táctiles capacitivos (CAP1208, I2C 0x28). No había binding CAP1xxx en dotnet/iot.
      **Binding nuevo `Cap1208`: PR abierto el 03/10/2026, [dotnet/iot#2616](https://github.com/dotnet/iot/pull/2616)**,
      rama `cap1xxx-binding` del fork (`src/devices/Cap1xxx`). **Fusionado el 18/10/2026 (`5cf07458`); rama
      borrada.** Pendiente: los pads en `ExplorerHat` (PR 4 del orden acordado).
  - API: `ReadTouchedInputs()` (lee el registro 0x03 y borra solo el bit INT: devuelve los toques desde la lectura
    anterior más los que siguen), `MultipleTouchBlocking`, `Sensitivity` y `Recalibrate()`. El constructor comprueba los
    ID (`0x6B`, `0x5D`) y no cambia la configuración. Entradas `SensorInputs.Input1`–`Input8` (`[Flags]`), numeradas
    como en la hoja de datos (CS1–CS8).
  - 19 pruebas unitarias con un CAP1208 simulado (`I2cSimulatedDeviceBase`). Con dos fallos metidos a propósito, fallan
    1 y 3 pruebas. Lista de bindings regenerada con `tools/device-listing`.
  - **Comportamiento del chip comprobado en el robot** (primero con `i2cget` en un bucle, después con el binding;
    programa no versionado en `~/apps/Cap1208Check`, cada fase termina con Enter):
    - Pads 1–4 = bits `0x10`–`0x80`; pads 5–8 = `0x01`–`0x08`, como dice Pimoroni.
    - El bit de un pad sigue en 1 mientras el dedo está encima (0,3–0,6 s en un toque normal) y se borra al soltar y
      borrar INT.
    - **De fábrica, el chip solo admite un toque a la vez** (`MULT_BLK_EN`): con dos pads, solo sale uno y el registro
      0x02 marca `MULT` (`0x05`). Con `MultipleTouchBlocking = false` salen 2 y 3 pads juntos.
    - Con 32x (de fábrica) responden los 8 pads. **Con 1x no se detecta ningún toque**, ni apretando fuerte.
    - Al terminar, el programa deja la configuración de fábrica (`0x2A` = `0x80`, `0x1F` = `0x2F`).
- [~] **Prioritario, confirmado en 4.2.0:** `ExplorerHat` pasa su `GpioController` a `Motors`, `Lights`, cada
      `Led` y cada `DCMotor` con `shouldDispose = true` (valor por defecto), así que el primero que se libera
      cierra todos los pines. Los hilos de `SoftwarePwmChannel` de los motores siguen escribiendo y el
      proceso muere. Arreglo: pasar `shouldDispose: false` a los hijos, liberar los motores antes que las
      luces y el controlador el último. Comprobar también que al liberar un motor que iba marcha atrás no quede el pin de dirección en alto.
      **Sigue igual en la rama `main` de dotnet/iot (03/10/2026):** `Motors`, `Lights` y `Led` tienen `internal ...
      (GpioController? controller = null, bool shouldDispose = true)`, `ExplorerHat` los crea con `new Motors(_controller)`
      y `new Lights(_controller)`, y en `Dispose` libera `Lights` antes que `Motors`; `DCMotor.Create(..., _controller)`
      también usa `shouldDispose` por defecto. El último cambio del binding (PR #2586, 02/07/2026) solo movió las
      asignaciones `= null` fuera del `if (_shouldDispose)`.
      **PR abierto el 03/10/2026: [dotnet/iot#2613](https://github.com/dotnet/iot/pull/2613)**, rama
      `explorerhat-dispose` del fork. **Fusionado el 15/10/2026** (`1856d767`, aprobado por pgrawehr). Al revisar el
      código salieron dos fallos de `DCMotor`, en el mismo PR (el de `ExplorerHat` no basta sin ellos):
  - `DCMotor.Create` (las variantes con números de pin) crea el `SoftwarePwmChannel` con `shouldDispose = true`:
    al liberar el motor se libera el controlador aunque se pida `shouldDispose: false`, y otro motor que lo comparte
    tumba el proceso desde su hilo de PWM.
  - **Fallo de seguridad en `DCMotor2PinNoEnable`** (el tipo de motor del Explorer HAT: el DRV8833 tiene dos
    entradas por motor, sin pin de activación). Marcha atrás, el pin de dirección está en alto; al liberar, el pin de
    PWM queda en bajo y el de dirección sigue en alto: para el DRV8833 eso es **toda la tensión al motor**. El
    controlador de la Pi 3 no cambia el pin al cerrarlo, así que la rueda sigue a toda velocidad después de `Dispose`
    y después de terminar el programa. Arreglo: poner en bajo el pin de dirección al liberar. `DCMotor3Pin` y
    `DCMotor2PinWithBiDirectionalPin` no tienen el fallo: su PWM va al pin de activación.
  - Arreglo de `ExplorerHat`: los hijos no liberan el controlador, los motores se liberan antes que las luces,
    `Lights` libera cada `Led` y `ExplorerHat(controller, shouldDispose: false)` ya no libera el del que llama.
  - Pruebas unitarias nuevas (`src/devices/DCMotor/tests` y `src/devices/ExplorerHat/tests`, 9 casos) con un
    `GpioDriver` falso que, como la Pi, conserva el valor de un pin al cerrarlo. Con el código original, las de un
    motor fallan (pin de dirección en alto a −0,5 y −1; controlador del que llama liberado) y las de dos motores y
    las de `ExplorerHat` tumban el proceso de pruebas (`ObjectDisposedException` en el hilo de PWM).
  - Prueba en la Pi con las ruedas en el aire (programas aparte, no versionados: `~/apps/DisposeCheckNew` con el
    binding arreglado y `~/apps/DisposeCheckOld` con 4.2.0; caso `hat`: luces, `One` a −0,6 y `Two` a 0,6 y
    `Dispose`; caso `motor`: `DCMotor.Create(19, 20)` a −0,6 y `Dispose`; `tools/parar-robot.sh` después de cada
    uno). El usuario lo repitió en su terminal mirando la rueda derecha:

    | Bindings | Caso | Resultado | Pines 19 y 20 tras `Dispose` | Rueda derecha |
    |---|---|---|---|---|
    | Arreglado | `hat` | código 0 | `lo`, `lo` (los 8, `lo`) | se para |
    | Arreglado | `motor` | código 0 | `lo`, `lo` | se para |
    | 4.2.0 | `hat` | el proceso se cae (código 134, `Can not get a pin mode of a pin that is not open`) | `lo`, **`hi`** | **sigue a toda velocidad** |
    | 4.2.0 | `motor` | código 0, sin error | `lo`, **`hi`** | **sigue a toda velocidad** |

  - **Revisión del 08/10/2026, respondida el 09/10/2026** con el commit `15efc1ae` (sin *force push*) y una respuesta en
    cada hilo:
    - pgrawehr pidió lanzar una excepción si no hay controlador y `shouldDispose` es false, porque esa combinación no
      es válida (antes, el código la corregía sin avisar). Hecho en las tres variantes de `DCMotor.Create` con números
      de pin: `ArgumentException` para `shouldDispose`. Le preguntamos si también las variantes que reciben un
      `PwmChannel` (hoy las clases de motor fuerzan `shouldDispose` a true sin controlador): cambia el comportamiento
      para quien ya las usa. Y un detalle de estilo: `Controller != null`.
    - Copilot (gravedad alta): con `Create(PwmChannel, directionPin, controller)`, el canal lo crea el usuario y puede
      liberar el controlador compartido (`SoftwarePwmChannel` tiene `shouldDispose = true` por defecto). Entonces
      `IsPinOpen` lanzaba `ObjectDisposedException` y el pin de dirección quedaba en alto: la rueda, a toda velocidad.
      pgrawehr respondió que el PR ya crea los canales con `shouldDispose: false`, pero eso solo vale para las
      variantes con números de pin. Arreglo: `Dispose` pone en bajo el pin de dirección **antes** de liberar el canal,
      en el mismo orden que el setter de `Speed`.
    - Sobre ese orden: marcha atrás, el PWM alterna "frenar" (los dos pines en alto) y "atrás" (PWM en bajo). Al bajar
      antes el pin de dirección, el resto del ciclo en curso empuja hacia delante (menos de 20 ms a 50 Hz), igual que
      al cambiar `Speed` de negativo a 0. `SoftwarePwmChannel.Stop()` no cambia el pin; `Dispose()` lo pone en bajo
      al terminar su hilo.
    - 3 pruebas nuevas (pasan las 10 de `DCMotor` y las de `ExplorerHat`); con el código anterior fallan las 3. Sin
      prueba en la Pi: en el robot (el mismo controlador para todo) el resultado no cambia.
    - La integración continua arrancó sola al subir el commit (antes la lanzaba un mantenedor con `/azp run`).
  - Cuando salga una versión con el arreglo, `SafeExplorerHat` podrá dejar de usar `SharedGpioController`; la parada
    ante señales y el forzado a nivel bajo de los pines siguen haciendo falta.
- [x] **Fallo en el binding `Ina219` (4.2.0 y rama principal en octubre de 2026):** `ReadShuntVoltage()` y
      `ReadCurrent()` leen el registro como número sin signo (`ReadRegister` devuelve `ushort`), así que las tensiones
      y corrientes negativas (por ejemplo, baterías descargándose en la UPS HAT (B)) salen como valores enormes.
      Arreglo: convertir a `short` antes de escalar. **Es la incidencia abierta #1659 de dotnet/iot** ("INA219 - strange
      readings", 2021): sin carga, la corriente salta entre 0 y 799 mA. Con su calibración (12,2 µA por unidad),
      −1 unidad leída sin signo da 65535 × 12,2 µA = 799,5 mA. Los mantenedores no tenían el chip para investigarla;
      nosotros sí (UPS HAT (B), 0x42). Último cambio del binding: febrero de 2022.
      **PR abierto el 03/10/2026: [dotnet/iot#2612](https://github.com/dotnet/iot/pull/2612)** ("Fixes #1659"), rama
      `ina219-signed-readings` del fork. **Fusionado el 08/10/2026** (`d84c9407`), aprobado por pgrawehr y raffaeler,
      sin cambios pedidos; rama borrada. UpsDashboard sigue leyendo los registros directamente hasta que salga una
      versión de `Iot.Device.Bindings` con el arreglo.
  - Al revisar el binding aparecieron dos fallos más, en el mismo PR:
    - `ReadBusVoltage()` convierte a `short` un registro que no tiene signo: con el rango de 32 V, desde 16,384 V
      la tensión sale negativa. La UPS no llega a esa tensión; lo comprueban las pruebas unitarias.
    - El setter de `ShuntAdcResolutionOrSamples` desplaza el valor 4 bits y lo escribe en el campo del ADC del bus
      (BADC) en lugar del suyo (SADC): los valores de la enumeración ya están en la posición de SADC.
    - `ReadPower()` no cambia: el registro de potencia no tiene signo (en la prueba, positivo con la corriente negativa).
  - Pruebas unitarias nuevas (`src/devices/Ina219/tests`, con un INA219 simulado como el de `Ina236`): 23 casos; con
    el código original fallan 10 (−1 en el registro de corriente da 799,527 mA) y con el arreglo pasan todos.
  - Prueba en la Pi con la UPS (programa aparte, no versionado; 32 V, ±320 mV, `SetCalibration(4096, 1e-4f)`, es
    decir, 0,1 mA por unidad; 2 minutos, quitando el cargador a mitad):

    | | Cargando (13 lecturas) | Descargando (107 lecturas) |
    |---|---|---|
    | `ReadCurrent()` con el arreglo | +474 a +544 mA | **−339 a −458 mA** |
    | El mismo registro leído sin signo (4.2.0) | +365 a +535 mA | **6093 a 6215 mA** |
    | `ReadPower()` | ~4,2 W | ~2,9 W |

    Configuración tras el setter arreglado: `0x39EF` (BADC 12 bits, SADC 32 muestras). Al terminar, `Reset()` deja
    la UPS como estaba: `0x399F` (la configuración de fábrica) y calibración `0x0000`.
- [x] Control del DRV8833 marcha atrás: el sentido es correcto (`DCMotor2PinNoEnable`, pin de dirección + PWM
      invertido), pero la velocidad no (siguiente punto).
- [x] **Marcha atrás más lenta con `DCMotor2PinNoEnable` (04/10/2026).** Al comprobar el cableado, el usuario vio las
      ruedas más lentas hacia atrás que hacia delante.
  - **Medida** (programa aparte, no versionado: `~/apps/ReverseSpeed`; las dos ruedas a la vez, en el aire, 1 s para
    coger velocidad y 3 s contando pulsos de los LM393), pulsos por segundo, media de las dos ruedas:

    | Velocidad | Delante | Atrás con `DCMotor` | Atrás con `HatMotor` |
    |---|---|---|---|
    | 0,4 | 41–44 | 16 (~36 %) | 42 (102 %) |
    | 0,6 | 51–52 | 30 (~57 %) | 48 (93 %) |
    | 0,8 | 57 | 47 (~81 %) | 57 (100 %) |
    | 1,0 | 61 | 61 (100 %) | 61 (100 %) |

  - **Causa:** el DRV8833 tiene dos entradas por motor: alto/bajo = tensión hacia delante, bajo/alto = hacia atrás,
    bajo/bajo = el motor gira libre, alto/alto = **frena** (une los dos cables del motor: freno dinámico).
    `DCMotor2PinNoEnable` pone el PWM en el pin 19 (o 21): hacia delante, el 20 en bajo y en la parte apagada del
    ciclo el motor gira libre; hacia atrás, el 20 en alto y el PWM invertido (`1.0 + val`), así que en la parte
    apagada los dos pines están en alto y el motor **frena**. Comprobado grabando los pines con `pinctrl` mientras
    giraba: a 0,4, delante 43 % con tensión y 57 % libre; atrás 43 % con tensión y 57 % frenando. A 1,0 no hay parte
    apagada y las velocidades coinciden.
  - **La frecuencia no lo arregla** (programa aparte `~/apps/PwmFrequency`, con `SoftwarePwmChannel` propio):

    | Frecuencia | 0,4 delante / atrás | 0,6 delante / atrás |
    |---|---|---|
    | 50 Hz (la de `DCMotor`) | 43 / 17 | 52 / 31 |
    | 100 Hz | 43 / 14 | 52 / 34 |
    | 200 Hz | 41 / 18 | 52 / 34 |
    | 500 Hz | 8 y 32 (irregular) / 15 | 53 / 38 |
    | 1000 Hz | 45 / 29 | 44 / 29 |
    | 2000 Hz | 44 / 29 | 45 / 29 |

    Hasta 200 Hz, la bobina del motor (mucho menos de 1 ms) llega en cada parte apagada a la corriente de frenado
    completa: frena lo mismo a cualquier frecuencia. Desde 500 Hz, `SoftwarePwmChannel` (que cede el procesador
    mientras espera) ya no sigue la velocidad pedida: a 1000 y 2000 Hz, 0,4 y 0,6 dan lo mismo. `pinctrl` muestrea
    cada ~7 ms: sus cifras no sirven por encima de 200 Hz. Haría falta PWM por hardware de varios kHz, y de los pines
    de los motores solo el 19 lo tiene (el GPIO 18 de la fila lateral también, pero no va al DRV8833).
  - **La librería de Python de Pimoroni** pone el PWM (100 Hz) en los dos pines: hacia delante en el 20 ("forward")
    con el 19 en bajo, y hacia atrás en el 19 ("backward") con el 20 en bajo. El motor gira libre en los dos sentidos.
    Su "delante" es el "atrás" de dotnet/iot: con ella, este robot iría al revés.
  - **Arreglo en el robot:** `HatMotor` y `HatMotors` en `ExplorerHat.Common` (README de Common): un
    `SoftwarePwmChannel` a 100 Hz en cada pin, el mismo sentido que `DCMotor`. `SafeExplorerHat` crea el
    `ExplorerHat` del binding solo por sus luces, libera sus motores y mueve los mismos pines con `HatMotors`; las
    lecciones y los ejemplos no cambian. Probado en el aire: pines en la parte apagada siempre libres (alto/alto el
    0 % del tiempo), las seis fases de sentido bien (`~/apps/MotorCheck`, el usuario mirando las ruedas), parada de
    emergencia con SIGINT, SIGTERM y SIGHUP a −0,6 (los 9 pines en `lo`, el mensaje de parada y ningún proceso) y la
    lección 01 (código 0). CPU del proceso: ~7 % de un núcleo con los motores parados y ~20–25 % en marcha (antes,
    ~15 %).
  - **Informado el 04/10/2026: [dotnet/iot#2617](https://github.com/dotnet/iot/issues/2617)** (plantilla *Feature
    request*, con encabezados propios). Propone `DCMotor.Create(PwmChannel forwardChannel, PwmChannel backwardChannel)`
    en el binding `DCMotor` (sirve para cualquier puente en H de dos entradas: DRV8833, L9110S, MX1508...) y que
    `ExplorerHat` lo use en `Motors`. Cita la #844 (2019, cerrada): alguien vio la marcha atrás más lenta y el
    mantenedor respondió que el cálculo `1.0 + val` era correcto; lo es, lo que cambia es la parte apagada del ciclo.
    Pregunta la forma de la API, si se añade la variante que frena en los dos sentidos (*slow decay*) y si
    `ExplorerHat` debe cambiar. El código, en su propio PR después del #2613 (toca `DCMotor.cs` y `Motors.cs`).
  - Especificación de Pimoroni: *"Two H-bridge motor drivers (up to 200mA per channel; soft PWM control)"*. El
    DRV8833 admite 1,5 A por canal y se protege solo; un motor TT gasta ~100–200 mA en el aire y más al arrancar.
    Si hace falta, se puede medir con el INA219 de la UPS (motores parados frente a en marcha).
- [x] **Incidencia en `System.Device.Gpio` (Pi 3, `RaspberryPi3Driver`):** quitar un aviso de eventos
      con `UnregisterCallbackForPinValueChangedEvent` lanza `GpiodException: Device or resource busy` (detalles en la
      lección 11 de la Fase 5). Buscada el 03/10/2026: no hay ninguna igual (la más parecida, #1637, es de la Pi 4 y
      está cerrada). **Informada el 03/10/2026: [dotnet/iot#2614](https://github.com/dotnet/iot/issues/2614)**, solo
      la incidencia (el robot no necesita el arreglo).
  - Reproducida con un programa mínimo (no versionado, `~/apps/GpioEventsOld` con 4.2.0 y `~/apps/GpioEventsMain` con
    `main`), igual en los dos: tras el primer `RegisterCallbackForPinValueChangedEvent`, fallan un segundo registro,
    quitar un aviso y `WaitForEvent`. `OpenPin` y `ClosePin` funcionan.
  - Causa: `RaspberryPi3LinuxDriver` llama a `_interruptDriver.OpenPin` antes de cada operación con eventos. La Pi
    tiene libgpiod 2.2.1 (`libgpiod.so.3`, Debian 13), así que el controlador de eventos es `LibGpiodV2Driver`, y su
    `OpenPin` vuelve a pedir la línea al kernel (el de libgpiod v1 no hace nada si ya está abierta). Las pruebas de
    hardware de dotnet/iot se ejecutan en Raspbian 11 (libgpiod v1): por eso no lo ven.
  - Forma de evitarlo: un solo aviso por pin, sin quitarlo nunca.
  - **PR [dotnet/iot#2610](https://github.com/dotnet/iot/pull/2610) de pgrawehr ("Various LibgpiodV2 fixes", abierto),
    probado el 03/10/2026:** su `LibGpiodV2Driver.OpenPin` ya no vuelve a pedir una línea abierta, y eso arregla la
    #2614 (el PR no la cita). Programa de prueba (no versionado; `~/apps/GpioEvents2610` con el PR en `a2740357` y
    `~/apps/GpioEventsOld2` con 4.2.0): **GPIO 8 (CS) sin nada conectado y flancos hechos con `pinctrl set 8 pu` /
    `pd`** (la resistencia interna fija el nivel de un pin que flota), 4 flancos por paso, contando los que recibe cada
    aviso. Así se prueban los eventos sin cablear nada.

    | Secuencia | 4.2.0 | PR #2610 |
    |---|---|---|
    | Un aviso: registrar, quitar y volver a registrar | `Device or resource busy` | bien |
    | `WaitForEvent` dos veces seguidas, o antes de un aviso | `Device or resource busy` | bien |
    | Dos avisos en el pin y quitar uno | `Device or resource busy` | **el otro deja de recibir flancos** y quitarlo da `InvalidOperationException` |

    Causa del último caso: con el PR, `RaspberryPi3LinuxDriver.RemoveCallbackForPinValueChangedEvent` siempre llama a
    `_interruptDriver.ClosePin`, que libera la línea y borra todos sus avisos (`LibGpiodV2Driver` sí comprueba si quedan
    otros). **Comentado en el PR el 03/10/2026**
    ([comentario](https://github.com/dotnet/iot/pull/2610#issuecomment-5973268665)); GitHub lo enlaza desde la #2614.
    pgrawehr respondió (04/10/2026) que añadirá un contador de referencias. **En el *triage* del 08/10/2026 decidieron
    fusionar el #2610 así** (aprobado por raffaeler) y arreglar el fallo de los dos avisos aparte, porque es un caso
    raro. En la #2614, pgrawehr dice que la arregla el #2610. Copilot, en su revisión del #2610, señala el mismo fallo.
- [x] **Incidencia nueva, encontrada al reproducir la anterior:** en la Pi 3, `GpioController.QueryComponentInformation()`
      lanza `NotSupportedException` (con 4.2.0 y con `main`): `RaspberryPi3Driver` llama a `GetChipInfo()`, que
      `RaspberryPi3LinuxDriver` no implementa. **Informada el 03/10/2026:
      [dotnet/iot#2615](https://github.com/dotnet/iot/issues/2615).**
- [~] Preparar PR(s) a dotnet/iot. Punto de partida (03/10/2026):
  - **Hecho el 03/10/2026:** el PR de `Ina219` (#2612) y el de `DCMotor` y `ExplorerHat` (#2613). Cómo se trabaja
    con el clon del fork, las pruebas y la prueba en la Pi: en `AGENTS.md` ("Contribuir a dotnet/iot"). Y las
    incidencias #2614 (eventos de GPIO) y #2615 (`QueryComponentInformation`).
  - **Integración continua de dotnet/iot (Azure DevOps):** compila y prueba en Linux, macOS y Windows, en Debug y
    Release. `Button.Tests` (por ejemplo, `ButtonTests.If_Button_Is_Held_Down_Longer_Than_Debouncing`, que mide
    tiempos) falla casi siempre en Linux Debug: en el #2612 (dos veces), en el #2613 y en el #2605 de otro autor.
    Según raffaeler (mantenedor, 03/10/2026), pgrawehr y joperezr preparan un PR para arreglar esas pruebas. Desde
    fuera no se puede repetir una compilación: se pide en un comentario, y raffaeler la repitió. El registro de la
    consola solo muestra el resultado de algunos proyectos de pruebas; para saber cuál falla, el *binlog* (método en
    `AGENTS.md`). En el *binlog* se ve que `Ina219.Tests`, `DCMotor.Tests` y `ExplorerHat.Tests` pasan.
  - **Fork `Kash0321/iot`:** existe, pero está 374 commits por detrás y su rama es `master` (la de dotnet/iot es
    `main`). Como las ramas se llaman distinto, lo más sencillo es clonar el fork, añadir
    `upstream` (dotnet/iot), crear cada rama desde `upstream/main` y subirla al fork. Un PR por arreglo.
  - **Versión publicada:** `Iot.Device.Bindings` 4.2.0 sigue siendo la última en NuGet. Los arreglos llegarán en la
    siguiente; hasta entonces el robot sigue con `SafeExplorerHat` y con la lectura directa de registros en
    UpsDashboard.
  - **Orden propuesto:** 1) `Ina219` (cambio pequeño, con incidencia abierta y fácil de probar con la UPS);
    2) `ExplorerHat` `Dispose` (con una prueba en la Pi: liberar con los motores en marcha y las ruedas en el aire);
    3) el fallo de los eventos de GPIO, como incidencia, si se reproduce con `main`.
  - **Antes de programar:** leer `CONTRIBUTING.md` y las normas de dotnet/iot (CLA de la .NET Foundation, estilo,
    pruebas unitarias si el binding las tiene), compilar el binding en el PC y probar en la Pi con una referencia de
    proyecto o un paquete local, sin tocar los programas del robot.

### Ampliar el binding: diseño acordado (03/10/2026)
La sesión de diseño se hizo el 03/10/2026 (noche): lo comprobado, lo leído y el diseño acordado con el usuario están
abajo. Una rama y un PR por pieza, como en los PR #2612 y #2613, en el orden de "Orden de los PR".

**Lo comprobado:**
- **Código:** `C:\work\iot\src\devices\ExplorerHat` en el PC (clon del fork; cómo se trabaja con él, en
  `AGENTS.md`). Clases: `ExplorerHat`, `Motors`, `Lights`, `Led` y `DCMotorExtensions`. El README del binding dice
  "Capacitive touchpad, inputs, outputs, and 3.3v breakout not supported... Working on them". El `.csproj` incluye
  las carpetas `Gpio/`, `Lighting/` y `Motorization/`, que no existen (restos del diseño original).
- **El PR #2613 (abierto) cambia `ExplorerHat.cs`, `Motors.cs` y `Lights.cs`** y crea `ExplorerHat/tests` con
  `FakeGpioDriver`. Decidido: el trabajo en `ExplorerHat` parte de la rama `explorerhat-dispose` y su PR se abre
  cuando fusionen el #2613 (ver "Orden de los PR").
- **Lo que falta, con sus pines** (ver también la lista de arriba):

  | Pieza | Conexión | Lo que sabemos |
  |---|---|---|
  | Entradas IN1–IN4 | GPIO 23, 22, 24, 25 | Admiten 5 V (búfer en el HAT); leen 0 sin nada conectado (pull-down); sin histéresis: el LM393 da saltos en cada borde (Fase 5, lección 11) |
  | Salidas OUT1–OUT4 | GPIO 6, 12, 13, 16 | Colector abierto: el pin en alto activa la salida y la une a 0 V (lógica invertida); necesitan pull-up externa (README, "Por qué los sensores van a las entradas del HAT") |
  | Entradas analógicas 1–4 | ADS1015, I2C 0x48 | Hay binding `Ads1115`; su código y su README no mencionan el ADS1015 (12 bits, otras velocidades): comprobar si sirve |
  | Pads táctiles 1–8 | CAP1208, I2C 0x28 | No hay ningún binding CAP1xxx en dotnet/iot: haría falta uno nuevo |

- **En el robot, esos pines ya tienen dueño:** IN1–IN3 y OUT1–OUT3 son los HC-SR04 (que usan el binding `Hcsr04`
  con números de pin) e IN4 es el LM393 derecho. Las pruebas en la Pi tienen que contar con ello.
- **Eventos de GPIO en la Pi 3 con libgpiod v2:** un solo aviso por pin y sin quitarlo (#2614). El PR #2610 lo arregla,
  pero con dos avisos en un pin, quitar uno deja sin flancos al otro. Afecta a cualquier diseño de las entradas con
  eventos.

**Lo leído en la sesión de diseño:**
- **Referencia técnica del HAT y librería de Python de Pimoroni** (`pimoroni/explorer-hat`, `pimoroni/cap1xxx`):
  - Entradas: búfer SN74LVC125A (protege la Pi de los 5 V). El GPIO está detrás del búfer, así que **las resistencias
    internas de la Pi no sirven**: siempre `PinMode.Input`; si hace falta *pull-up*, va por fuera.
  - Salidas: ULN2003A (transistores Darlington). Con el pin en alto, la salida **absorbe corriente hacia masa**; para
    Pimoroni eso es "on" (el aparato va entre la alimentación y la salida).
  - **Los números del HAT no coinciden con los canales de los chips:**

    | HAT | Chip |
    |---|---|
    | Analógica 1, 2, 3, 4 | ADS1015, canales 3, 2, 1, 0 |
    | Pads 1–4 | CAP1208, entradas 5–8 (bits `0x10`–`0x80` del registro 0x03) |
    | Pads 5–8 | CAP1208, entradas 1–4 (bits `0x01`–`0x08`) |

  - Analógico: Pimoroni usa ±6,144 V, una medida cada vez (*single-shot*) a 250 por segundo y deja en 0 las tensiones
    negativas.
  - Táctil: Pimoroni crea el `Cap1208()` **sin pin de alerta** y sondea el registro cada 5 ms; la referencia técnica
    no cita ningún GPIO para la alerta. Al empezar cambia la configuración de fábrica (multitáctil, sin repetición,
    sensibilidad, muestreo).
- **Chips leídos por I2C en la Pi (solo lectura):** CAP1208 con ID de producto `0x6B`, fabricante `0x5D` (Microchip),
  revisión `0x01`, sensibilidad `0x2F` (32x, la de fábrica) y `0x44` = `0x40` (alerta activa en bajo). ADS1015 con la
  configuración de fábrica `0x8583`: por los registros no se distingue del ADS1115 (sí por la velocidad).
- **dotnet/iot:** no hay nada del ADS1015 ni de los CAP1xxx (ni código, ni incidencias, ni PR). **`Ads1115` ya lee
  bien las tensiones del ADS1015**: el ADS1015 deja sus 12 bits en la parte alta del mismo registro de 16 bits. Solo
  cambian las velocidades: el mismo código de `DataRate` es otra frecuencia (`SPS128` = 1600/s en el ADS1015) y
  `FrequencyFromDataRate` da valores falsos.
- **`Devices-conventions.md`:** métodos para los valores que cambian (`Read…`) y propiedades para los fijos; UnitsNet
  (`ElectricPotential`); el dispositivo libera su `I2cDevice`; el constructor solo pide lo imprescindible.

**Diseño acordado (03/10/2026):**
- **Abrir cada pin la primera vez que se usa, no al crear `ExplorerHat`** (como Pimoroni). El robot usa IN1–IN3 y
  OUT1–OUT3 con `Hcsr04` y otro `GpioController`: si `ExplorerHat` los abriera todos, libgpiod no dejaría volver a
  pedirlos y fallarían todos los programas con sensores. Lo mismo con los chips I2C: se crean la primera vez.
1. **Forma de la API:** colecciones como `Lights`, con nombres y números: `hat.Inputs.One`…`Four`,
   `hat.Outputs.One`…`Four`, `hat.Analog.One`…`Four` y `hat.Touch.One`…`Eight`.
   - Entradas y salidas con la propiedad `Pin` (como `Led.Pin`), para usarlas con `Hcsr04` o `GpioButton`.
   - Salidas con `On()` / `Off()` / `IsOn`, como `Led` y Pimoroni: `On()` = activa (une la salida a 0 V). Se explica
     en la documentación, sin invertir nada en el código.
   - **Entradas con `Read()` que devuelve `PinValue`** (decisión del usuario: la librería es de propósito general; si
     hace falta, se simplifica en nuestro lado, en `SafeExplorerHat` o en las lecciones).
2. **Entradas:** solo lectura simple en el primer PR, sin eventos. Para eventos ya está `GpioButton` (con
   `hat.Inputs.Four.Pin`). Los eventos, más adelante, cuando estén arreglados #2614 y el fallo de dos avisos del #2610.
3. **Analógico:** `ExplorerHat` usa `Ads1115` por dentro, con ±6,144 V, una medida cada vez, el canal correcto y un
   código de velocidad elegido por nosotros. API: `hat.Analog.One.ReadVoltage()` → `ElectricPotential`. El soporte del
   ADS1015 en `Ads1115` (velocidades y nombres correctos), en un PR aparte y opcional. **Medido en la Pi el 04/10/2026:** el
   ADS1015 del HAT va a 5 V y sus entradas admiten de 0 a 5 V.
4. **Pads táctiles:** binding nuevo `Cap1xxx` con la clase `Cap1208`, por sondeo (sin pin de alerta), en su propio PR y
   con pruebas con `I2cSimulatedDeviceBase`. Después, `hat.Touch.One`…`Eight` con `IsTouched()`. El CAP1188 y el
   CAP1166 (con LED) se podrían añadir más adelante sobre la misma base.
5. **Propiedad y `Dispose`:** el patrón del #2613. `ExplorerHat` es dueño del controlador, de los dos `I2cDevice`
   (bus 1) y de `Ads1115` y `Cap1208`. Al liberar, pone las salidas en `Off()` antes de cerrar sus pines y solo cierra
   los pines que abrió.
6. **Orden de los PR:**
   1. Binding `Cap1208` (`src/devices/Cap1xxx`). No toca `ExplorerHat`: se puede abrir sin esperar al #2613.
      **Abierto el 03/10/2026: [dotnet/iot#2616](https://github.com/dotnet/iot/pull/2616). Fusionado el 18/10/2026.**
   2. Entradas y salidas en `ExplorerHat`: se programan sobre la rama del #2613 (`explorerhat-dispose`) y el PR se
      abre cuando lo fusionen. **Hecho y probado el 04/10/2026. Abierto el 16/10/2026:
      [dotnet/iot#2625](https://github.com/dotnet/iot/pull/2625)** (rama `explorerhat-io`).
   3. Analógico en `ExplorerHat`. **Hecho y probado el 04/10/2026** (rama `explorerhat-analog-v2`, encima del #2625).
   4. Pads en `ExplorerHat`.
   5. Según lo que respondan en la #2617 (04/10/2026): motor con PWM en las dos entradas en `DCMotor` y en
      `ExplorerHat.Motors`, después del #2613.

   Cada uno con pruebas unitarias (`FakeGpioDriver`, `I2cSimulatedDeviceBase`), prueba en la Pi y el README del binding
   al día. De paso, quitar del `.csproj` de `ExplorerHat` las carpetas que no existen.
7. **Para los niños:** la lección 05 pasa a `hat.Inputs.Four.Read()` y `hat.Outputs.Four.On()` (sin `GpioController`);
   la 07 usa `hat.Touch.One.IsTouched()` y la 08, `hat.Analog.One.ReadVoltage().Volts`. `SafeExplorerHat` expone las
   cuatro colecciones nuevas y en la parada de emergencia pone a nivel bajo también OUT1–OUT4. **La 08 ya usa
   `hat.Analog.One` desde el 17/10/2026** con el ayudante `AnalogInputs` de `Common` (el fork no sirve para las
   lecciones: su tipo `ExplorerHat` choca con el del paquete 4.2.0, `CS0433`); se borra cuando el binding publicado
   tenga las entradas analógicas. **La 07 usa `hat.Touch.One.IsTouched()` desde el 17/10/2026** con el ayudante
   `TouchPads` de `Common`, que habla por I2C con el CAP1208 (`Cap1208` es un binding nuevo, #2616, y no está en el
   paquete 4.2.0); se borra cuando el binding publicado tenga los pads. El PR 4 (pads en `ExplorerHat`) tendrá
   `Touch` con la misma forma.

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
- [~] Medir el LLM en el portátil candidato: Ollama con `qwen2.5:3b` y `llama3.2:3b`, y `qwen2.5:7b` para comparar.
      Unas 20 órdenes de prueba en español: tiempo de respuesta y aciertos en la herramienta y en sus valores.
      **Hecho el 25/10/2026** (24 órdenes, ver "Medida del 25/10/2026", más abajo). **Falta repetir con la API de reserva.**
- [x] Decidir el protocolo entre el portátil y la Pi (HTTP sencillo, WebSocket o MQTT, que necesita un servidor más)
      y cómo detecta la Pi que se ha perdido la conexión. **Decidido por el usuario el 25/10/2026: HTTP sencillo.** La
      pérdida de conexión no se detecta: la cubre la duración máxima de cada acción (ver "Servicio de órdenes").
- [~] Servicio de órdenes en la Pi (C#, con `SafeExplorerHat`): lista cerrada de acciones, validación, duración
      máxima, parada al perder la conexión y parada por obstáculo. Probarlo con las ruedas en el aire y con órdenes
      escritas a mano, sin LLM. Repetir la prueba de corte de wifi de la Fase 3.
      **Escrito y probado sin mover las ruedas el 25/10/2026** (`src/ExplorerHat.Orders`, ver "Servicio de órdenes",
      más abajo). **Pruebas con motores hechas el 25/10/2026 (7 de 7 y corte de red).** **Falta:** probarlo en el suelo y el cerebro.
- [ ] Cerebro en el portátil (C#): orden escrita → LLM con las herramientas → acciones → Pi. El LLM recibe como texto
      el estado de los sensores (distancias y batería).
- [ ] Voz: Whisper local en el portátil (por ejemplo, whisper.cpp) con *push-to-talk*. Probar con ruido de fondo.
- [ ] Opcional: respuesta hablada (un altavoz con amplificador propio en el conector de 3,5 mm de la Pi, o el portátil).
- [ ] Opcional: cámara en el conector CSI de la Pi y un modelo de visión en el portátil, para que el robot "vea".
      **La cámara ya está** (confirmado el 16/10/2026; anotar el modelo). La Pi 3 B+ tiene un conector CSI de 15 pines:
      comprobar que el cable de la cámara es de ese ancho.
- [ ] Opcional, como experimento para el taller: `llama.cpp` con un modelo de 0,5B en la Pi, con los motores parados.
      Medir tokens/s, RAM y temperatura, para que los niños vean con datos por qué el cerebro va fuera. Ocupa unos
      400 MB (la microSD tiene ~2,6 GB libres).
- [ ] Lecciones: sentir, pensar y actuar; editar el *system prompt* y la lista de herramientas; capa reactiva frente a
      capa deliberativa; comparar su programa fijo con el agente. La parte compleja (servidor, JSON y validación) es
      código del monitor, no de los niños.

### Medida del 25/10/2026 (MSI Prestige 15 A12UD, RTX 3050 Ti de 4 GB, controlador 621.01, enchufado)
Herramienta: [`tools/medir-llm/`](tools/medir-llm/README.md). 24 órdenes (simples, con números, en lenguaje de niño, de varios
pasos, fuera de rango, absurdas o peligrosas, y una urgente) con el resultado esperado escrito **antes** de medir; cada
orden 3 veces, temperatura 0, semilla 1, `num_ctx` 2048, Ollama 0.40.11. Esquema de cinco herramientas:
`avanzar`, `retroceder`, `girar`, `luz` y `parar`. Resultados completos en `tools/medir-llm/resultados/`.
**Con temperatura 0 las 3 repeticiones dan casi siempre la misma respuesta** (solo difiere la orden 23 de Llama): sirven
para el tiempo, no para medir la variación. Los aciertos se cuentan **por orden** (una orden cuenta si acierta en las 3
repeticiones): la muestra son 24 órdenes, escritas por la misma sesión que escribió los resultados esperados.

| Modelo (GGUF `Q4_K_M`) | Herramienta OK | Valores OK | Válidas | Tiempo (mediana / máximo) | Carga en frío | Tarjeta gráfica |
| --- | --- | --- | --- | --- | --- | --- |
| `Qwen2.5-3B-Instruct` (Qwen) | 21/24 | 19/24 | 20/24 | 0,70 s / 4,1 s | 3,0 s | 100 % |
| `Llama-3.2-3B-Instruct` (bartowski) | 15/24 | 6/24 | 9/24 | 0,53 s / 5,6 s | 14,5 s | 100 % |
| `Qwen2.5-7B-Instruct` (bartowski) | **24/24** | 22/24 | 22/24 | 3,0 s / 25 s | 4,7 s | 45 % |
| `Qwen3-4B-Instruct-2507` (unsloth, con `think: false`) | 21/24 | 20/24 | 23/24 | 1,1 s / 7,6 s | 3,9 s | 72 % |

El Qwen3 4B se midió en una segunda tanda, el mismo día. Con el GGUF de bartowski, Ollama devolvía las llamadas en el campo
`thinking` o como texto (solo se interpretaba una de dos): el mismo tipo de fallo de plantilla que en Llama. Con el de
unsloth y `think: false` (opción `--sin-pensar` de `medir.py`) las llamadas llegan bien.

* **Los modelos no vienen de la biblioteca de Ollama**, sino de Hugging Face (la red bloquea el servidor de archivos de
  Ollama). Usan la plantilla de su GGUF: **el resultado de Llama no vale como medida del modelo oficial**. Sus fallos son
  de formato (números como texto, `"2"` en vez de `2`; la llamada escrita como texto plano), lo que apunta a la plantilla.
  Repetir con `llama3.2:3b` de la biblioteca de Ollama si se quiere comparar de verdad.
* **Fuera de rango (órdenes 19 y 20): fallan los tres modelos, siempre** (son los 2 fallos de valores de `qwen2.5:7b`).
  Pasan el valor tal cual (`segundos` = 300 y 10), aunque el esquema y el mensaje de sistema dicen "máximo 3 s". **El
  recorte de la Pi no es opcional**: no se puede confiar en que el modelo respete los límites.
* **`qwen2.5:3b` falla en cosas que importan:** ante `Para.` llama a `avanzar` (3 de 3), inventa un color
  (`luz("morada")`) y en el cuadrado de 8 pasos se deja el último (7 llamadas). Con `¡¡PARA YA!!` acierta. **Propuesta
  (sin decidir):** la parada no depende del LLM. Es coherente con la decisión de la Fase 7 "seguridad, siempre en la Pi y
  nunca en el LLM": las palabras "para", "stop" o "alto" (y un botón o un pad táctil) paran el robot sin pasar por el
  modelo, y la Pi valida además nombres de herramienta, colores y números.
* **`qwen2.5:7b` acierta la herramienta en las 24 órdenes**, también el cuadrado, pero tarda 3 s de mediana (25 s el
  cuadrado, con 8 llamadas) porque solo el 45 % del modelo cabe en la tarjeta gráfica (12 tokens/s frente a 53 del de 3B).
  **La promesa de menos de 1 s solo la cumple el de 3B.**
* **`Qwen3-4B-Instruct-2507` queda en medio:** 1,1 s de mediana (el cuadrado, 7,6 s) y la mejor tasa de llamadas válidas
  (23/24). Sus fallos son otros: no conoce "tira palante" (contesta "reformula tu solicitud"); **ante `¡¡PARA YA!!` escribe
  el texto `parar` en vez de llamar a la herramienta** (la parada no se ejecutaría); y en "5 minutos" se **niega** a girar en
  vez de recortar a 3 s (el puntuador lo cuenta como fallo, pero es un comportamiento seguro). En "10 segundos" pasa el
  valor tal cual. No cabe entero en la tarjeta (72 %) con la pantalla en uso.
* **Los dos modelos rápidos fallan en una orden de parada** (`qwen2.5:3b` con `Para.` y `qwen3:4b` con `¡¡PARA YA!!`);
  solo el 7B las acierta, con 2 órdenes de parada de muestra. Refuerza la propuesta de dejar la parada fuera del modelo.
* `qwen2.5:3b` se midió dos veces: la primera con el modelo ya en memoria (carga de 0 s) y la segunda en frío (3,0 s). Los
  aciertos fueron iguales, como se espera con temperatura 0.
* **Límite de la medida (tiempos):** el mensaje de sistema y las herramientas son iguales en todas las peticiones
  (~650 tokens de entrada; `prompt_eval_count` marca 652 a 699 en todas las ejecuciones) y `medir.py` no guarda
  `prompt_eval_duration`, así que no sabemos si Ollama reutilizó la caché de ese prefijo. Para el cerebro de la Fase 7: el
  estado de los sensores va **después** del prefijo fijo (en el mensaje del usuario), no en el mensaje de sistema.
* **Sin probar:** la API de reserva; los modelos de la biblioteca de Ollama (otra red); otros modelos de 4B (por ejemplo,
  `phi4-mini`, cuya plantilla puede no admitir herramientas); mejorar el mensaje de sistema (por ejemplo, con
  "`tira palante` = avanzar" y "si piden más de 3 segundos, usa 3"); modelos que cuenten mejor los pasos. **Decisión pendiente con el usuario:** elegir entre rapidez
  (3B, con la parada fuera del modelo) y fiabilidad (7B, con más espera).

### Servicio de órdenes (25/10/2026)
Código en [`src/ExplorerHat.Orders`](src/ExplorerHat.Orders/README.md) y pruebas en
[`tools/probar-ordenes`](tools/probar-ordenes/probar.py). Decisiones de diseño (con la consulta al asesor):
* **HTTP sencillo** (Kestrel, que ya está en la Pi: `Microsoft.AspNetCore.App` 10.0.12). `POST /accion` espera a que
  termine la acción y responde por qué terminó (`tiempo`, `parada`, `obstaculo` o `sensores`) y cuánto duró.
* **Sin detección de desconexión ni cola.** Un corte de wifi no cierra la conexión TCP (Kestrel tarda ~30 s en
  verlo, o no lo ve), así que no se confía en eso. La protección es la duración máxima: 3 s en `avanzar` y `girar`, **1 s
  en `retroceder`** (va a ciegas; decisión del usuario). El portátil manda una acción, espera la respuesta y manda la
  siguiente; si llega otra con una en curso, 409. Con un corte, el robot se para en 3 s como mucho.
* **Seguridad en la Pi:** lista cerrada; tipos estrictos (un número escrito como texto se rechaza con 400, como hacía
  Llama); recorte de segundos y velocidad, que se avisa en `ajustes`; obstáculo a 30 cm con los tres HC-SR04 (filtro de
  la lección 06, parada si algún sensor mide menos de 30; sin datos de los sensores no se avanza, 503); clave en la
  cabecera `X-Clave` (archivo `~/.explorerhat-clave` con permisos 600, creado al azar la primera vez).
* **Un solo hilo escribe en los motores**, con un `lock`; el movimiento corre en su propio hilo y mira cada 20 ms la
  hora, la parada y el obstáculo. `/parar` cancela y pone los motores a 0 bajo el mismo `lock`.
* **Tres modos de arranque, y hay que elegir uno:** `--simulado` (sin pines), `--velocidad-cero` (pines reales; las
  ruedas no giran nunca, pero `/estado` dice lo que se mandó) y `--real`.
* **El nombre `ExplorerHat.Orders`** se añadió a `tools/parar-robot.sh`, `tools/vigilar-pines.sh` y la tarea de VS Code.

Resultados (sin mover las ruedas):
* **29 casos de caja negra** (`probar.py`, con el resultado esperado escrito antes que el servicio): 29 de 29 en `--simulado`
  en el PC; 23 de 23 en la Pi con `--velocidad-cero` (los 6 del simulador se omiten) y los sensores reales (275, 115 y
  50 cm). **Se metieron cinco fallos a propósito** (retroceder de 3 s, "sin datos" tomado como libre, `Stop()` sin
  cancelar, `<=` en lugar de `<` y número como texto aceptado) y las pruebas detectaron los cinco.
* **Señales con Kestrel** (SIGTERM, SIGHUP, SIGINT y SIGQUIT, a mitad de `avanzar 3 s`, en `--velocidad-cero`): el servicio
  termina en 0,2 s, imprime "Parada de emergencia", y los 8 pines quedan en `lo`. Los manejadores de Kestrel y los de
  `SafeExplorerHat` conviven bien. `bash tools/parar-robot.sh` desde el PC también lo para. Memoria de la Pi con el
  servicio en marcha: 191 MB usados.
* **Códigos de salida de las señales** (`timeout --preserve-status`, a mitad de `avanzar 3 s`): SIGTERM 143, SIGINT 130 y
  SIGHUP 129, los mismos que en las lecciones. Los scripts nuevos (`parar-robot.sh`, `vigilar-pines.sh`) se copiaron a
  `~/apps/tools-orders/` de la Pi sin subir la rama: con ellos, la parada encuentra el servicio y `vigilar-pines` cuenta
  `procs=1`. (Tras fusionar el #64, la Pi usa los de `main`, que ya conocen `Orders`, y la copia se borró.)
* **Capa de obstáculos con los sensores reales, ruedas sin girar** (`tools/probar-obstaculo.sh`, con la mano del usuario, 25/10/2026):
  4 de 4. Control sin nada a menos de 30 cm: `tiempo` (3,02 s). Mano a ~20 cm del centro, de la izquierda y de la derecha:
  `obstaculo` en los tres, con 0 s (el obstáculo ya estaba al empezar). **Sin probar con sensores reales:** un obstáculo
  que aparece a mitad del movimiento (con el simulador sí, caso 25).
* **Dos lecciones:** en Windows, `localhost` prueba primero IPv6 y cada petición tarda 2 s de más (falsea las pruebas
  de tiempo): usar `127.0.0.1`. Y el filtro de los sensores guarda la menor de las dos últimas lecturas, así que las
  pruebas esperan 0,3 s entre casos.
* **Límite conocido para el cerebro:** una orden que el portátil dio por perdida puede llegar tarde, cuando vuelve la red,
  y mover el robot hasta 3 s. Regla: tras un tiempo de espera agotado, mirar `/estado` antes de repetir la orden.
* **Límites conocidos:** un sensor desenchufado no da eco y se lee como 400 cm ("no hay nada"), como en la lección 09;
  al retroceder y al girar no hay protección de obstáculo.
* **Primera prueba con motores (`--real`, ruedas en el aire, confirmación del usuario, 25/10/2026, 19:36 a 19:41).** El
  usuario arrancó el servicio en su terminal (sin `| tee`) y yo mandé una orden cada vez con `curl`, con cuenta atrás de 8 s;
  `vigilar-pines.sh` registró los pines. El usuario vio cada rueda. **Los 7 pasos salieron como se esperaba:**
  1. `avanzar 0.5`: las dos ruedas hacia delante, `tiempo`, 0,51 s.
  2. `girar derecha 0.5` (dos veces): la rueda derecha hacia atrás y la izquierda hacia delante, 0,5 s.
  3. `retroceder 0.5`: las dos hacia atrás.
  4. `retroceder 10`: paró a 1 s, con `ajustes: ["segundos"]`.
  5. `avanzar 3` y `/parar` a 1 s: `/parar` respondió en 0,18 s, la acción terminó por `parada` a 1,08 s y las ruedas
     pararon al momento.
  6. `avanzar 3` con la mano delante de un sensor a mitad: `obstaculo` a 0,7 s, ruedas paradas.
  7. `avanzar 3` y **Ctrl+C del usuario** a 0,8 s: las ruedas pararon, el servicio imprimió "Parada de emergencia (SIGINT):
     motores parados" y terminó. En el registro de pines, los pines de los motores estaban en `hi` (PWM) hasta las 19:40:50.1
     y todos en `lo` con `procs=0` a las 19:40:50.3 (muestras cada ~0,27 s). Al acabar: 8 pines en `lo`,
     `get_throttled` = `0x0` y ningún programa en marcha.
* **Prueba de corte de red (`--real`, ruedas en el aire, 25/10/2026, 19:49).** El usuario arrancó el servicio en su terminal; yo
  mandé `avanzar 3` desde el PC y el usuario **desenchufó el cable de red del portátil** al primer segundo con las ruedas
  girando (la Pi va por wifi y no se tocó). Mi `curl` no recibió la respuesta (código 28, tiempo agotado a los 12 s), aunque
  el servicio la había enviado: el enlace estaba cortado. **Resultado: las ruedas pararon solas a los 3 s.** Servicio: orden
  a las 19:49:24, `terminada` por `tiempo` con 3,01 s a las 19:49:27. Registro de pines (muestras cada ~0,27 s): primera muestra
  con los motores en `hi` a las 19:49:24.7 y la última a las 19:49:27.4; todos en `lo` desde las 19:49:27.7, con el proceso
  vivo (`procs=1`). El usuario vio las ruedas parar a los ~3 s. Criterio cumplido: **pines en `lo` a los 3 s de la orden, sin
  ayuda del portátil.** La sesión de terminal del usuario no se cortó, así que el SIGHUP por pérdida de SSH no intervino.
  Al acabar: 8 pines en `lo`, `get_throttled` = `0x0`.
* **El servicio de órdenes queda probado** (sin motores en el simulador y la Pi; con motores en el aire: sentido de las
  ruedas, recortes, `/parar`, obstáculo, Ctrl+C y corte de red). **Falta:** probarlo en el suelo (no hecho) y el cerebro en
  el portátil (C#), que reutiliza `tools/medir-llm/herramientas.json` como esquema.
