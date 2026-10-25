# Checklist del taller

Lista para preparar y dar un taller con el robot (niños de 10 a 14 años, un robot por cada 2 o 3 niños). Está pensada
para el monitor. Las órdenes que empiezan por `sudo` las ejecuta una persona en su terminal; ningún paso de esta lista
mueve los motores sin que el robot tenga las ruedas en el aire.

## 1. Semanas antes

### Material por robot

- [ ] Robot montado: Raspberry Pi 3 B+, Explorer HAT Pro y UPS HAT (B) con sus dos baterías 18650 (montaje en el [README](../README.md)).
- [ ] Chasis con los dos motores y las ruedas. Las tres sondas de distancia (HC-SR04) y los dos sensores de velocidad (LM393).
- [ ] Pantalla LCD con sus cuatro cables (colores en el README: amarillo = GND, verde = VCC a 3v3, azul = SDA, blanco = SCL).
- [ ] Cargador de la UPS (8,4 V) y un potenciómetro para la lección 08.
- [ ] Para la lección 05: protoboard, pulsador, LED, resistencia de 330 Ω y cables Dupont.
- [ ] Un bote o un libro por robot, para poner las ruedas en el aire.

### Material del aula

- [ ] Un portátil por robot (o uno por grupo), con cargador.
- [ ] Una red wifi que funcione sin depender de nadie (un router propio o un móvil con punto de acceso). Ver el apartado 2.
- [ ] Suelo liso y libre de unos 2 m × 1 m por grupo, cajas para hacer obstáculos, cinta métrica, transportador y cinta para marcar.
- [ ] Regletas, un cargador de repuesto y baterías de repuesto cargadas.
- [ ] Una copia en papel de las normas de seguridad (apartado 5) para cada mesa.

### Portátiles

Cada portátil necesita lo siguiente (detalle en el [README](../README.md) y en `PLAN.md`, Fase 3):

- [ ] Windows, .NET 10 y VS Code.
- [ ] La extensión de C# en la versión **2.140.9**, y la extensión C# Dev Kit **deshabilitada** en este repositorio.
- [ ] El repositorio clonado y una clave SSH copiada a la Pi de su robot.
- [ ] Que apunte a **su** robot: si hay varios robots en la misma red, cada uno necesita un nombre distinto (`hostname`) y
  cada portátil debe usar el suyo (`Host` en `~/.ssh/config` y `explorerHat.host` en `.vscode/settings.json`).
- [ ] Prueba hecha: `ssh <nombre> true` entra sin pedir contraseña y la tarea *Desplegar en la Pi* termina bien.

### Datos de menores

- [ ] Decidir quién guarda las autorizaciones de las familias y si se hacen fotos o vídeos.
- [ ] En los talleres de IA con voz, todo funciona en local y no sale a internet (ver `PLAN.md`, Fase 7).

## 2. Red del aula

Los robots y los portátiles tienen que estar en la misma red. La red de casa no estará en el aula: hay que preparar otra y
**probar todo en ella un día antes**.

- [ ] Elegir la red: un router propio o un móvil con punto de acceso (zona wifi). Anota su nombre y su clave.
- [ ] Añadir esa red a la Pi, una sola vez, desde la red de casa. La Pi usa NetworkManager. En la terminal de la Pi:

  ```bash
  sudo nmcli connection add type wifi ifname wlan0 con-name taller ssid "NOMBRE_DE_LA_RED" \
      wifi-sec.key-mgmt wpa-psk wifi-sec.psk "CLAVE" connection.autoconnect yes
  nmcli connection show
  ```

- [ ] Con las dos redes a la vez, la Pi elige por prioridad. Para que gane la del taller:
  `sudo nmcli connection modify taller connection.autoconnect-priority 10`.
- [ ] En el aula, buscar la IP de la Pi (en el panel del router o del móvil) y ponerla en `HostName` de `~/.ssh/config`,
  o usar `harlequin.local`. Con la IP fija es más rápido (ver el README).
- [ ] Prueba completa en esa red: entrar por SSH, desplegar una lección y pararla con Ctrl+C.
- [ ] Tener la clave y el nombre de la red también en papel.

## 3. El día antes (o la mañana del taller)

Haz esto con cada robot, **con las ruedas en el aire**:

- [ ] Cargar las baterías de la UPS y comprobar que el robot arranca sin cable micro-USB.
- [ ] Actualizar el código en la Pi: `git pull --ff-only` en `~/work/explorer-hat` y desplegar desde `main`
  (tarea *Desplegar en la Pi* en VS Code). Si cambió `src/ExplorerHat.Common`, desplegar de nuevo todas las lecciones.
- [ ] Comprobar los pines: `pinctrl get 19,20,21,26,4,17,27,5` debe mostrar `lo` en los ocho.
- [ ] Comprobar los chips: `/usr/sbin/i2cdetect -y 1` debe mostrar 0x27, 0x28, 0x42 y 0x48. Si falta el 0x27, mira los
  cables de la pantalla (SDA y SCL cambiados, o un cable suelto).
- [ ] Ejecutar `bash tools/probar-cableado.sh` (luces, sensores de velocidad, sentido de cada motor, pantalla y sensores de
  distancia). Tú ves el sentido real de las ruedas: los pines solo dicen lo que manda la Pi.
- [ ] `vcgencmd get_throttled` debe dar `0x0` (sin tensión baja ni límite de temperatura).
- [ ] Probar la parada: lanzar una lección, pulsar Ctrl+C y comprobar que los pines vuelven a `lo`. Tener a mano
  `bash tools/parar-robot.sh`.
- [ ] Probar una lección que se mueva, primero en el aire y después en el suelo de la sala.
- [ ] Preparar el suelo: marcar la zona, poner las cajas y retirar cables.

## 4. Durante el taller

### Ritmo de una sesión de 90 minutos

| Minutos | Qué pasa |
|---|---|
| ≈ 10 | **El reto.** El monitor enseña el robot haciendo algo nuevo. ¿Cómo lo hace? |
| ≈ 15 | **La idea.** Un concepto de programación y uno de electrónica, con ejemplos. |
| ≈ 50 | **¡A programar!** Cambiar el código y probar con las ruedas en el aire, y después en el suelo. |
| ≈ 15 | **Lo enseñamos.** Cada grupo enseña su robot: ¿qué ha salido? ¿qué cambiaríamos? |

### Si un robot se mueve, ruedas en el aire primero

- [ ] Los programas nuevos se prueban siempre con el robot sobre un bote o un libro.
- [ ] Solo pasa al suelo con el visto bueno del monitor, con el sitio libre.
- [ ] En las lecciones 06 y 07, `moveMotors = false` hasta que el niño lo cambie a `true`.
- [ ] El monitor sabe qué lecciones mueven los motores: 03, 04, 06, 07, 09 y 10.
- [ ] El monitor tiene la terminal abierta para pulsar **Ctrl+C**.

## 5. Normas de seguridad (para dar a los niños)

- Primero, las ruedas en el aire. Después, en el suelo con el visto bueno del monitor.
- Con el robot encendido no se tocan los cables ni los pines. Para montar o cambiar un cable, el robot se apaga.
- En el potenciómetro, el cable negro no es GND: es la señal.
- Las baterías 18650 se ponen con la polaridad correcta y con el interruptor de la UPS en OFF.
- Si el robot hace algo raro: **Ctrl+C**. Si no para, `bash tools/parar-robot.sh` o apagar la UPS.
- El láser (módulo KY-008) solo lo usa el monitor, nunca apuntado a una persona.

## 6. Si algo falla

| Qué pasa | Qué mirar |
|---|---|
| La pantalla da `Error 5 (Input/output error)` | Nadie contesta en 0x27. `i2cdetect -y 1` debe mostrarlo. Mira la alimentación (VCC a 3v3), SDA y SCL (cambiados) o un cable suelto. |
| Una rueda no gira | Mira que el cable no esté suelto en la borna del motor. Prueba el motor con `probar-cableado.sh` y ve si el sensor de velocidad de esa rueda cuenta. |
| El robot no para | Ctrl+C. Si no, `bash tools/parar-robot.sh`. Si se cortó la wifi, ninguna de las dos llega a la Pi: **levanta el robot del suelo o apaga la UPS en el momento**. La Pi cierra la sesión y para los motores sola en unos 15–30 s. |
| El robot va más lento o se reinicia | `vcgencmd get_throttled` (`0x0` está bien). Mira las baterías de la UPS y el cable de alimentación. |
| El portátil no encuentra la Pi | Mira la red, la IP y `ssh <nombre> true`. Con `harlequin.local`, puede tardar varios segundos. |
| VS Code no para en los puntos de interrupción | La extensión de C# debe ser la 2.140.9 y C# Dev Kit debe estar deshabilitada. |
| La Pi tarda en responder | No compiles toda la solución en la Pi: compila en el portátil y despliega. |

## 7. Al terminar

- [ ] Parar todos los programas (`bash tools/parar-robot.sh`) y comprobar los pines en `lo`.
- [ ] Apagar cada robot: `sudo shutdown -h now`, esperar a que se apague el LED verde y poner la UPS en OFF.
- [ ] Poner a cargar las baterías y recoger cables, cajas y papeles.
- [ ] Anotar qué falló y qué entendieron peor los niños, para mejorar las lecciones.

## Antes de una demostración

Si el taller es una demostración para otras personas, añade a lo anterior:

- [ ] Ensayo completo en la red de la sala, con la misma secuencia que se va a enseñar.
- [ ] Un plan B sin red: fotos y un vídeo corto del robot.
- [ ] Portátil con cargador, fuentes instaladas y la presentación copiada en el disco, sin depender de internet.
- [ ] Suelo libre y cajas listas. El robot, cargado y con los pines en `lo`.
