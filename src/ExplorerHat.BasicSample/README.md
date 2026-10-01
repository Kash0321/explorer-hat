# Laboratorio básico (BasicSample)

El primer programa del robot. Enciende luces y mueve los motores de uno en uno, con pausas de medio
segundo. Sirve para comprobar que el montaje funciona y para aprender a cambiar tiempos, velocidades y
luces. **Mueve los motores.**

## Qué hardware usa

* **Motores** (motores amarillos TT): motor 1 = GPIO 19 (velocidad) y 20 (dirección); motor 2 = GPIO 21 y 26.
  El motor `One` es la **rueda derecha** y `Two` la **izquierda**.
* **Luces:** GPIO 4 (azul), 17 (amarilla), 27 (roja) y 5 (verde).
* No usa sensores.

## Aviso de seguridad

> ⚠️ **Prueba siempre con las ruedas en el aire.** Levanta el robot o ponlo sobre un soporte antes de
> ejecutarlo.

* El programa dura unos 3 segundos y termina solo, con los motores parados y las luces apagadas.
* Para pararlo antes, pulsa **Ctrl+C** en la terminal: `SafeExplorerHat` para los motores antes de salir.
* Si algo falla, usa la tarea *Parar el robot* de VS Code o `bash tools/parar-robot.sh` en la Pi.

## Cómo ejecutarlo

**En la Raspberry Pi** (desde la carpeta del repositorio):

```bash
dotnet run --project src/ExplorerHat.BasicSample/ExplorerHat.BasicSample.csproj
```

**Desde el PC con VS Code:** `Terminal > Run Task... > Ejecutar en la Pi` y elige `ExplorerHat.BasicSample`.
La preparación (clave SSH y demás) está en el [README del repositorio](../../README.md).

## Qué hace

Primero escribe en la consola los datos del sistema (versión de .NET, sistema operativo y arquitectura).
Después ejecuta seis pasos. Cada paso dura `MOTOR_TIME` (500 ms), enciende un par o un trío de luces y
termina parando los motores y apagando las luces.

| Paso | Motores | Luces |
|---|---|---|
| 1 | Los dos hacia delante | azul, amarilla y verde |
| 2 | Los dos hacia atrás | azul, amarilla y roja |
| 3 | Solo el motor `One` hacia delante (velocidad 0,8) | azul y verde |
| 4 | Solo el motor `One` hacia atrás (velocidad −0,8) | azul y roja |
| 5 | Solo el motor `Two` hacia delante (velocidad 0,8) | amarilla y verde |
| 6 | Solo el motor `Two` hacia atrás (velocidad −0,8) | amarilla y roja |

La velocidad va de −1,0 (toda hacia atrás) a 1,0 (toda hacia delante). El 0 es parado.
Los pasos 1 y 2 usan `Motors.Forwards()` y `Motors.Backwards()` sin indicar velocidad.

El programa usa `SafeExplorerHat` dentro de un bloque `using`. Así, al terminar, se paran los motores y
se liberan los pines (ver el [README de `ExplorerHat.Common`](../ExplorerHat.Common/README.md)).

## Ideas para el taller

Cambia una cosa cada vez y vuelve a ejecutar:

* **Cambia el tiempo.** Sube `MOTOR_TIME` a `1000` para que cada paso dure un segundo.
* **Cambia la velocidad.** Pon `hat.Motors.One.Speed = 0.5d;` en vez de `0.8d`. ¿Se nota la diferencia?
* **Elige la velocidad de los dos motores.** Prueba `hat.Motors.Forwards(0.5)`.
* **Cambia las luces.** Cambia `Blue` por `Red` en un paso. ¿Qué color se enciende?
* **Gira el robot.** Pon `One.Speed = 0.8d` y `Two.Speed = -0.8d` a la vez, espera medio segundo y
  párales. ¿Hacia qué lado gira?
* **Haz un baile.** Escribe tu propia secuencia: avanza, gira, retrocede y enciende las luces a tu gusto.
* **Usa un bucle.** Repite un paso tres veces con `for (int i = 0; i < 3; i++) { ... }`.

Cuando termines un movimiento, escribe `hat.Motors.Stop();`.
