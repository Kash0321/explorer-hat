# Plan de trabajo: puesta al día y ampliación

Objetivo: recuperar el repositorio para volver a hacer talleres con niños (programación, electrónica
y robótica) con Raspberry Pi 3 B+, Pimoroni Explorer HAT Pro y .NET 10, y revisar/ampliar el binding
`Iot.Device.ExplorerHat` de dotnet/iot.

Leyenda: `[ ]` pendiente · `[x]` hecho · `[~]` en curso

## Estado actual (10/10/2026)
- **Sesión del 10/10/2026:**
  - **dotnet/iot, sin cambios:** nada nuevo desde nuestras respuestas del 09/10/2026. El #2613 sigue en "Review
    required" (pgrawehr no ha contestado a la pregunta de las variantes con `PwmChannel`); el #2616 y el #2610, aprobados
    y sin fusionar. `upstream/main`, sin commits nuevos.
  - **Lección 08: sensores decididos.** Un **potenciómetro** lineal de 10 kΩ (B10K) y una **fotorresistencia** (LDR,
    modelo sin confirmar) con una resistencia fija de 10 kΩ. **Las fotorresistencias ya las hay** (una bolsa de 20); los
    potenciómetros (3 ALLECIN WH148 B10K) están pedidos y llegan el 11/10/2026. Motivos y descartes en la Fase 5
    (lección 08).
  - **Mientras llegan, no se empieza la lección 10:** antes, decidir con el usuario otra tarea.
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
  - **Rueda derecha parada:** `Lesson11.Odometry` contaba 0 pulsos. El sensor estaba bien: el cable azul del motor
    derecho se había soltado de la borna MOTOR 1. Diagnóstico, mensajes de las lecciones 11 y README. Fase 5.
  - **Lección 08 (sensores analógicos), decidida como siguiente tarea:** con `ProjectReference` al binding de la rama
    `explorerhat-analog` del fork (`C:\work\iot`). Faltaba saber qué sensor analógico se usa: decidido el 10/10/2026
    (potenciómetro y fotorresistencia, por comprar).
- **Sesión del 04/10/2026, mañana** (PR #33 a #36 de este repositorio; detalles en las fases):
  - El usuario desmontó y volvió a montar todo el cableado. Revisado con `tools/probar-cableado.sh` (nuevo): todo
    bien salvo los motores, cambiados de borna y con el derecho al revés. Colores de los cables, en el README.
  - **Marcha atrás más lenta** con el `DCMotor` del binding (a 0,4, al ~36 % de la velocidad hacia delante): marcha
    atrás, el DRV8833 frena en la parte apagada del PWM. **Nuevo `HatMotor`** en `ExplorerHat.Common` (PWM en los dos
    pines, como Pimoroni): los dos sentidos al 93–102 %. Informado en dotnet/iot (#2617). Fase 6.
  - **Giros recalibrados en el suelo:** lección 04, `turnTime` 200 ms; `Lesson11.Square`, `brakePulses` 4. La 09 y
    ObstacleAvoidance, sin cambios (límite de obstáculo en 30 cm, decisión del usuario). Fase 5.
  - **Alimentación:** la Pi va solo por los contactos de la UPS, **sin cable micro-USB** (con y sin cable, igual).
    Pin de 5 V: 5,30 V en reposo y 5,22 V como mínimo con los motores (0,6 V de margen). Al compilar en la Pi, la CPU
    llega a 60 °C y baja su frecuencia (`0x80000`). Fase 2.
  - **ADS1015 del HAT a 5 V:** entradas analógicas de 0 a 5 V, sin divisor; `Ads1115` lo lee bien. Fase 6.
  - **Binding `ExplorerHat` ampliado** en dos ramas del fork, probadas en la Pi: entradas y salidas digitales
    (`explorerhat-inputs-outputs`) y entradas analógicas (`explorerhat-analog`). Esperan al #2613. Fase 6.
  - Todos los programas de `~/apps` están desplegados desde `main` con `HatMotor`.
- **Fase 5, lecciones hechas y probadas** (`lessons/`): 01 Luces, 02 Semáforo, 03 Motores, 04 Cuadrado, 05 Botones,
  06 Distancia, 09 Robot autónomo, 11 Contar vueltas (odometría, ir recto y el cuadrado con pulsos) y 12 La pantalla.
  Todas con las ruedas en el aire y, las que se mueven, también en el suelo.
- **Faltan de la Fase 5:** 07 Pads táctiles y 08 Sensores analógicos (necesitan una versión de `Iot.Device.Bindings`
  con la Fase 6, o el binding compilado desde el fork) y 10 Siguelíneas (dos TCRT5000, por confirmar).
- **Montaje actual:** todo conectado a la vez. Tres HC-SR04 (IN1–IN3), LM393 derecho en IN4 (a 5 V), LM393 izquierdo
  a **3,3 V** en GPIO 18 (fila lateral "3.3V ONLY") y pantalla LCD 2004 en I2C 0x27 (a 3,3 V). El LED de la lección 05
  no está montado. El pulsador de la lección 05 necesita IN4: idea, moverlo al GPIO 8 (CS) con el pull-down de la Pi.
- **Cerradas:** fases 0 a 4 (solo queda en la Fase 3 la guía de compilación ligera en la Pi, poco urgente).
- **Fase 7 (robot con IA):** analizada y decidida; empieza cuando termine la Fase 5.
- **Fase 6, en dotnet/iot (09/10/2026):**

  | Qué | Estado | Pendiente |
  |---|---|---|
  | [PR #2612](https://github.com/dotnet/iot/pull/2612): `Ina219` con signo ("Fixes #1659") | **Fusionado** el 08/10/2026 | Nada. Llegará en la próxima versión de `Iot.Device.Bindings` |
  | [PR #2613](https://github.com/dotnet/iot/pull/2613): `Dispose` de `DCMotor` y `ExplorerHat` | Revisión de pgrawehr y de Copilot respondida el 09/10/2026 (`15efc1ae`); 10/10 comprobaciones; "Review required" (el commit nuevo anuló la aprobación de raffaeler) | Que lo aprueben otra vez y lo fusionen. De él dependen las dos ramas de abajo |
  | [PR #2616](https://github.com/dotnet/iot/pull/2616): binding nuevo `Cap1208` (pads táctiles) | Aprobado por raffaeler, sin comentarios | Que lo fusionen |
  | [Incidencia #2614](https://github.com/dotnet/iot/issues/2614): eventos de GPIO con libgpiod v2 | Abierta (`untriaged`) | pgrawehr: la arregla el #2610 |
  | [Incidencia #2615](https://github.com/dotnet/iot/issues/2615): `QueryComponentInformation` en la Pi 3 | Abierta (`untriaged`), sin respuesta | Solo incidencia |
  | [Incidencia #2617](https://github.com/dotnet/iot/issues/2617): marcha atrás de `DCMotor2PinNoEnable`; propuesta de motor con PWM en las dos entradas | Abierta (`untriaged`), sin respuesta | Respuesta a sus tres preguntas (forma de la API, variante que frena, si `ExplorerHat` la usa) |
  | Rama `explorerhat-inputs-outputs` del fork (`4ee7df07`): entradas y salidas en `ExplorerHat` | Lista y probada en la Pi, sin PR | Abrir el PR cuando fusionen el #2613 |
  | Rama `explorerhat-analog` del fork (`cc557fe9`): entradas analógicas en `ExplorerHat` | Lista y probada en la Pi, sin PR | Abrir el PR cuando fusionen el de entradas y salidas |
  | [PR #2610](https://github.com/dotnet/iot/pull/2610) de pgrawehr: arreglos de libgpiod v2 | Aprobado por raffaeler (no es nuestro) | Lo fusionan así; el fallo de los dos avisos en un pin, aparte (pgrawehr quiere contar las referencias) |

  El #2608 (pruebas de `Button`) está fusionado: los PR ya pasan todas las comprobaciones. `upstream/main` (09/10/2026)
  tiene 5 commits nuevos desde `95384e77`; ninguno toca `DCMotor`, `ExplorerHat`, `Cap1xxx` ni `Ads1115`. Si los
  mantenedores comentan, se responde en el PR y los cambios van en commits nuevos (sin *force push*). Para mirar el
  estado: `gh pr checks <PR> -R dotnet/iot`, los comentarios con `gh api repos/dotnet/iot/issues/<n>/comments` y, en
  un PR, también las revisiones (`.../pulls/<n>/reviews`) y los comentarios en el código (`.../pulls/<n>/comments`).
- **Al retomar (siguiente sesión):**
  1. Mirar comentarios y revisiones en los PR #2613 y #2616, en las incidencias #2614, #2615 y #2617 y en el
     PR #2610. Si hay comentarios, contárselos al usuario antes de responder. En el #2613, mirar si pgrawehr contesta a
     la pregunta de nuestra respuesta: si también deben lanzar la excepción las variantes de `DCMotor.Create` que
     reciben un `PwmChannel`.
  2. Cuando fusionen el #2613: llevar la rama `explorerhat-inputs-outputs` encima de `upstream/main`
     (`git rebase --onto upstream/main explorerhat-dispose explorerhat-inputs-outputs`, porque su base es la rama del
     #2613), volver a pasar las pruebas y abrir el PR de entradas y salidas. *Force push* solo antes de abrir el PR.
  3. Las ramas van apiladas: `explorerhat-dispose` (#2613) → `explorerhat-inputs-outputs` → `explorerhat-analog`.
     Cuando fusionen el PR de entradas y salidas, llevar la del analógico encima de `upstream/main` igual (`git rebase
     --onto upstream/main explorerhat-inputs-outputs explorerhat-analog`) y abrir su PR. Si piden cambios en un PR de
     abajo, rehacer encima las ramas de arriba, como el 09/10/2026: guardar el commit anterior de la rama de en medio
     (`OLD=$(git rev-parse explorerhat-inputs-outputs)`), `git rebase explorerhat-dispose explorerhat-inputs-outputs`,
     `git rebase --onto explorerhat-inputs-outputs $OLD explorerhat-analog`, pasar las pruebas de `DCMotor` y de
     `ExplorerHat` en cada rama y subirlas con `git push --force-with-lease` (solo las ramas sin PR).
  4. Siguiente pieza del binding: los pads táctiles en `ExplorerHat` (PR 4), cuando fusionen el `Cap1208` (#2616).
     **Recomendación del 04/10/2026:** no alargar la cadena de ramas sin revisar; mientras tanto, trabajar en otra cosa.
     **Decidido (04/10/2026, tarde): la lección 08** con `ProjectReference` al fork (ver arriba), cuando lleguen el
     potenciómetro y la fotorresistencia (10/10/2026). Mientras tanto, otra tarea por decidir con el usuario; **la
     lección 10 no** (decisión del usuario, 10/10/2026). Después, la lección 10 (siguelíneas, si están los dos TCRT5000)
     o la Fase 7 (medir el LLM en el portátil).
  5. Ideas opcionales: `Brake()` en `HatMotor` (el DRV8833 frena con los dos pines en alto: paradas más cortas),
     medir la corriente de cada motor con el INA219 (Pimoroni dice 200 mA por canal), condensadores de 100 nF en los
     motores, un disipador o ventilador para la Pi y repetir en el suelo la medida del pin de 5 V.
- **Sin prisa:** la guía de compilación ligera en la Pi (Fase 3); con un multímetro, medir si la pull-up del Trig de
  los HC-SR04P es una resistencia de la placa o la interna del chip (método en el README); repetir con carga completa
  la batería Xiaomi (Fase 2).
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
  - [x] **Lecciones en las tareas y en la depuración (04/10/2026).** Las lecciones solo estaban en la tarea *Ejecutar una
        lección en la Pi*, y para depurar solo estaba BasicSample. Ahora:
    - *Desplegar en la Pi* y *Ejecutar en la Pi* tienen una sola lista con las lecciones y los programas, por su carpeta
      (`lessons/Lesson12.Screen`, `src/ExplorerHat.BasicSample`). En la Pi, el nombre de la carpeta lo da `$(basename ...)`:
      lo calcula el shell de la Pi (en macOS y Linux, antes el del PC, con el mismo resultado). Quitadas *Ejecutar una
      lección en la Pi* y *Desplegar BasicSample*.
    - **Depurar en la Pi el programa abierto** (sustituye a *Depurar BasicSample en la Pi*): despliega y depura el proyecto
      del archivo abierto en el editor (`${relativeFileDirname}` y `${fileDirnameBasename}`). Así la tarea previa y el
      depurador usan el mismo programa sin preguntarlo dos veces (las variables `${input:...}` de `launch.json` y de
      `tasks.json` se preguntan por separado).
    - La consola de depuración no tiene teclado: los programas que leen teclas (ObstacleAvoidance, SonarDashboard y las
      lecciones 04, 11 Odometry y 11 Square) se depuran con *Ejecutar en la Pi* y *Adjuntar a un programa en la Pi*.
    - Probado desde el PC (comandos de las tareas en `cmd.exe`, con la lección 12): los dos despliegues borran la carpeta
      anterior y copian la nueva; *Ejecutar en la Pi*, código 0. El usuario probó F5 con la lección 12 en VS Code: se
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
- [ ] 07 Pads táctiles: control remoto del robot (requiere Fase 6).
- [ ] 08 Sensores analógicos: luz o potenciómetro (requiere Fase 6).
  - **Sensores decididos (10/10/2026):**
    - **Potenciómetro** lineal de 10 kΩ (B10K). Pedidos 3 **ALLECIN WH148 B10K** (película de carbono, con mando y
      cable con conector XH2.54 de 3 pines), llegan el 11/10/2026. Al llegar: mirar qué hay en el otro extremo del
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
      quitarlo. Informado en dotnet/iot#2614 (Fase 6). Con la excepción, `SafeExplorerHat` paró el motor (`Error inesperado: motores parados`).
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
        visto, no. `Lesson11.Straight` y `Lesson11.Square` leen ahora la rueda izquierda en GPIO 18. Probado `Lesson11.Straight` en el aire:
        104 y 102 pulsos en 1 m, código 0 y pines a nivel bajo.
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
  - [x] Girar ángulos exactos con los pulsos y el cuadrado de la lección 04 con pulsos: `Lesson11.Square`
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
    - **Avería del 04/10/2026 (tarde): `Lesson11.Odometry` contaba 0 pulsos** en las dos partes (también con F5), sin
      ningún error. Diagnóstico:
      - Grabación de los pines cada ~11 ms durante la lección (script en `/tmp`, como `tools/vigilar-pines.sh` pero con
        las entradas; en `pinctrl get`, el nivel de una entrada es el campo anterior a `//`): PWM en los pines 19 y 21,
        443 cambios en el GPIO 18 (sensor izquierdo) y **0 cambios en el GPIO 25** (siempre `lo`).
      - Sin programa, girando a mano la rueda derecha: 48 y 78 cambios en el GPIO 25. Con el motor derecho a toda
        velocidad (`pinctrl set 19 op dh`): 0 cambios. **La rueda derecha no giraba**: el usuario creía que giraban
        las dos.
      - Causa: **el cable azul del motor derecho, suelto en la borna MOTOR 1.** Apretado, la lección cuenta bien en el
        aire: 92 pulsos en 2 s (47 cm/s, con `HatMotor`) y parada a los 49 pulsos con 5 más de inercia.
      - Cambios: el mensaje `Time is up!` de `Lesson11.Odometry` y `Lesson11.Straight` pregunta también si giran las
        ruedas, y el README de la lección tiene la sección "Si no cuenta ningún pulso".
      - Método para otra vez: si un sensor no cuenta, girar la rueda a mano (descarta el sensor) y mirar si la rueda gira
        con el motor (los pines solo dicen lo que manda la Pi).
    - La UPS se quedó sin batería durante las pruebas (la Pi se reinició). El programa desplegado justo antes quedó
      con `runtimeconfig.json` vacío (el corte llegó antes de escribir la caché en la microSD): las tareas de
      despliegue de VS Code ejecutan ahora `sync` después de copiar. `git fsck` en la Pi, sin daños. Distancia entre las ruedas: **13 cm** (medida por el usuario): una vuelta sobre sí mismo son 40,8 cm de
        cada rueda y 90° unos 10 pulsos.
- [x] 12 Pantalla: LCD de 20×4 caracteres (2004A) con adaptador I2C (PCF8574). Ideas: escribir texto y
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
  - **Lección `Lesson12.Screen` (03/10/2026, rama `fase-5-leccion-12`):** clase `Screen` en `ExplorerHat.Common`
    (`Clear()` y `Write(línea, texto)`, que corta o rellena a 20 caracteres). Tres partes, sin motores: texto con el
    nombre del niño y una línea demasiado larga, contar del 1 al 10 con su doble y un panel de 20 s con la distancia
    del sensor central (filtro de la 06) y una barra de `#` (uno por cada 10 cm) y "Too near!" a menos de 30 cm.
    Probada en la Pi: código 0; el usuario vio la línea cortada en 20 caracteres, la cuenta, la barra siguiendo a
    la mano con fluidez, "Too near!" y la despedida.

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
- 20 fotorresistencias (LDR, modelo sin confirmar; encontradas el 10/10/2026): lección 08.
- **Pedidos (10/10/2026), para la lección 08:** 3 potenciómetros ALLECIN WH148 B10K (10 kΩ lineales, con mando y
  cable con conector XH2.54 de 3 pines). Llegan el 11/10/2026.
- IN4 (GPIO 25) con nada conectado lee 0 en 50 de 50 lecturas (pull de la Pi desactivado): la entrada del HAT
  parece tener pull-down o un búfer que la mantiene baja. Confirmar con el pulsador.

## Fase 6: Binding `Iot.Device.ExplorerHat` en dotnet/iot
Estado: el binding sigue en el repositorio (activo, último cambio en el binding en julio de 2026), pero
solo cubre motores y las 4 luces. Falta:
- [~] 4 entradas digitales (GPIO 23, 22, 24, 25, tolerantes a 5 V) y 4 salidas de colector abierto (GPIO 6, 12, 13,
      16). **Hechas el 04/10/2026** en la rama `explorerhat-inputs-outputs` del fork (sobre `explorerhat-dispose`, la
      del #2613). Pendiente: abrir el PR cuando fusionen el #2613.
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
- [~] 4 entradas analógicas (ADS1015, I2C 0x48). **Hechas el 04/10/2026** en la rama `explorerhat-analog` del fork
      (encima de `explorerhat-inputs-outputs`). Pendiente: abrir el PR cuando fusionen el de entradas y salidas.
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
      rama `cap1xxx-binding` del fork (`src/devices/Cap1xxx`). Pendiente: la revisión y, después, los pads en
      `ExplorerHat` (PR 4 del orden acordado).
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
      `explorerhat-dispose` del fork. Aprobado por raffaeler el 08/10/2026; pendiente de que lo fusionen. Al revisar el
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
      **Abierto el 03/10/2026: [dotnet/iot#2616](https://github.com/dotnet/iot/pull/2616).**
   2. Entradas y salidas en `ExplorerHat`: se programan sobre la rama del #2613 (`explorerhat-dispose`) y el PR se
      abre cuando lo fusionen. **Hecho y probado el 04/10/2026** (rama `explorerhat-inputs-outputs`).
   3. Analógico en `ExplorerHat`. **Hecho y probado el 04/10/2026** (rama `explorerhat-analog`).
   4. Pads en `ExplorerHat`.
   5. Según lo que respondan en la #2617 (04/10/2026): motor con PWM en las dos entradas en `DCMotor` y en
      `ExplorerHat.Motors`, después del #2613.

   Cada uno con pruebas unitarias (`FakeGpioDriver`, `I2cSimulatedDeviceBase`), prueba en la Pi y el README del binding
   al día. De paso, quitar del `.csproj` de `ExplorerHat` las carpetas que no existen.
7. **Para los niños:** la lección 05 pasa a `hat.Inputs.Four.Read()` y `hat.Outputs.Four.On()` (sin `GpioController`);
   la 07 usa `hat.Touch.One.IsTouched()` y la 08, `hat.Analog.One.ReadVoltage().Volts`. `SafeExplorerHat` expone las
   cuatro colecciones nuevas y en la parada de emergencia pone a nivel bajo también OUT1–OUT4. Hasta que haya versión en
   NuGet, las lecciones 07 y 08 necesitan el binding compilado desde el fork.

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
