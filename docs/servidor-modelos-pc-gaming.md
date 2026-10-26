# Servidor de modelos en un PC de gaming (Fase 7)

Cómo montar un PC de gaming con una **RTX 3060 Ti (8 GB de VRAM)** como servidor de Ollama para los portátiles del taller.
Estado: **montado y medido el 26/10/2026** (ver "Registro del montaje" y los resultados en `PLAN.md`, Fase 7). Las variables
de Ollama y el acceso desde otro equipo funcionaron, y el cortafuegos quedó limitado a la red local tras corregirlo (ver
"Revisar el cortafuegos" y el registro). **Sin comprobar:** el identificador de `winget` y los tamaños de `qwen3:8b` y
`qwen2.5:14b`. Si algo falla, anota el error en el apartado "Registro del montaje" y se corrige este documento.

Decisiones del usuario (26/10/2026): el PC **va al taller**, en la misma red local que los portátiles y las Pi; usa
**Windows**; trabajan **entre 4 y 6 portátiles y robots a la vez**.

## Para qué sirve

* El portátil de pruebas (RTX 3050 Ti, 4 GB) no deja `qwen2.5:7b` entero en la tarjeta: va un 45 % en la tarjeta y tarda 3 s
  de mediana. Con 8 GB, el 7B (unos 4,7 GB) **cabe entero**: **medido el 26/10/2026, 0,44 s de mediana con un cliente y 0,97 s
  con seis a la vez** (tabla en `PLAN.md`, Fase 7).
* Un modelo de 14B (unos 9 GB) no cabe entero: parte va por la CPU y será más lento.
* Un modelo mayor **no arregla** los valores fuera de rango (todos los modelos pasan 300 y 10 segundos): el recorte de la Pi
  sigue siendo obligatorio.

```text
Pi (robot)  <--HTTP--  cerebro en cada portátil (C#)  --HTTP-->  Ollama en el PC de gaming (RTX 3060 Ti)
```

En el cerebro solo cambia la URL base. La Pi y el servicio de órdenes no cambian.

## Antes de empezar

* **El PC, por cable de red** al router del taller si es posible (la wifi añade retraso y cortes).
* **Aviso de seguridad:** Ollama **no tiene autenticación**. Quien llegue al puerto puede usar el modelo y también bajar o
  borrar modelos (`/api/pull`, `/api/delete`). Por eso el paso 4 limita el acceso a la red del taller, y **el puerto 11434
  no se abre nunca a internet** (ni se redirige en el router).
* **Privacidad:** el tráfico va en HTTP sin cifrar, dentro de la red local. Las órdenes de los niños no salen de ella.
* **Mientras dure el taller, nadie juega en el PC:** el juego y el modelo comparten la VRAM.

## Pasos en el PC de gaming

Todos los comandos son de **PowerShell**. Los pasos 4 y 5 piden **administrador** (clic derecho en PowerShell, *Ejecutar como
administrador*).

### 1. Instalar Ollama y actualizar el controlador

```powershell
winget install Ollama.Ollama
nvidia-smi
```

`nvidia-smi` debe mostrar la RTX 3060 Ti y la versión del controlador. Si falla, actualiza el controlador de NVIDIA.

### 2. Configurar Ollama (variables de usuario)

```powershell
setx OLLAMA_HOST "0.0.0.0:11434"
setx OLLAMA_KEEP_ALIVE "-1"
setx OLLAMA_NUM_PARALLEL "6"
setx OLLAMA_MAX_LOADED_MODELS "1"
```

| Variable | Qué hace |
|---|---|
| `OLLAMA_HOST` | Escucha en toda la red, no solo en `localhost` |
| `OLLAMA_KEEP_ALIVE` | `-1`: el modelo se queda cargado en la tarjeta (sin la carga de 3 a 5 s en la primera orden) |
| `OLLAMA_NUM_PARALLEL` | Peticiones que atiende a la vez. Seis, para 4 a 6 portátiles. El contexto de cada una ocupa VRAM |
| `OLLAMA_MAX_LOADED_MODELS` | Un solo modelo cargado a la vez |

### 3. Reiniciar Ollama

1. Cierra Ollama desde el icono de la bandeja (junto al reloj): clic derecho, *Quit Ollama*.
2. Ábrelo otra vez desde el menú Inicio. **Las variables nuevas solo valen en un Ollama arrancado después de `setx`.**
3. Comprueba que escucha en toda la red:

```powershell
netstat -ano | findstr :11434
```

Debe aparecer `0.0.0.0:11434` en estado `LISTENING`. Si aparece `127.0.0.1:11434`, Ollama no leyó la variable: repite el paso 3.

### 4. Abrir el cortafuegos solo para la red del taller (administrador)

1. Mira el nombre y la categoría de tu red: `Get-NetConnectionProfile`. Debe ser **Private**. Si dice `Public`, la regla de
   abajo no vale (`-Profile Private`): cámbiala con `Set-NetConnectionProfile -InterfaceAlias "<nombre>" -NetworkCategory Private`.
2. **Desactiva las reglas automáticas de Windows.** Al arrancar Ollama por primera vez, Windows pregunta si lo deja pasar y,
   si aceptas, crea **dos reglas `ollama.exe`** (el 26/10/2026 existían: `Perfil` = `Private`, `Remota` = `Any`, `Puerto` =
   `Any`). Dejan pasar a **cualquier equipo** de la red. Desactívalas (no las borres: así se pueden volver a activar):

```powershell
Disable-NetFirewallRule -DisplayName "ollama.exe"
```

3. Crea la regla propia. **`LocalSubnet`** quiere decir "la red local a la que está conectado el PC", así que vale en cualquier
   taller sin cambiar nada. (Con un rango fijo, como `-RemoteAddress 192.168.0.0/24`, hay que cambiarlo en cada red.)

```powershell
New-NetFirewallRule -DisplayName "Ollama taller" -Direction Inbound -Protocol TCP -LocalPort 11434 -Action Allow -Profile Private -RemoteAddress LocalSubnet
```

4. Comprueba desde otro equipo de la red que sigue respondiendo (`Invoke-RestMethod http://<ip>:11434/api/version`). Si no,
   reactiva las reglas con `Enable-NetFirewallRule -DisplayName "ollama.exe"` y revisa la categoría de la red (punto 1).

#### Revisar el cortafuegos (sin administrador)

Un puerto abierto no prueba que la regla esté limitada a la red del taller (el 26/10/2026 no lo estaba: ver el paso 4).
Repite esta revisión **después de actualizar Ollama**, porque Windows puede crear otra vez las reglas automáticas.
**Busca por puerto y por programa, no por el nombre de la regla** (el 26/10/2026, una búsqueda por nombre no encontró una
regla propia que se llamaba `Liliput`). Ejecuta esto y revisa el resultado. **Recorre todas las reglas: tarda de unos
segundos a varios minutos**, según cuántas haya (en un portátil de empresa, más de 2 minutos):

```powershell
Get-NetFirewallRule -Direction Inbound -Enabled True -Action Allow | ForEach-Object { $p = $_ | Get-NetFirewallPortFilter; $a = $_ | Get-NetFirewallApplicationFilter; if (@($p.LocalPort) -contains '11434' -or $a.Program -like '*ollama*') { [pscustomobject]@{ Nombre=$_.DisplayName; Perfil=$_.Profile; Remota=($_ | Get-NetFirewallAddressFilter).RemoteAddress; Protocolo=$p.Protocol; Puerto=$p.LocalPort; Programa=$a.Program } } }
```

Lo esperado es **solo** la regla `Ollama taller`, con `Perfil` = `Private`, `Protocolo` = `TCP`, `Puerto` = `11434` y `Remota` =
`LocalSubnet` (o el rango del taller). Cualquier otra regla con `Remota` = `Any` o `Perfil` = `Public` o `Any` hay que
desactivarla (con administrador: `Disable-NetFirewallRule -DisplayName "<nombre>"`).

### 5. Fijar la IP del PC

1. Averigua la IP actual: `Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike '169.*' -and $_.IPAddress -ne '127.0.0.1' }`.
2. En el router del taller, **reserva esa IP** para la dirección MAC del PC (reserva DHCP), para que no cambie entre
   talleres. Si no puedes tocar el router, anota la IP cada vez.

### 6. Descargar los modelos

```powershell
ollama pull qwen2.5:7b
ollama pull qwen2.5:3b
```

Candidatos para medir después (tamaños aproximados, sin comprobar): `qwen3:8b` (unos 5 GB; con `--sin-pensar`) y
`qwen2.5:14b` (unos 9 GB; parte por la CPU).

### 7. Comprobar que usa la tarjeta gráfica

```powershell
ollama run qwen2.5:7b "Hola"
ollama ps
```

La columna `PROCESSOR` de `ollama ps` debe decir **`100% GPU`**. Si dice otra cosa (por ejemplo, `20%/80% CPU/GPU`), el modelo
no cabe entero: anota el valor en el registro. Con seis peticiones a la vez, mira también la memoria:

```powershell
nvidia-smi --query-gpu=memory.used,memory.total --format=csv
```

### 8. Que el PC no se duerma

```powershell
powercfg /change standby-timeout-ac 0
```

## Comprobar desde otro equipo

Desde **un portátil** de la misma red (no desde el propio PC de gaming), con la IP del paso 5:

```powershell
Test-NetConnection <ip> -Port 11434
Invoke-RestMethod http://<ip>:11434/api/version
```

`TcpTestSucceeded` debe ser `True` y la segunda orden debe devolver la versión de Ollama.

| Síntoma | Causa probable |
|---|---|
| `TcpTestSucceeded : False` | El cortafuegos (paso 4: red en `Public` o rango equivocado) o Ollama escucha solo en `127.0.0.1` (paso 3) |
| Funciona en el PC con `localhost`, no desde el portátil | Lo mismo que arriba |
| Funciona, pero `ollama ps` no dice `100% GPU` | Controlador de NVIDIA antiguo o el modelo no cabe: anota el reparto |
| Tarda 3 a 5 s la primera orden | El modelo se descargó de la memoria: falta `OLLAMA_KEEP_ALIVE` o no se reinició Ollama |

## Medir (desde el portátil, con la IP del PC)

La herramienta es [`tools/medir-llm`](../tools/medir-llm/README.md). Antes de cada medida, descarga el modelo de la memoria
en el PC (`ollama stop qwen2.5:7b`) para medir también la carga en frío.

```powershell
# 1 cliente
python tools/medir-llm/medir.py --modelo qwen2.5:7b --equipo gaming3060ti --url http://<ip>:11434
# 6 clientes a la vez (la peor carga de un taller)
python tools/medir-llm/medir.py --modelo qwen2.5:7b --equipo gaming3060ti --url http://<ip>:11434 --paralelo 6
```

Repite las dos con `qwen2.5:3b`. Compara la **mediana y el máximo** de tiempo con 1 y con 6 clientes (con 6 habrá cola: lo
que importa es cuánto sube el máximo). El mensaje de sistema, los límites y las 24 órdenes son los mismos que en las medidas
del portátil, así que las tablas de `PLAN.md` (Fase 7) se pueden comparar.

Si con 6 clientes el 7B tarda demasiado, hay dos salidas: usar `qwen2.5:3b`, o bajar `OLLAMA_NUM_PARALLEL`.

## Qué falta decidir al escribir el cerebro

* **Respaldo:** una lista de URLs, primero el PC de gaming y después el `qwen2.5:3b` del propio portátil. Si el PC o la red
  caen, el taller sigue.
* **Opción estricta, sin probar:** Ollama solo en `127.0.0.1` y delante un proxy inverso con clave (por ejemplo, Caddy, que
  comprueba la cabecera `Authorization`). Solo si el cortafuegos por rango no basta.
* **El PC en casa y los portátiles por VPN** (por ejemplo, Tailscale): descartado por el usuario (opción B). Cruzaría internet,
  y la regla de privacidad de la Fase 7 ("el LLM, en local") exige revisarlo con la organización del taller.

## Deshacerlo

```powershell
Remove-NetFirewallRule -DisplayName "Ollama taller"
[Environment]::SetEnvironmentVariable("OLLAMA_HOST", $null, "User")
```

(Igual con las otras tres variables.) Después, reinicia Ollama.

## Registro del montaje

Anota aquí lo que pase al montarlo (fecha, comando, resultado, errores) y copia los resultados de las medidas a `PLAN.md`.

| Fecha | Paso | Resultado |
|---|---|---|
| 26/10/2026 | El usuario configura Ollama en el PC (según su mensaje: Ollama configurado; **no dijo si creó la regla del cortafuegos: sin comprobar**) y descarga `qwen2.5:3b` y `qwen2.5:7b` | `ollama list` muestra los dos. `ollama ps`: `qwen2.5:7b`, 5,9 GB, `100% GPU`, contexto 4096, `UNTIL` = `Forever`, ejecutor `llamacpp` |
| 26/10/2026 | Comprobación desde el portátil (IP del día: 192.168.0.241, **sin reservar**) | `Test-NetConnection` al puerto 11434: `True`. `/api/version`: 0.40.11. `/api/tags` y `/api/ps` correctos. El ping no responde (Windows bloquea el ICMP: es normal) |
| 26/10/2026 | `medir.py` con 1 y con 6 clientes (`--paralelo 6`), 7B y 3B | Sin errores. Resultados en `PLAN.md` (Fase 7) y en `tools/medir-llm/resultados/gaming3060ti_*.json`. 7B: 0,44 s de mediana con 1 cliente y 0,97 s con 6 |
| 26/10/2026 | Revisión de las reglas del cortafuegos, primero por nombre (`*ollama*`) | Salieron solo dos reglas automáticas `ollama.exe`: `Perfil` = `Private`, `Remota` = `Any`, `Puerto` = `Any`. Dejan pasar a **cualquier equipo** de una red marcada como Privada. La búsqueda por nombre no encontró la regla propia, porque el usuario la había llamado `Liliput` |
| 26/10/2026 | Revisión por puerto (11434) | Existe la regla propia, **bien hecha**, llamada `Liliput`: `Private`, TCP, puerto 11434, `Remota` = `192.168.0.0/255.255.255.0`. Pendiente: renombrarla a `Ollama taller` (`Set-NetFirewallRule -DisplayName "Liliput" -NewDisplayName "Ollama taller"`), desactivar las dos `ollama.exe` y, si se quiere que valga en cualquier red, cambiar su rango a `LocalSubnet` |
| 26/10/2026 | El usuario renombra `Liliput` a `Ollama taller`, le pone `Remota` = `LocalSubnet` y desactiva las dos reglas `ollama.exe` (con administrador) | Revisión por puerto y por programa: sale **solo** `Ollama taller` (`Private`, TCP, 11434, `LocalSubnet`). Desde el portátil, el puerto responde (`TcpTestSucceeded` = `True`) y `/api/version` da 0.40.11 |
| | | |
