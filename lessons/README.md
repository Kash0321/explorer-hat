# Lecciones

Itinerario del taller, de lo más sencillo a lo más difícil. Cada lección es un proyecto pequeño con un único
`Program.cs` que se lee de arriba abajo. El código y sus comentarios están en inglés, como el resto del
repositorio; cada lección tiene un README en español para el monitor.

| Lección | Qué aprende | ¿Mueve los motores? |
|---|---|---|
| [01 Luces](Lesson01.Lights/README.md) | Instrucciones en orden, esperar con `Thread.Sleep`, el bucle `for` | No |
| [02 Semáforo](Lesson02.TrafficLight/README.md) | Variables, repetir un ciclo con `for` | No |
| [03 Motores](Lesson03.Motors/README.md) | Velocidad de cada motor, avanzar, retroceder y girar | **Sí** |
| [04 Cuadrado](Lesson04.Square/README.md) | Métodos propios, bucles con movimiento, `if`/`else`, calibrar un giro | **Sí** |
| [05 Botones](Lesson05.Buttons/README.md) | Entradas y salidas digitales, `if` con un botón, contar pulsaciones; montar un pulsador y un LED en una protoboard | No |
| [06 Distancia](Lesson06.Distance/README.md) | Un sensor que da números, `if`/`else if`/`else`, bucle `while` y `break` | **Sí**, con `moveMotors = true` |
| [07 Pads táctiles](Lesson07.Touch/README.md) | Un sensor táctil capacitivo, un mando a distancia que solo mueve el robot mientras tocas, `if`/`else if` con cuatro opciones | **Sí**, con `moveMotors = true` |
| [08 Sensores analógicos](Lesson08.Analog/README.md) | Una tensión que cambia poco a poco, el potenciómetro como divisor de tensión, poner un número en una escala; montar un potenciómetro | No |
| [09 Robot autónomo](Lesson09.Autonomous/README.md) | Mirar, pensar y actuar; juntar condiciones con `\|\|` y `&&` | **Sí** |
| [10 Contar vueltas](Lesson10.Odometry/README.md) | Odometría con un sensor de velocidad, el número pi, `while` con dos condiciones | **Sí** |
| [10 Ir recto](Lesson10.Straight/README.md) | Realimentación: dos sensores de velocidad y control proporcional | **Sí** |
| [10 El cuadrado perfecto](Lesson10.Square/README.md) | Girar ángulos exactos con geometría y pulsos; calibrar lo que resbala | **Sí** |
| [11 La pantalla](Lesson11.Screen/README.md) | Texto y variables en una pantalla, líneas desde 0, una barra con un bucle | No |

El catálogo no tiene la lección 10 de siguelíneas: se guarda como idea en `PLAN.md` (Fase 5, "Ideas para ampliar").

## Cómo ejecutar una lección

**Desde el PC con VS Code:** `Terminal > Run Task... > Ejecutar en la Pi` y elige la lección (`lessons/...`).
Para depurarla, abre su `Program.cs` y pulsa F5 con la configuración *Depurar en la Pi el programa abierto*.
La preparación (clave SSH y demás) está en el [README del repositorio](../README.md).

**En la Raspberry Pi** (desde la carpeta del repositorio):

```bash
dotnet run --project lessons/Lesson01.Lights/Lesson01.Lights.csproj
```

Para parar una lección antes de que termine, pulsa **Ctrl+C**.

## Seguridad

> ⚠️ **Las lecciones 03, 04, 06, 07, 09 y 10 mueven los motores** (la 06 y la 07, solo con `moveMotors = true`). Prueba siempre primero con las ruedas en el aire, con el robot
> sobre un bote o un libro. Las normas completas están en el [README del repositorio](../README.md).

* Todas las lecciones usan `SafeExplorerHat` dentro de un bloque `using`. Al terminar, o con Ctrl+C, para los
  motores y apaga las luces.
* Si algo falla, usa la tarea *Parar el robot* de VS Code o `bash tools/parar-robot.sh` en la Pi. Las dos
  reconocen los programas que se llaman `LessonNN.*`.

## Cómo es el código de una lección

* Sin clases ni `namespace`: el programa empieza directamente con las instrucciones (*top-level statements*).
* Los números que el niño puede cambiar están arriba, en variables con nombre (`speed`, `turnTime`...).
* Cada movimiento termina con `Speed = 0.0` en los dos motores.
* Si añades una lección, sigue la numeración (`Lesson05.Buttons`...), añádela a `ExplorerHatSandbox.slnx` y a
  la lista `program` de `.vscode/tasks.json`.
