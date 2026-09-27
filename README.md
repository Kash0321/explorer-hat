# 🤖 Proyecto Robot Educativo - Explorer Hat & .NET 10

¡Bienvenidos al repositorio del robot educativo basado en **Raspberry Pi 3 Model B+** y el complemento **Pimoroni Explorer Hat Pro**! Este proyecto está diseñado para que los niños aprendan lógica de programación, electrónica básica y robótica utilizando **C# y .NET 10 (LTS)**.

> 💡 **Nota Histórica:** Este proyecto utiliza las librerías oficiales `System.Device.Gpio` e `Iot.Device.Bindings`, las cuales incluyen soporte nativo para el Explorer Hat gracias a las contribuciones directas realizadas en el repositorio oficial de Microsoft .NET IoT.

---

## 🛠️ Requisitos del Sistema (Hardware & Software)

* **Placa:** Raspberry Pi 3 Model B+ (ARM64 de 64 bits).
* **OS:** Raspberry Pi OS 64-bit (Debian Moderno).
* **Hardware IoT:** Pimoroni Explorer Hat Pro (Conexión I2C habilitada).
* **Entorno:** .NET SDK 10.0.401 instalado localmente en la Raspberry Pi.

---

## 📂 Estructura del Proyecto (`src/`)

El entorno está organizado en soluciones independientes según el nivel de aprendizaje:

* 🟢 **`ExplorerHat.BasicSample/`**: El laboratorio de inicio. Ideal para enseñar bucles, hilos con `Thread.Sleep`, encendido de luces LED secuenciales y movimientos básicos de los dos motores (Adelante, Atrás, Giros).
* ⚙️ **`ExplorerHat.Common/`**: Código compartido por los ejemplos. `SafeExplorerHat` se usa igual que `ExplorerHat`, pero al terminar para los motores, apaga las luces y libera los pines sin errores.
* 📡 **`ExplorerHat.SonarDashboard/`**: Panel en la consola con la distancia que mide cada sensor de ultrasonidos, sin mover los motores. Sirve para comprobar el montaje de los sensores.
* 🔵 **`ExplorerHat.ObstacleAvoidance/`**: El robot autónomo. Integra lecturas de sensores de distancia por ultrasonidos (HC-SR04) para calcular proximidad y tomar decisiones de esquiva en tiempo real.

---

## 🚀 Comandos Rápidos de Consola

Para compilar o ejecutar los proyectos directamente desde la terminal remota de la Raspberry Pi:

### Compilar la solución completa:
```bash
dotnet build ExplorerHatSandbox.slnx
```

### Ejecutar el laboratorio básico:
```bash
dotnet run --project src/ExplorerHat.BasicSample/ExplorerHat.BasicSample.csproj
```

### Ejecutar el robot autónomo:
```bash
dotnet run --project src/ExplorerHat.ObstacleAvoidance/ExplorerHat.ObstacleAvoidance.csproj
```
