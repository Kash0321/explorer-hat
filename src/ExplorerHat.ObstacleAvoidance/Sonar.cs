using System;
using System.Timers;
using Iot.Device.Hcsr04;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Sonar services. Manages internally a <see cref="Hcsr04">HC-SR04 sonar device</see> to update <see cref="Distance"/> property asynchronously
    /// </summary>
    public class Sonar : IDisposable
    {
        const int TRIG = 6;
        const int ECHO = 23;

        private Timer MeasurementTimer { get; set; }

        private Hcsr04 SonarDevice { get; set; } = null;

        private double _distance;

        /// <summary>
        /// Distance measured by sonar
        /// </summary>
        public double Distance 
        { 
            get
            {
                if (SonarDevice is null)
                {
                    var exception = new Exception("Sonar hardware and services not initialized");
                    Log.Error(exception.Message);
                    throw exception;
                }

                return _distance;
            } 
            private set
            {
                _distance = value;
            }
        }

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
            if (!(SonarDevice is null))
            {
                Log.Debug("Updating distance measurement...");

                Distance = SonarDevice.Distance;

                Log.Debug("Distance measuremente updated ({distance} cm.)", Math.Round(Distance, 4, MidpointRounding.AwayFromZero));
            }
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