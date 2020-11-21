using System;
using System.Threading;
using System.Timers;
using Iot.Device.Hcsr04;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Sonar services. Manages internally a group of <see cref="Hcsr04">HC-SR04 sonar devices</see> 
    /// to update <see cref="CenterDistance"/>, <see cref="LeftDistance"/> and <see cref="RightDistance"/>
    /// properties asynchronously
    /// </summary>, >
    public class Sonar : IDisposable
    {
        private static object _lock = new object();

        const int CENTER_TRIG = 6;
        const int LEFT_TRIG = 13;
        const int RIGHT_TRIG = 12;
        const int CENTER_ECHO = 23;
        const int LEFT_ECHO = 24;
        const int RIGHT_ECHO = 22;

        private System.Timers.Timer MeasurementTimer { get; set; }

        private Hcsr04 CenterSonarDevice { get; set; } = null;
        private Hcsr04 LeftSonarDevice { get; set; } = null;
        private Hcsr04 RightSonarDevice { get; set; } = null;

        public DistanceTuple Distance { get; private set; }

        /// <summary>
        /// Initializes a <see cref="Sonar"/> instance
        /// </summary>
        public Sonar()
        {
            Log.Debug("Initializing sonar hardware and services...");

            Distance = new DistanceTuple(0, 0, 0);

            CenterSonarDevice = new Hcsr04(CENTER_TRIG, CENTER_ECHO);
            LeftSonarDevice = new Hcsr04(LEFT_TRIG, LEFT_ECHO);
            RightSonarDevice = new Hcsr04(RIGHT_TRIG, RIGHT_ECHO);

            Log.Debug("Sonar hardware and services initialized");
            
            MeasurementTimer = new System.Timers.Timer(250);
            MeasurementTimer.Elapsed += MeasurementTimer_Elapsed;
            MeasurementTimer.AutoReset = true;
            MeasurementTimer.Enabled = true;
        }

        private void MeasurementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // lock (_lock)
            // {
                if (!(CenterSonarDevice is null))
                {
                    try
                    {
                        Log.Debug($"Updating distance measurements...");

                        var centerDistance = CenterSonarDevice.Distance.Centimeters;
                        Log.Debug("Center distance measuremente updated ({distance} cm.)", Math.Round(centerDistance, 4, MidpointRounding.AwayFromZero));
                        Thread.Sleep(60);

                        var leftDistance = LeftSonarDevice.Distance.Centimeters;
                        Log.Debug("Left distance measuremente updated ({distance} cm.)", Math.Round(leftDistance, 4, MidpointRounding.AwayFromZero));
                        Thread.Sleep(60);

                        var rightDistance = RightSonarDevice.Distance.Centimeters;
                        Log.Debug("Right distance measuremente updated ({distance} cm.)", Math.Round(rightDistance, 4, MidpointRounding.AwayFromZero));
                        Thread.Sleep(60);

                        Distance = new DistanceTuple(leftDistance, centerDistance, rightDistance);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex.Message);
                    }

                }
            // }
        }

        #region IDisposable Support

        /// <summary>
        /// Disposes <see cref="Sonar"/> resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (CenterSonarDevice != null)
            {
                if (disposing)
                {
                    MeasurementTimer.Stop();
                    MeasurementTimer.Enabled = false;

                    CenterSonarDevice.Dispose();
                    CenterSonarDevice = null;
                    LeftSonarDevice.Dispose();
                    LeftSonarDevice = null;
                    RightSonarDevice.Dispose();
                    RightSonarDevice = null;
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