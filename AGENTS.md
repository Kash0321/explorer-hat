# 🤖 AGENTS.md - Reglas de Contexto para Claude Code / Asistentes de IA

Este archivo define las restricciones operativas, limitaciones de hardware y directrices de código para cualquier agente de IA que manipule este repositorio de robótica educativa. Es también la **memoria del proyecto** para los asistentes: todo lo que una sesión nueva necesita saber para continuar el trabajo debe estar aquí, en `PLAN.md` o en el `README.md`, no solo en la memoria local de una sesión.

## 📌 Contexto del proyecto
* **Objetivo:** talleres con niños para aprender programación, electrónica y robótica con C#. El autor del repositorio escribió el binding `Iot.Device.ExplorerHat` de dotnet/iot (PR #926).
* **Estado y siguientes pasos:** en `PLAN.md` (fases 0–7, con lo probado y lo pendiente). Marca allí el avance (`[ ]`, `[~]`, `[x]`) y anota los resultados de las pruebas.
* **Retomar el trabajo en una sesión nueva:** prompt en `docs/retomar-sesion.md`.
* **El usuario** es experto en programación; la electrónica es su afición y quiere aprender: explícale los conceptos de electrónica según aparezcan (qué es, por qué importa y cómo comprobarlo), sin dar por sabido el vocabulario.
* **Idioma:** conversación, documentación y mensajes de commit en español. Comentarios del código en inglés, como el resto del código.

## 🖥️ Entorno de ejecución
* **Raspberry Pi 3 B+** (`harlequin`), Debian 13 de 64 bits (`linux-arm64`), 1 GB de RAM y unos 3 GB libres en la microSD: evita generar logs masivos o archivos basura.
* **.NET 10.0.401** en `~/.dotnet`, con enlace en `/usr/local/bin/dotnet` para que funcione también por SSH sin sesión interactiva.
* **Repositorio clonado en la Pi** en `~/work/explorer-hat`. Los programas desplegados desde el PC van a `~/apps/<Proyecto>` y el depurador está en `~/vsdbg`.
* **I2C habilitado** (`/dev/i2c-1`): el HAT responde en 0x28 (táctil CAP1208) y 0x48 (analógico ADS1015). La UPS responde en 0x42 (INA219). Por SSH no interactivo, `i2cdetect` e `i2cget` están en `/usr/sbin`, fuera del `PATH`.
* **Librerías:** `System.Device.Gpio` e `Iot.Device.Bindings` 4.2.0.
* **PC de desarrollo:** Windows con VS Code y .NET 10. Desde ahí se despliega y depura con las tareas de `.vscode/` (ver README). La extensión de C# debe ser la **2.140.9**: la 2.160.x rompe los puntos de interrupción remotos (dotnet/vscode-csharp#9802). **C# Dev Kit debe estar deshabilitada en este repositorio** (*Deshabilitar (área de trabajo)*): la 3.40.210 con la extensión de C# 2.140.9 deja el depurador parado en el primer punto de interrupción, sin responder a F10 ni a F5. Al deshabilitarla, comprueba que la extensión C# sigue habilitada (si no, VS Code dice que el tipo `coreclr` no es compatible).

## 🔌 Hardware (Pimoroni Explorer HAT Pro)
* **Motores:** motor 1 = GPIO 19 (velocidad) / 20 (dirección); motor 2 = GPIO 21 / 26. **El motor `One` es la rueda derecha** y `Two` la izquierda (girar a la derecha: `One` hacia atrás, `Two` hacia delante). Motores amarillos TT con reductora 1:48.
* **Luces:** GPIO 4 (azul), 17 (amarilla), 27 (roja), 5 (verde).
* **Sensores HC-SR04:** izquierda TRIG 13 / ECHO 24, centro 6 / 23, derecha 12 / 22. OUT4 (GPIO 16) e IN4 (GPIO 25): el LED y el pulsador de la lección 05, en una protoboard (se pueden quitar); IN4 tiene pull-down dentro del HAT. En la lección 11, IN4 es el sensor de velocidad LM393 de la rueda derecha (D0); en su segunda parte (`Lesson11.Straight`), IN3 es el de la rueda izquierda, en lugar del ECHO del HC-SR04 izquierdo (hay que desconectarlo y volver a conectarlo después).
* **La velocidad de los motores es PWM por software** (un hilo del programa). Si el programa muere o un depurador lo detiene, cada motor se queda como estuviera: **parado o a toda velocidad**.

## ⛔ Restricciones Estrictas de Código
1. **Sintaxis Clara y Pedagógica:** El código final debe ser leído por niños. Evita patrones avanzados innecesarios (como inyección de dependencias compleja o abstracciones pesadas). Prioriza estructuras secuenciales, bucles `for/while` tradicionales y métodos explícitos (`Speed = 0.5`).
   * **Lecciones (`lessons/LessonNN.Nombre`):** un único `Program.cs` con *top-level statements* (sin clase ni `namespace`), los valores que el niño puede cambiar en variables al principio, código y comentarios en inglés y un README en español con retos. Los scripts de parada reconocen los procesos `LessonNN.*`.
2. **Control del Tiempo:** El uso de `Thread.Sleep()` o `Task.Delay()` es explícitamente bienvenido para que los niños puedan cronometrar los movimientos del robot de forma visual ("Avanza 2 segundos, gira 1 segundo").
3. **Liberación de Pines:** Usa `SafeExplorerHat` (proyecto `src/ExplorerHat.Common`) en lugar de `Iot.Device.ExplorerHat.ExplorerHat`, siempre dentro de un bloque `using`. Tiene las mismas propiedades `Motors` y `Lights`, y además:
   * Evita el fallo de `Iot.Device.Bindings` 4.2.0 por el que liberar `ExplorerHat` mata el proceso (`Can not write to pin 19 because it is not open`).
   * Al liberarse para ambos motores (`Speed = 0.0`), apaga las luces, libera los pines y los fuerza a nivel bajo.
   * Hace la parada de emergencia ante Ctrl+C, `kill`, el cierre de la sesión SSH o una excepción no controlada. Sin ella, el proceso termina sin ejecutar `using` ni `finally` y los pines se quedan como estaban: **los motores seguirían girando**.
   * En un manejador de señal, **para los motores antes de hacer nada más**: al perder la sesión SSH, escribir en la consola falla (pasó: los motores se quedaron al 100 %).

## 🦺 Seguridad física del robot (obligatorio para los agentes)
* **Antes de cualquier ejecución que mueva los motores, pide confirmación explícita de que el robot tiene las ruedas en el aire**, aunque el usuario ya haya dicho "adelante".
* Comprueba el estado real de los pines en vez de suponerlo: `pinctrl get 19,20,21,26,4,17,27,5` (`lo` = apagado).
* **Parada de emergencia:** `bash tools/parar-robot.sh` en la Pi, o `ssh harlequin 'bash -s' < tools/parar-robot.sh` desde el PC. Los `.sh` tienen que estar con finales de línea LF también en Windows (lo obliga `.gitattributes`): con CRLF, la parada desde el PC no para nada (pasó el 03/10/2026).
* Para registrar pruebas usa `tools/vigilar-pines.sh` y `tools/vigilar-tension.sh` lanzados con `setsid -f ... > /dev/null 2>&1 < /dev/null` (la línea exacta está al principio de cada script), para que sigan grabando aunque se corte la sesión. Con `setsid nohup ... &`, el `ssh` lanzado desde el PC no devuelve el control. Para pararlos por SSH, no uses `pkill -f` con un patrón que también aparezca en tu propio comando (por ejemplo, `vigilar-pines.sh`): mata la propia sesión. Por la misma razón, `pgrep -fc` cuenta tu propio comando si contiene el nombre del programa (por ejemplo, `Lesson03.Motors`): parte el nombre en una variable (`P=Lesson0; ... ${P}3.Motors`).
* Al sugerir algoritmos de movimiento, asegura siempre un bloque de cierre que asigne `Speed = 0.0` a ambos motores al finalizar o al capturar una excepción.
* La Pi está configurada para cerrar las sesiones SSH perdidas (ClientAlive y `tcp_retries2`, ver README): si se corta la wifi, los motores se paran en unos 15–30 s.

## 🔧 Cómo trabaja un asistente en este proyecto
* **Desde la propia Pi:** los comandos se ejecutan directamente.
* **Desde el PC:** la Pi es accesible como `ssh harlequin` (clave SSH, sin contraseña). Ejecuta en la Pi con `ssh harlequin "cd ~/work/explorer-hat && ..."`. Prueba antes que el acceso no pide nada: `ssh -o BatchMode=yes harlequin true`. En Windows, desde Git Bash usa siempre el `ssh.exe` de Windows (`/c/Windows/System32/OpenSSH/ssh.exe`, y su `scp.exe`): el `ssh` de Git Bash no llega a la clave (`Permission denied (publickey,password)`). Desde PowerShell, `ssh` y `scp` ya son los de Windows, y las tareas de `.vscode/` también los usan. Pon un límite de tiempo a los comandos por SSH (`timeout 30 ssh.exe ...`) y, si uno tarda, di al usuario qué pasa.
* **Autor de los commits:** `Sergio Castillo <castillo.sergio@gmail.com>`, dueño de la cuenta de GitHub Kash0321. En el PC con Windows el `.gitconfig` global tiene otro correo, así que el repositorio tiene configuración local: `user.name`, `user.email` y `credential.https://github.com.username Kash0321` (para que el Credential Manager no pregunte la cuenta en cada `push`). Antes del primer commit en un clon nuevo, comprueba `git var GIT_AUTHOR_IDENT`.
* **Lo que hace el usuario, no el asistente:** todo lo que requiere `sudo` (pide contraseña y no hay terminal) y los programas que esperan una tecla (`Console.ReadKey`, como ObstacleAvoidance): dale los comandos exactos para su terminal. El usuario suele tener abierta una terminal SSH en la Pi: dale los comandos para ejecutarlos allí directamente, sin el prefijo `ssh harlequin`.
* **Ramas y PR:** una rama por fase de `PLAN.md` (`fase-N-descripcion`) a partir de `main`, subida a GitHub y fusionada por PR (`gh` está autenticado en la Pi). El usuario hace el merge; después, comprueba que la rama no tiene commits fuera de `origin/main`, bórrala (local y remota) y actualiza `main`.
* **Markdown:** una sublista empieza en la columna del texto de su elemento padre (2 espacios bajo `- [x] `), y sus tablas y párrafos tras una línea en blanco, en la columna del texto de la sublista. Con 4 espacios de más, GitHub pega la sublista al párrafo y muestra las tablas como código. Para comprobarlo: `gh api markdown -f mode=gfm -F text=@PLAN.md`.
* **Actualización de Dependencias:** Al añadir paquetes, usa la sintaxis explícita apuntando al archivo `.csproj` correspondiente: `dotnet add src/[Proyecto]/[Proyecto].csproj package [Nombre]`.
* **Alimentación:** el robot usa una **Waveshare UPS HAT (B)** (2×18650 en serie, salida de 5 V hasta 5 A) debajo de la Pi: sin caídas de tensión ni con los motores. `tools/vigilar-tension.sh` registra también la tensión y la corriente de sus baterías. Con cargador, muchos cables micro-USB no dan la corriente suficiente. Si hay comportamientos raros, mira `vcgencmd get_throttled` (`0x0` = bien; bit 0 = tensión baja ahora). Resultados y método de las pruebas con baterías en `PLAN.md` (Fase 2).
