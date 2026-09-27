using Iot.Device.Hcsr04;
using UnitsNet;

namespace ExplorerHat.SonarDashboard
{
    /// <summary>
    /// HC-SR04 distance sensor wired to the Explorer HAT, with the history of its latest readings
    /// </summary>
    public class SonarSensor : IDisposable
    {
        const int HISTORY_SIZE = 20;

        private readonly Hcsr04 _device;
        private readonly Queue<bool> _history = new Queue<bool>();

        /// <summary>
        /// Sensor name (where it points to)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Explorer HAT output wired to the sensor TRIG pin (e.g. "OUT1")
        /// </summary>
        public string TriggerLabel { get; }

        /// <summary>
        /// GPIO pin of the Explorer HAT output wired to TRIG
        /// </summary>
        public int TriggerPin { get; }

        /// <summary>
        /// Explorer HAT input wired to the sensor ECHO pin (e.g. "IN1")
        /// </summary>
        public string EchoLabel { get; }

        /// <summary>
        /// GPIO pin of the Explorer HAT input wired to ECHO
        /// </summary>
        public int EchoPin { get; }

        /// <summary>
        /// Latest distance measured, in centimeters (null when there is none yet)
        /// </summary>
        public double? LastDistance { get; private set; }

        /// <summary>
        /// Whether the latest reading worked
        /// </summary>
        public bool LastReadingOk { get; private set; }

        /// <summary>
        /// Number of readings done so far (up to <see cref="HistorySize"/>)
        /// </summary>
        public int Readings => _history.Count;

        /// <summary>
        /// How many of the latest readings worked
        /// </summary>
        public int ReadingsOk => _history.Count(ok => ok);

        /// <summary>
        /// How many readings are kept to compute <see cref="ReadingsOk"/>
        /// </summary>
        public int HistorySize => HISTORY_SIZE;

        /// <summary>
        /// Initializes a <see cref="SonarSensor"/> instance
        /// </summary>
        public SonarSensor(string name, int triggerPin, string triggerLabel, int echoPin, string echoLabel)
        {
            Name = name;
            TriggerPin = triggerPin;
            TriggerLabel = triggerLabel;
            EchoPin = echoPin;
            EchoLabel = echoLabel;
            _device = new Hcsr04(triggerPin, echoPin);
        }

        /// <summary>
        /// Measures the distance once and saves the result
        /// </summary>
        public void Measure()
        {
            LastReadingOk = _device.TryGetDistance(out Length distance);

            if (LastReadingOk)
            {
                LastDistance = distance.Centimeters;
            }

            _history.Enqueue(LastReadingOk);
            if (_history.Count > HISTORY_SIZE)
            {
                _history.Dequeue();
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
