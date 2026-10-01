# Código compartido (Common)

Biblioteca con `SafeExplorerHat`, que usan los ejemplos que mueven los motores
(`BasicSample` y `ObstacleAvoidance`). No es un programa: no se ejecuta sola.

## Para qué sirve `SafeExplorerHat`

Se usa igual que `Iot.Device.ExplorerHat.ExplorerHat` (tiene las mismas propiedades `Motors` y `Lights`),
pero apaga el robot de forma segura. Hay dos razones para no usar la clase original.

### 1. Fallo al liberar `ExplorerHat` (Iot.Device.Bindings 4.2.0)

`ExplorerHat` pasa su `GpioController` a `Motors`, `Lights`, a cada `Led` y a cada `DCMotor`, y todos lo
liberan al terminar. El primero que se libera cierra todos los pines. Pero los hilos de PWM por software
de los motores siguen escribiendo en ellos, y el proceso muere con este error:

```
Can not write to pin 19 because it is not open
```

La solución está en la clase interna `SharedGpioController`: ignora las liberaciones hasta que
`SafeExplorerHat` le dice que ya puede cerrar los pines. Se podrá retirar cuando se publique el arreglo en
dotnet/iot (ver la Fase 6 de `PLAN.md`).

### 2. Parada de emergencia

Con Ctrl+C, `kill` o el cierre de la sesión SSH, el proceso de .NET termina **sin ejecutar los bloques
`using` ni `finally`**. Los pines se quedan como estaban y **los motores seguirían girando**.
`SafeExplorerHat` lo evita:

* Escucha estas señales: `SIGINT` (Ctrl+C), `SIGQUIT` (Ctrl+\), `SIGTERM` (`kill`) y `SIGHUP` (se cierra la
  sesión SSH).
* Escucha también las excepciones no controladas de cualquier hilo.
* **Primero para los motores y después escribe en la consola.** Si se pierde la sesión SSH, escribir en la
  consola falla. Antes de este cambio, el programa terminaba sin parar los motores y se quedaban al 100 %.
  Los errores de escritura se ignoran.

## Qué hace al liberarse (`Dispose`)

En este orden:

1. Quita el registro de las señales.
2. Pone `Motors.One.Speed` y `Motors.Two.Speed` a `0.0`.
3. Apaga las luces.
4. Libera `ExplorerHat` y después el controlador de pines.
5. Fuerza a nivel bajo los ocho pines de salida (19, 20, 21, 26, 4, 17, 27 y 5). El programa puede seguir
   ejecutándose durante una parada de emergencia y haber escrito un pin justo antes de liberarlo.

Es seguro llamarlo más de una vez.

## Cómo usarlo

Siempre dentro de un bloque `using`:

```csharp
using ExplorerHat.Common;

using (var hat = new SafeExplorerHat())
{
    hat.Lights.Blue.On();
    hat.Motors.Forwards();
    Thread.Sleep(500);
    hat.Motors.Stop();
}
```

El proyecto que lo use necesita esta referencia en su `.csproj`:

```xml
<ProjectReference Include="..\ExplorerHat.Common\ExplorerHat.Common.csproj" />
```

## Límites

* La parada de emergencia solo vale si el proceso aún puede ejecutar código. Si un depurador lo detiene o
  lo mata con `SIGKILL`, los motores se quedan como estuvieran (parados o a toda velocidad). Usa la tarea
  *Parar el robot* de VS Code o `bash tools/parar-robot.sh`.
* Los pines de los sensores HC-SR04 no los gestiona esta clase: cada programa libera los suyos.
