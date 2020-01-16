using System;
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
        const int CENTER_TRIG = 6;
        const int LEFT_TRIG = 12;
        const int RIGHT_TRIG = 13;
        const int ECHO = 23;

        private Timer MeasurementTimer { get; set; }

        private Hcsr04 CenterSonarDevice { get; set; } = null;
        private Hcsr04 LeftSonarDevice { get; set; } = null;
        private Hcsr04 RightSonarDevice { get; set; } = null;

        private double _centerDistance;

        /// <summary>
        /// Distance measured by sonar, using center sensor
        /// </summary>
        public double CenterDistance 
        { 
            get
            {
                if (CenterSonarDevice is null)
                {
                    var exception = new Exception("Sonar hardware and services not initialized");
                    Log.Error(exception.Message);
                    throw exception;
                }

                return _centerDistance;
            } 
            private set
            {
                _centerDistance = value;
            }
        }

        private double _leftDistance;

        /// <summary>
        /// Distance measured by sonar, using left sensor
        /// </summary>
        public double LeftDistance 
        { 
            get
            {
                if (LeftSonarDevice is null)
                {
                    var exception = new Exception("Sonar hardware and services not initialized");
                    Log.Error(exception.Message);
                    throw exception;
                }

                return _leftDistance;
            } 
            private set
            {
                _leftDistance = value;
            }
        }

        private double _rightDistance;

        /// <summary>
        /// Distance measured by sonar, using right sensor
        /// </summary>
        public double RightDistance 
        { 
            get
            {
                if (RightSonarDevice is null)
                {
                    var exception = new Exception("Sonar hardware and services not initialized");
                    Log.Error(exception.Message);
                    throw exception;
                }

                return _rightDistance;
            } 
            private set
            {
                _rightDistance = value;
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

            CenterDistance = 0;
            LeftDistance = 0;
            RightDistance = 0;

            CenterSonarDevice = new Hcsr04(CENTER_TRIG, ECHO);
            LeftSonarDevice = new Hcsr04(LEFT_TRIG, ECHO);
            RightSonarDevice = new Hcsr04(RIGHT_TRIG, ECHO);

            Log.Debug("Sonar hardware and services initialized");
        }

        private int _device = 0;
        private void MeasurementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!(CenterSonarDevice is null))
            {
                Log.Debug("Updating distance measurement...");

                if (_device == 0)
                {
                    CenterDistance = CenterSonarDevice.Distance;
                    Log.Debug("Center distance measuremente updated ({distance} cm.)", Math.Round(CenterDistance, 4, MidpointRounding.AwayFromZero));
                }
                if (_device == 1)
                {
                    LeftDistance = LeftSonarDevice.Distance;
                    Log.Debug("Left distance measuremente updated ({distance} cm.)", Math.Round(LeftDistance, 4, MidpointRounding.AwayFromZero));
                }
                if (_device == 2)
                {
                    RightDistance = RightSonarDevice.Distance;
                    Log.Debug("Right distance measuremente updated ({distance} cm.)", Math.Round(RightDistance, 4, MidpointRounding.AwayFromZero));
                }

                if (_device < 2)
                {
                    _device++;
                }
                else
                {
                    _device = 0;
                }
            }
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