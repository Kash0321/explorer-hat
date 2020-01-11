using System;
using System.Timers;
using Iot.Device.Hcsr04;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Servicios de sonar
    /// </summary>
    public class SonarSingleton
    {
        const int TRIG = 6;
        const int ECHO = 23;

        /// <summary>
        /// Distancia hasta el obstáculo más cercano
        /// </summary>
        /// <value></value>
        public static double Distance { get; private set; }

        static Timer MeasurementTimer { get; set; }

        static Hcsr04 Sonar { get; set; } = null;

        static SonarSingleton()
        {
            InitializeResources();
        }

        public static void InitializeResources()
        {
            Log.Information("Inicializando SonarSingleton...");

            MeasurementTimer = new Timer(250);

            MeasurementTimer.Elapsed += MeasurementTimer_Elapsed;
            MeasurementTimer.AutoReset = true;
            MeasurementTimer.Enabled = true;

            Distance = 0;

            Sonar = new Hcsr04(TRIG, ECHO);

            Log.Information("SonarSingleton inicializado");
        }

        public static void DisposeResources()
        {
            MeasurementTimer.Stop();
            MeasurementTimer.Enabled = false;

            Sonar.Dispose();
            Sonar = null;
        }

        private static void MeasurementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Debug("Actualizando medición de distancia...");

            Distance = Sonar.Distance;

            Log.Information("Medición de distancia actualizada ({distance} cm.)", Math.Round(Distance, 4, MidpointRounding.AwayFromZero));
        }
    }
}