# Retomar el trabajo con un asistente de IA

Prompt para empezar una sesión nueva de Claude Code (u otro asistente) en cualquier equipo, de forma que recupere el contexto del proyecto que quedó escrito en el repositorio.

**Antes de pegarlo:**

1. Abre el asistente en la carpeta del repositorio, estando en `main` y al día (`git pull`).
2. Comprueba la configuración del experimento de modelos (`.claude/settings.json`): `/model` debe decir Sonnet 5.5 y `/advisor`, Opus. El subagente `buscador` aparece entre los agentes disponibles.
3. Escribe `/usage` y apunta el resultado: es la base para medir el coste de la sesión (el experimento pide compararlo tras tres sesiones; la primera fue la del 17/10/2026).
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
     sobre todo los #4 a #51, cuyas descripciones resumen cada fase y cada lección.
   - Lo que tenemos abierto en dotnet/iot (Fase 6): los PR e incidencias de la tabla del "Estado
     actual" de PLAN.md (el 17/10/2026: PR #2616 y #2625, incidencias #2614, #2615 y #2617,
     y el PR #2610 de pgrawehr, donde comentamos dos fallos), con sus comprobaciones, comentarios y revisiones
     nuevos (gh pr checks <n> -R dotnet/iot, gh api repos/dotnet/iot/issues/<n>/comments y, en los
     PR, gh api repos/dotnet/iot/pulls/<n>/reviews y .../pulls/<n>/comments). Si hay
     comentarios, dímelo antes de responder.

2. Comprueba que llegas a la Raspberry Pi sin que te pida nada:
   ssh -o BatchMode=yes harlequin true
   En Windows, desde Git Bash, usa el ssh de Windows (/c/Windows/System32/OpenSSH/ssh.exe), como
   indica AGENTS.md. Comprueba también que el autor de los commits es el correcto:
   git var GIT_AUTHOR_IDENT
   En el PC, revisa también el clon del fork de dotnet/iot (C:\work\iot, ver AGENTS.md): sus
   ramas (la del analógico va apilada sobre la del #2625, la de entradas y salidas), que no
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

**Esta variante ya se hizo la noche del 17/10/2026** (resultados en `PLAN.md`, Fase 5). Solo sirve como receta si cambia `Common` o `SafeExplorerHat` y hay que repetirlo. La tarea principal pendiente es ahora la lección 07 (pads táctiles). El protocolo completo está en `PLAN.md`, Fase 5, "Repaso con el `Common` nuevo": qué comprobar antes, el orden, qué debe verse en cada lección y cómo parar si algo falla. Dura unos 40 minutos y necesita al usuario delante, con el robot con las ruedas en el aire.

Pega el prompt de arriba y **añade esto al final**:

```text
Hoy hacemos la tarea principal de "Al retomar" de PLAN.md: el repaso de las lecciones con motores
(04, 06, 09 y 11) con el Common nuevo. Haz los pasos 1 a 3 (incluido mirar dotnet/iot, que son 5
minutos) y propón el orden según el protocolo de la Fase 5 ("Repaso con el Common nuevo"). Espera mi
"adelante". Antes de CADA ejecución que mueva los motores, pregúntame si el robot tiene las ruedas en
el aire (aunque ya haya dicho "adelante") y empieza con una cuenta atrás de 8 segundos para que pueda
mirar. Si algo no cuadra, no sigas con la lección siguiente: para el robot y cuéntamelo. Al terminar,
anota los resultados en PLAN.md en una rama nueva con su PR, y pídeme /usage.
```

La siguiente tarea propuesta es la lección 07 (pads táctiles). Otras opciones que se ofrecieron: un checklist del taller, la lección 10 y la Fase 7. La sesión 3 de 3 del experimento de modelos es la próxima: pide `/usage` al empezar y al terminar.
