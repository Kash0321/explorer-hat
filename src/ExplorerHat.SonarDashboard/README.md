# Panel de sensores de distancia

Muestra en la consola, en tiempo real, la distancia que mide cada uno de los tres sensores de
ultrasonidos HC-SR04 del robot. **No mueve los motores**: sirve para comprobar el montaje de los
sensores antes de probar `ExplorerHat.ObstacleAvoidance` y para ver cómo "ve" el robot.

## Cómo ejecutarlo

**En la Raspberry Pi:**

```bash
dotnet run --project src/ExplorerHat.SonarDashboard/ExplorerHat.SonarDashboard.csproj
```

**Desde el PC con VS Code:** `Terminal > Run Task... > Ejecutar en la Pi` y elige `ExplorerHat.SonarDashboard`
(preparación en el [README del repositorio](../../README.md)).

Pulsa **Ctrl+C** para salir; los pines de los sensores se liberan antes de terminar. El panel se actualiza
sin parar: mide los tres sensores uno detrás de otro, con 60 ms de pausa entre ellos.

No usa los motores, así que no hace falta poner el robot con las ruedas en el aire.

```
                      Sensores de distancia HC-SR04
╭───────────┬──────────────┬───────────┬─────────────────┬────────────┬───────╮
│ Sensor    │ TRIG → ECHO  │ Distancia │ 0 – 2 m         │ Estado     │    OK │
├───────────┼──────────────┼───────────┼─────────────────┼────────────┼───────┤
│ Izquierda │ OUT3 → IN3   │   71.6 cm │ █████░░░░░░░░░░ │ ✔ Funciona │ 20/20 │
│           │ GPIO 13 → 24 │           │                 │            │       │
│ Centro    │ OUT1 → IN1   │  135.7 cm │ ██████████░░░░░ │ ✔ Funciona │ 17/20 │
│           │ GPIO 6 → 23  │           │                 │            │       │
│ Derecha   │ OUT2 → IN2   │  129.6 cm │ ██████████░░░░░ │ ✔ Funciona │ 20/20 │
│           │ GPIO 12 → 22 │           │                 │            │       │
╰───────────┴──────────────┴───────────┴─────────────────┴────────────┴───────╯
```

## Conexiones

Cada sensor usa una **salida** del Explorer HAT para el disparo (TRIG) y una **entrada** para el eco
(ECHO). Las etiquetas `OUT1`… e `IN1`… son las que aparecen serigrafiadas en el HAT.

| Sensor    | TRIG            | ECHO           |
|-----------|-----------------|----------------|
| Izquierda | OUT3 (GPIO 13)  | IN3 (GPIO 24)  |
| Centro    | OUT1 (GPIO 6)   | IN1 (GPIO 23)  |
| Derecha   | OUT2 (GPIO 12)  | IN2 (GPIO 22)  |

Quedan libres OUT4 (GPIO 16) e IN4 (GPIO 25).

## Cómo leer el panel

* **Distancia**: en rojo si hay algo a menos de 20 cm, en amarillo a menos de 50 cm, en verde más lejos.
  En gris es el último valor bueno, porque la lectura más reciente falló. La barra va de 0 a 2 m.
* **OK**: cuántas de las últimas 20 lecturas funcionaron.
* **Estado**:
  * `✔ Funciona`: al menos el 75 % de las últimas lecturas son buenas.
  * `⚠ Falla a veces`: alguna lectura buena, pero muchas fallan. Puede ser un cable flojo, o que el
    sensor apunte a algo muy lejano, muy pequeño o en ángulo (el eco no vuelve).
  * `✖ Sin respuesta`: ninguna lectura buena. Revisa la alimentación del sensor (VCC y GND) y que
    TRIG y ECHO estén en la salida y la entrada indicadas.

## Tecla F: filtro de lecturas falsas

A veces un HC-SR04 pierde el eco del obstáculo y mide algo mucho más lejos (por ejemplo, la pared de
detrás). Pulsa **F** para activar o desactivar el filtro. Empieza **activado**. El pie del panel dice
si está `activado` o `desactivado`.

* **Con filtro:** la distancia es la más cercana de las dos últimas lecturas. Una lectura sin eco cuenta
  como 400 cm. Un salto aislado a una distancia mayor no se ve. Es el mismo filtro que usa
  [`ExplorerHat.ObstacleAvoidance`](../ExplorerHat.ObstacleAvoidance/README.md).
* **Sin filtro:** se ve la última lectura tal cual. Si falla, se ve el último valor bueno en gris.

El filtro no suaviza el temblor normal de la medida (unos ±8 cm en el centro) ni dos saltos seguidos.

## Modo registro: guardar las lecturas en un archivo

Con `--registro` no se muestra el panel: el programa mide durante un tiempo y guarda cada lectura en un
archivo. Sirve para estudiar las lecturas falsas con el robot quieto. El tiempo es opcional y, si no lo
indicas, son **60 segundos**.

```bash
dotnet run --project src/ExplorerHat.SonarDashboard/ExplorerHat.SonarDashboard.csproj -- --registro lecturas.csv 30
```

Pulsa **Ctrl+C** para terminar antes. Al acabar escribe `Registro terminado.`

El archivo es CSV con `;` como separador y una fila por lectura. Las columnas son:

| Columna | Significado |
|---|---|
| `hora` | Hora de la lectura (`HH:mm:ss.fff`) |
| `sensor` | `Izquierda`, `Centro` o `Derecha` |
| `ok` | `True` si el sensor recibió eco; `False` si no |
| `cm` | Distancia leída, en centímetros. Vacía si la lectura falló |
| `filtrada` | Distancia con el filtro (la más cercana de las dos últimas lecturas; sin eco cuenta 400 cm) |
| `ms` | Lo que tardó la lectura, en milisegundos |
