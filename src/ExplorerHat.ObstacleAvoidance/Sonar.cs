using System;
using System.Diagnostics;
using System.Threading;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Sonar services. Measures with three <see cref="DistanceSensor">HC-SR04 sensors</see>, one after the other,
    /// in a background thread, and keeps <see cref="Distance"/> updated
    /// </summary>
    public class Sonar : IDisposable
    {
        // Wait between two sensors (in milliseconds), so the echoes of one sensor don't reach the next one
        const int ECHO_FADE_TIME = 60;
        // Longest wait for new readings (in milliseconds), in case a sensor doesn't answer
        const int MAX_WAIT_TIME = 2000;

        private readonly DistanceSensor _leftSensor;
        private readonly DistanceSensor _centerSensor;
        private readonly DistanceSensor _rightSensor;
        private readonly Thread _measurementThread;
        private volatile bool _running;
        private bool _disposed;

        /// <summary>
        /// Latest distances of the three sensors
        /// </summary>
        public DistanceTuple Distance { get; private set; }

        /// <summary>
        /// Initializes a <see cref="Sonar"/> instance and starts measuring
        /// </summary>
        public Sonar()
        {
            Log.Debug("Initializing sonar hardware and services...");

            Distance = new DistanceTuple(0, 0, 0);

            _leftSensor = new DistanceSensor("Left", triggerPin: 13, echoPin: 24);
            _centerSensor = new DistanceSensor("Center", triggerPin: 6, echoPin: 23);
            _rightSensor = new DistanceSensor("Right", triggerPin: 12, echoPin: 22);

            _running = true;
            _measurementThread = new Thread(MeasureAll);
            _measurementThread.IsBackground = true;
            _measurementThread.Start();

            Log.Debug("Sonar hardware and services initialized");
        }

        /// <summary>
        /// Measures with each sensor in turn, until the sonar is disposed
        /// </summary>
        private void MeasureAll()
        {
            while (_running)
            {
                MeasureWith(_centerSensor);
                MeasureWith(_leftSensor);
                MeasureWith(_rightSensor);
            }
        }

        private void MeasureWith(DistanceSensor sensor)
        {
            try
            {
                sensor.Measure();
                Distance = new DistanceTuple(_leftSensor.Distance, _centerSensor.Distance, _rightSensor.Distance);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            Thread.Sleep(ECHO_FADE_TIME);
        }

        /// <summary>
        /// Waits until every sensor has two new readings, so <see cref="Distance"/> only uses readings
        /// taken after this call (for example, after a maneuver). Waits two seconds at most.
        /// </summary>
        public void WaitForNewReadings()
        {
            _leftSensor.StartNewReadings();
            _centerSensor.StartNewReadings();
            _rightSensor.StartNewReadings();

            var stopwatch = Stopwatch.StartNew();
            while (_running && stopwatch.ElapsedMilliseconds < MAX_WAIT_TIME &&
                (_leftSensor.NewReadings < 2 || _centerSensor.NewReadings < 2 || _rightSensor.NewReadings < 2))
            {
                Thread.Sleep(20);
            }
        }

        #region IDisposable Support

        /// <summary>
        /// Disposes <see cref="Sonar"/> resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Stop measuring before releasing the sensor pins
                _running = false;
                _measurementThread.Join();

                _leftSensor.Dispose();
                _centerSensor.Dispose();
                _rightSensor.Dispose();
                _disposed = true;
                Log.Debug("Sonar disposed");
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
