# 🤖 AGENTS.md - Reglas de Contexto para Claude Code / Asistentes de IA

Este archivo define las restricciones operativas, limitaciones de hardware y directrices de código para cualquier agente de IA que manipule este repositorio de robótica educativa.

## 📌 Contexto de Ejecución del Entorno
* **Host Remoto:** El código se edita desde el PC pero se ejecuta de forma remota en una Raspberry Pi 3 B+ a través de SSH.
* **Versión de la Plataforma:** .NET 10.0.401 (C# 14).
* **Librerías Clave:** `System.Device.Gpio` e `Iot.Device.Bindings` (Versiones compatibles con .NET 10).
* **Hardware Específico:** Los motores y luces están mapeados nativamente bajo la abstracción `Iot.Device.ExplorerHat.ExplorerHat`.

## ⛔ Restricciones Estrictas de Código
1. **Sintaxis Clara y Pedagógica:** El código final debe ser leído por niños. Evita patrones avanzados innecesarios (como inyección de dependencias compleja o abstracciones pesadas). Prioriza estructuras secuenciales, bucles `for/while` tradicionales y métodos explícitos (`Speed = 0.5`).
2. **Control del Tiempo:** El uso de `Thread.Sleep()` o `Task.Delay()` es explícitamente bienvenido para que los niños puedan cronometrar los movimientos del robot de forma visual ("Avanza 2 segundos, gira 1 segundo").
3. **Liberación de Pines:** Usa `SafeExplorerHat` (proyecto `src/ExplorerHat.Common`) en lugar de `Iot.Device.ExplorerHat.ExplorerHat`, siempre dentro de un bloque `using`. Tiene las mismas propiedades `Motors` y `Lights`, y además:
   * Evita el fallo de `Iot.Device.Bindings` 4.2.0 por el que liberar `ExplorerHat` mata el proceso (`Can not write to pin 19 because it is not open`).
   * Al liberarse para ambos motores (`Speed = 0.0`), apaga las luces y libera los pines.
   * Hace la parada de emergencia ante Ctrl+C, `kill` o el cierre de la sesión SSH. Sin ella, el proceso termina sin ejecutar `using` ni `finally` y los pines se quedan como estaban: **los motores seguirían girando**.

## 🛠️ Directrices para Tareas de Claude Code
* **Actualización de Dependencias:** Al añadir paquetes, usa la sintaxis explícita apuntando al archivo `.csproj` correspondiente: `dotnet add src/[Proyecto]/[Proyecto].csproj package [Nombre]`.
* **Manejo del Almacenamiento:** El espacio en la tarjeta MicroSD es limitado (~7GB libres). Evita generar logs masivos o archivos basura durante las pruebas de compilación.
* **Seguridad Física del Robot:** Al sugerir algoritmos de movimiento de motores, asegura siempre un método o bloque de cierre que asigne `Speed = 0.0` a ambos motores al finalizar o al capturar una excepción (parada de emergencia).
