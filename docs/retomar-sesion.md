# Retomar el trabajo con un asistente de IA

Prompt para empezar una sesión nueva de Claude Code (u otro asistente) en cualquier equipo, de forma que recupere el contexto del proyecto que quedó escrito en el repositorio.

**Antes de pegarlo:**

1. Abre el asistente en la carpeta del repositorio, estando en `main` y al día (`git pull`).
2. Comprueba la configuración del experimento de modelos (`.claude/settings.json`): `/model` debe decir Sonnet 5.5 y `/advisor`, Opus. El subagente `buscador` aparece entre los agentes disponibles.
3. El experimento de modelos está decidido (17/10/2026, se mantiene como está): ya no hace falta pasar `/usage` al empezar ni al cerrar, salvo que quieras volver a revisarlo.
4. Si la sesión va a mover el robot: la Raspberry Pi encendida y con wifi, la UPS cargada, el robot **sobre un bote o un libro, con las ruedas en el aire**, y una terminal SSH abierta en la Pi.

Esa sesión no tendrá las conversaciones anteriores, sino lo que quedó escrito: `AGENTS.md`, `PLAN.md`, el `README.md`, `tools/` y el historial de git y de los PR. Si echas en falta algo, añádelo a `AGENTS.md` o a `PLAN.md` para que no vuelva a perderse. Al terminar cada sesión, pide al asistente que actualice la sección "Estado actual" de `PLAN.md`.

```text
Retomamos el proyecto explorer-hat. Las sesiones anteriores de Claude Code (en la propia Raspberry Pi
o en mi portátil con Windows) dejaron escrito en el repositorio todo lo necesario para continuar.
Antes de hacer nada más:

1. Lee y ten presentes durante toda la sesión:
   - AGENTS.md: contexto del proyecto, entorno, pines del HAT, normas de seguridad del robot y cómo
     trabajar desde el PC por SSH. Sus reglas son obligatorias.
   - PLAN.md: fases del proyecto, lo hecho, lo pendiente y los resultados de todas las pruebas.
     Empieza por su sección "Estado actual".
   - README.md y los scripts de tools/ (cada uno explica su uso al principio).
   - El historial reciente: git log --oneline -30 y los PR fusionados (gh pr list --state merged),
     sobre todo los #4 a #64, cuyas descripciones resumen cada fase y cada lección.
   - Lo que tenemos abierto en dotnet/iot (Fase 6): los PR e incidencias de la tabla del "Estado
     actual" de PLAN.md (el 25/10/2026: PR #2625, que espera la nueva revisión de pgrawehr tras
     nuestra respuesta, e incidencias #2614, #2615 y #2617; los PR #2610 y #2616 están fusionados), con sus comprobaciones, comentarios y revisiones
     nuevos (gh pr checks <n> -R dotnet/iot, gh api repos/dotnet/iot/issues/<n>/comments y, en los
     PR, gh api repos/dotnet/iot/pulls/<n>/reviews y .../pulls/<n>/comments). Si hay
     comentarios, dímelo antes de responder.

2. Comprueba que llegas a la Raspberry Pi sin que te pida nada:
   ssh -o BatchMode=yes harlequin true
   En Windows, desde Git Bash, usa el ssh de Windows (/c/Windows/System32/OpenSSH/ssh.exe), como
   indica AGENTS.md. Comprueba también que el autor de los commits es el correcto:
   git var GIT_AUTHOR_IDENT
   En el PC, revisa también el clon del fork de dotnet/iot (C:\work\iot, ver AGENTS.md): sus
   ramas (la del analógico va apilada sobre la del #2625, la de entradas y salidas; el clon
   debería estar en `explorerhat-io`, en `c4119033` o más si los mantenedores subieron algo), que no
   haya cambios sin confirmar y lo que ha cambiado en upstream/main.

3. Ya en la Pi, sin mover nada, revisa su estado: rama y estado de git en ~/work/explorer-hat
   (que esté en main y al día), vcgencmd get_throttled, pinctrl get 19,20,21,26,4,17,27,5 y que
   no haya ningún programa del robot en marcha.

4. Después, resúmeme en pocas líneas: qué has entendido del proyecto, en qué fase estamos, qué
   queda pendiente según PLAN.md (empieza por "Al retomar") y qué propones hacer a continuación.
   Espera mi respuesta antes de cambiar nada.

Recuerda: háblame en español; nunca muevas los motores sin que te confirme antes que el robot tiene
las ruedas en el aire; lo que requiera sudo o pulsar teclas en un programa lo hago yo en mi
terminal (suelo tener una abierta por SSH en la Pi: dame los comandos para ejecutarlos allí); y todo lo que aprendamos que sea útil para el futuro debe quedar en AGENTS.md o en
PLAN.md, no solo en tu memoria de la sesión.
```

Si trabajas desde la propia Raspberry Pi, el paso 2 no hace falta: los comandos se ejecutan directamente. El clon de dotnet/iot solo está en el PC con Windows.

## Variante: el repaso de las lecciones con motores (hecho el 17/10/2026)

**Esta variante ya se hizo la noche del 17/10/2026** (resultados en `PLAN.md`, Fase 5). Solo sirve como receta si cambia `Common` o `SafeExplorerHat` y hay que repetirlo. La lección 07 (pads táctiles) también está hecha. El protocolo completo está en `PLAN.md`, Fase 5, "Repaso con el `Common` nuevo": qué comprobar antes, el orden, qué debe verse en cada lección y cómo parar si algo falla. Dura unos 40 minutos y necesita al usuario delante, con el robot con las ruedas en el aire.

Pega el prompt de arriba y **añade esto al final**:

```text
Hoy hacemos la tarea principal de "Al retomar" de PLAN.md: el repaso de las lecciones con motores
(04, 06, 09 y 10) con el Common nuevo. Haz los pasos 1 a 3 (incluido mirar dotnet/iot, que son 5
minutos) y propón el orden según el protocolo de la Fase 5 ("Repaso con el Common nuevo"). Espera mi
"adelante". Antes de CADA ejecución que mueva los motores, pregúntame si el robot tiene las ruedas en
el aire (aunque ya haya dicho "adelante") y empieza con una cuenta atrás de 8 segundos para que pueda
mirar. Si algo no cuadra, no sigas con la lección siguiente: para el robot y cuéntamelo. Al terminar,
anota los resultados en PLAN.md en una rama nueva con su PR, y pídeme /usage.
```

La lección 07 (pads táctiles) se hizo la noche del 17/10/2026. Desde entonces se probó también en el suelo, a 0,45 de velocidad. La probó también una niña de 10 años en el suelo, sin problemas. El checklist del taller se hizo el 25/10/2026 ([`checklist-taller.md`](checklist-taller.md)); la siguiente tarea propuesta es la Fase 7. La lección 10 de siguelíneas se descartó el 17/10/2026 y quedó como idea; las lecciones 11 y 12 pasaron a ser la 10 y la 11. El experimento de modelos está decidido (17/10/2026): se mantiene como está, con el asesor y `buscador` ("Experimento de modelos" en `PLAN.md`).

## Variante: seguir la Fase 7 (robot con IA): decidir el modelo y escribir el cerebro

Esta variante es para la sesión que sigue a la del 25/10/2026. Ese día se midió el modelo de lenguaje (`tools/medir-llm`) y se escribió y probó el servicio de órdenes de la Pi (`src/ExplorerHat.Orders`, con las ruedas en el aire: 7 de 7 y corte de red). Ver `PLAN.md`, Fase 7 y "Al retomar", puntos 6 y 6b.

**Antes de pegarla:**

1. **Las dos ramas de la Fase 7 están fusionadas** (PR #63 y #64, 25/10/2026): empieza en `main` y al día (`git pull`).
2. La Pi ya está en `main` (con el servicio de órdenes y los scripts de parada que conocen `Orders`). Vuelve a desplegar `~/apps/ExplorerHat.Orders` solo si cambia su código o el de `Common`.
3. Ollama está instalado en el portátil (controlador NVIDIA 621.01) con cuatro modelos de la biblioteca de Ollama (`qwen2.5:3b`, `qwen2.5:7b`, `llama3.2:latest` y `qwen3:4b-instruct-2507-q4_K_M`), medidos el 26/10/2026 (`PLAN.md`, Fase 7). El 25/10/2026 la red bloqueaba el servidor de archivos de Ollama; si vuelve a pasar, ver `tools/medir-llm/README.md`. Está pendiente decidir el modelo. El usuario montará un PC de gaming con una RTX 3060 Ti como servidor de Ollama para los portátiles del taller: pasos y registro en [`servidor-modelos-pc-gaming.md`](servidor-modelos-pc-gaming.md) (revisa su "Registro del montaje" y mide con `--url` y `--paralelo 6`).
4. Si la sesión va a mover el robot (por ejemplo, probar el servicio en el suelo): la Pi encendida y con wifi, la UPS cargada, el robot **sobre un bote o un libro, con las ruedas en el aire** (salvo la prueba en el suelo, que se pide aparte), y una terminal SSH abierta en la Pi.

Pega el prompt de arriba y **añade esto al final**:

```text
Hoy seguimos la Fase 7 (robot con IA). Haz los pasos 1 a 3 (incluido dotnet/iot, que son 5 minutos), mira
si las ramas de la Fase 7 están fusionadas (punto 6b de "Al retomar") y después propón el plan de lo que
sigue: decidir el modelo con los datos de "Medida del 25/10/2026" (hazme una pregunta por cada decisión, con
su contexto y tu recomendación) y escribir el cerebro en el portátil (C#), con la parada por palabra sin
pasar por el modelo. El servicio de órdenes ya está probado: no muevas el robot sin mi confirmación de que
tiene las ruedas en el aire. Antes de fijar el plan, consulta al asesor. Cuéntame cuánto puede tardar cada
comando largo. Al terminar, anota los resultados en PLAN.md, en una rama nueva con su PR.
```
