using System;
using System.Timers;
using Iot.Device.Hcsr04;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Sonar services
    /// </summary>
    public class Sonar : IDisposable
    {
        const int TRIG = 6;
        const int ECHO = 23;

        private Timer MeasurementTimer { get; set; }

        private Hcsr04 SonarDevice { get; set; } = null;

        /// <summary>
        /// Distance measured
        /// </summary>
        public double Distance { get; private set; }

    /// <summary>
    /// Initializes a <see cref="Sonar"/> instance
    /// </summary>
        public Sonar()
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

        private void MeasurementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Debug("Updating distance measurement...");

            Distance = SonarDevice.Distance;

            Log.Debug("Distance measuremente updated ({distance} cm.)", Math.Round(Distance, 4, MidpointRounding.AwayFromZero));
        }

        #region IDisposable Support

        /// <summary>
        /// Disposes <see cref="Sonar"/> resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (SonarDevice != null)
            {
                if (disposing)
                {
                    MeasurementTimer.Stop();
                    MeasurementTimer.Enabled = false;

                    SonarDevice.Dispose();
                    SonarDevice = null;
                    Log.Debug("Sonar disposed");
                }
            }
        }

        /// <summary>
        /// Disposes <see cref="Sonar"/> resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}