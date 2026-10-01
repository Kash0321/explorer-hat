# Retomar el trabajo con un asistente de IA

Prompt para empezar una sesión nueva de Claude Code (u otro asistente) en cualquier equipo, de forma que recupere el contexto del proyecto que quedó escrito en el repositorio.

**Antes de pegarlo:** abre el asistente en la carpeta del repositorio, estando en `main` y al día (`git pull`).

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
     sobre todo los #4 a #12, cuyas descripciones resumen cada fase.

2. Comprueba que llegas a la Raspberry Pi sin que te pida nada:
   ssh -o BatchMode=yes harlequin true
   En Windows, desde Git Bash, usa el ssh de Windows (/c/Windows/System32/OpenSSH/ssh.exe), como
   indica AGENTS.md. Comprueba también que el autor de los commits es el correcto:
   git var GIT_AUTHOR_IDENT

3. Ya en la Pi, sin mover nada, revisa su estado: rama y estado de git en ~/work/explorer-hat
   (que esté en main y al día), vcgencmd get_throttled, pinctrl get 19,20,21,26,4,17,27,5 y que
   no haya ningún programa del robot en marcha.

4. Después, resúmeme en pocas líneas: qué has entendido del proyecto, en qué fase estamos, qué
   queda pendiente según PLAN.md y qué propones hacer a continuación. Espera mi respuesta antes de
   cambiar nada.

Recuerda: háblame en español; nunca muevas los motores sin que te confirme antes que el robot tiene
las ruedas en el aire; lo que requiera sudo o pulsar teclas en un programa lo hago yo en mi
terminal (suelo tener una abierta por SSH en la Pi: dame los comandos para ejecutarlos allí); y todo lo que aprendamos que sea útil para el futuro debe quedar en AGENTS.md o en
PLAN.md, no solo en tu memoria de la sesión.
```

Si trabajas desde la propia Raspberry Pi, el paso 2 no hace falta: los comandos se ejecutan directamente.
