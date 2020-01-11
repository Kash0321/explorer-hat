using System;
using System.Timers;
using Iot.Device.Hcsr04;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Sonar services
    /// </summary>
    public class Sonar
    {
        const int TRIG = 6;
        const int ECHO = 23;

        private static Timer MeasurementTimer { get; set; }

        private static Hcsr04 SonarDevice { get; set; } = null;

        static Sonar()
        {
            InitializeResources();
        }

        /// <summary>
        /// Distance measured
        /// </summary>
        public static double Distance { get; private set; }

        public static void InitializeResources()
        {
            Log.Debug("Initializing sonar hardware and services...");

            MeasurementTimer = new Timer(250);

            MeasurementTimer.Elapsed += MeasurementTimer_Elapsed;
            MeasurementTimer.AutoReset = true;
            MeasurementTimer.Enabled = true;

            Distance = 0;

            SonarDevice = new Hcsr04(TRIG, ECHO);

            Log.Debug("Sonar hardware and services initialized");
        }

        /// <summary>
        /// Allows to free up hardware resources
        /// </summary>
        public static void DisposeResources()
        {
            MeasurementTimer.Stop();
            MeasurementTimer.Enabled = false;

            SonarDevice.Dispose();
            SonarDevice = null;
        }

        private static void MeasurementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Debug("Updating distance measurement...");

            Distance = Sonar.Distance;

            Log.Debug("Distance measuremente updated ({distance} cm.)", Math.Round(Distance, 4, MidpointRounding.AwayFromZero));
        }
    }
}