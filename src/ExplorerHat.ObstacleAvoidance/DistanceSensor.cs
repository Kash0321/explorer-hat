using System;
using Iot.Device.Hcsr04;
using UnitsNet;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// One HC-SR04 sensor, with a simple filter for wrong readings.
    /// Sometimes the sensor misses the echo of the obstacle and gives a distance much farther than the real one.
    /// That is dangerous: the robot would think the way is free. So the <see cref="Distance"/> of the sensor
    /// is the nearest of its last two readings: a single wrong reading is ignored.
    /// </summary>
    public class DistanceSensor : IDisposable
    {
        // No echo means there is nothing in front of the sensor (it measures up to 400 cm)
        const double NO_ECHO_DISTANCE = 400d;

        private readonly object _lock = new object();
        private readonly Hcsr04 _device;

        private double? _lastReading = null;
        private double? _previousReading = null;
        private int _newReadings = 0;

        /// <summary>
        /// Sensor name (where it points to)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Nearest of the last two readings, in centimeters (0 when there are no readings yet)
        /// </summary>
        public double Distance
        {
            get
            {
                lock (_lock)
                {
                    if (_lastReading is null)
                    {
                        return 0d;
                    }

                    if (_previousReading is null)
                    {
                        return _lastReading.Value;
                    }

                    return Math.Min(_lastReading.Value, _previousReading.Value);
                }
            }
        }

        /// <summary>
        /// Readings done since the last call to <see cref="StartNewReadings"/>
        /// </summary>
        public int NewReadings
        {
            get
            {
                lock (_lock)
                {
                    return _newReadings;
                }
            }
        }

        /// <summary>
        /// Initializes a <see cref="DistanceSensor"/> instance
        /// </summary>
        public DistanceSensor(string name, int triggerPin, int echoPin)
        {
            Name = name;
            _device = new Hcsr04(triggerPin, echoPin);
        }

        /// <summary>
        /// Measures the distance once
        /// </summary>
        public void Measure()
        {
            double reading;

            if (_device.TryGetDistance(out Length length))
            {
                reading = Math.Round(length.Centimeters, 1);
            }
            else
            {
                reading = NO_ECHO_DISTANCE;
            }

            lock (_lock)
            {
                _previousReading = _lastReading;
                _lastReading = reading;
                _newReadings++;
            }

            Log.Debug("{name} sensor: read {reading} cm, distance {distance} cm", Name, reading, Distance);
        }

        /// <summary>
        /// Starts counting <see cref="NewReadings"/> from zero
        /// </summary>
        public void StartNewReadings()
        {
            lock (_lock)
            {
                _newReadings = 0;
            }
        }

        /// <summary>
        /// Releases the sensor pins
        /// </summary>
        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
