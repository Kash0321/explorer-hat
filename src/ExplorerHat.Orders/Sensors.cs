using Iot.Device.Hcsr04;
using UnitsNet;

namespace ExplorerHat.Orders
{
    /// <summary>Distances in centimeters. 400 means that a sensor sees nothing in front</summary>
    public record Distances(double Left, double Center, double Right);

    public interface IDistanceSensors
    {
        /// <summary>One reading of the three sensors, or null when there is no data. It can take some time.</summary>
        Distances? Read();
    }

    /// <summary>Sensors that the tests set by hand (PUT /simulado/sensores)</summary>
    public class SimulatedSensors : IDistanceSensors
    {
        private volatile Distances? _distances = new Distances(400, 400, 400);

        public void Set(Distances? distances) => _distances = distances;

        public Distances? Read() => _distances;
    }

    /// <summary>The three HC-SR04 of the robot, read one after the other like in lesson 09</summary>
    public class HatSensors : IDistanceSensors, IDisposable
    {
        private const int EchoTime = 60;  // milliseconds to let the echoes go away before the next reading

        private readonly Hcsr04 _left = new Hcsr04(triggerPin: 13, echoPin: 24);
        private readonly Hcsr04 _center = new Hcsr04(triggerPin: 6, echoPin: 23);
        private readonly Hcsr04 _right = new Hcsr04(triggerPin: 12, echoPin: 22);

        public Distances? Read()
        {
            double center = Measure(_center);
            double left = Measure(_left);
            double right = Measure(_right);
            return new Distances(left, center, right);
        }

        // No echo means there is nothing in front (400 cm), like in lesson 09. A sensor that is unplugged
        // gives no echo either, so it looks like a free way: the same limit as the lesson.
        private static double Measure(Hcsr04 sensor)
        {
            double reading = 400;
            if (sensor.TryGetDistance(out Length length))
            {
                reading = Math.Round(length.Centimeters);
            }

            Thread.Sleep(EchoTime);
            return reading;
        }

        public void Dispose()
        {
            _left.Dispose();
            _center.Dispose();
            _right.Dispose();
        }
    }

    /// <summary>What the obstacle layer sees right now</summary>
    public enum WayState
    {
        /// <summary>No fresh reading: the robot must not go forwards</summary>
        NoData,
        Clear,
        Obstacle,
    }

    /// <summary>
    /// Reads the sensors in a thread of its own and keeps the nearest of the last two readings of each one
    /// (the filter of lesson 06). The movements ask it for the state of the way.
    /// </summary>
    public class ObstacleMonitor : IDisposable
    {
        /// <summary>Nearer than this (centimeters) is an obstacle</summary>
        public const double ObstacleDistance = 30;

        // A reading older than this (milliseconds) is not data any more
        private const long MaxAge = 1000;

        private record Snapshot(Distances Filtered, long Time);

        private readonly IDistanceSensors _sensors;
        private readonly Thread _thread;
        private volatile bool _stop;
        private volatile Snapshot? _latest;

        public ObstacleMonitor(IDistanceSensors sensors)
        {
            _sensors = sensors;
            _thread = new Thread(Loop) { IsBackground = true, Name = "ObstacleMonitor" };
            _thread.Start();
        }

        /// <summary>The distances after the filter, or null without fresh data</summary>
        public Distances? Latest
        {
            get
            {
                var snapshot = _latest;
                return snapshot is not null && Environment.TickCount64 - snapshot.Time < MaxAge ? snapshot.Filtered : null;
            }
        }

        public WayState Way
        {
            get
            {
                var distances = Latest;
                if (distances is null)
                {
                    return WayState.NoData;
                }

                bool obstacle = distances.Left < ObstacleDistance || distances.Center < ObstacleDistance || distances.Right < ObstacleDistance;
                return obstacle ? WayState.Obstacle : WayState.Clear;
            }
        }

        private void Loop()
        {
            Distances? last = null;
            while (!_stop)
            {
                try
                {
                    var current = _sensors.Read();
                    if (current is null)
                    {
                        _latest = null;
                        last = null;
                    }
                    else
                    {
                        var filtered = last is null
                            ? current
                            : new Distances(Math.Min(current.Left, last.Left), Math.Min(current.Center, last.Center), Math.Min(current.Right, last.Right));
                        _latest = new Snapshot(filtered, Environment.TickCount64);
                        last = current;
                    }

                    Thread.Sleep(30);
                }
                catch (Exception)
                {
                    // A failing sensor is a way without data: the robot does not go forwards until it works again
                    _latest = null;
                    last = null;
                    Thread.Sleep(500);
                }
            }
        }

        public void Dispose()
        {
            _stop = true;
            _thread.Join(2000);
        }
    }
}
