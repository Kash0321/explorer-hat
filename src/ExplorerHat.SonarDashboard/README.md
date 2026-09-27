# Panel de sensores de distancia

Muestra en la consola, en tiempo real, la distancia que mide cada uno de los tres sensores de
ultrasonidos HC-SR04 del robot. **No mueve los motores**: sirve para comprobar el montaje de los
sensores antes de probar `ExplorerHat.ObstacleAvoidance` y para ver cómo "ve" el robot.

```bash
dotnet run --project src/ExplorerHat.SonarDashboard/ExplorerHat.SonarDashboard.csproj
```

Pulsa **Ctrl+C** para salir; los pines de los sensores se liberan antes de terminar.

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
  En gris es el último valor bueno, porque la lectura más reciente falló.
* **OK**: cuántas de las últimas 20 lecturas funcionaron.
* **Estado**:
  * `✔ Funciona`: al menos el 75 % de las últimas lecturas son buenas.
  * `⚠ Falla a veces`: alguna lectura buena, pero muchas fallan. Puede ser un cable flojo, o que el
    sensor apunte a algo muy lejano, muy pequeño o en ángulo (el eco no vuelve).
  * `✖ Sin respuesta`: ninguna lectura buena. Revisa la alimentación del sensor (VCC y GND) y que
    TRIG y ECHO estén en la salida y la entrada indicadas.
