using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;
using System.Device.Gpio.Drivers;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// HC-SR04 - Ultrasonic Ranging Module
    /// </summary>
    public class Hcsr04Sonar : IDisposable
    {
        private readonly int _echo;
        private readonly int _trigger;
        private GpioController _controller;
        private Stopwatch _timer = new Stopwatch();

        private int _lastMeasurment = 0;

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        public double Distance => GetDistance();

        /// <summary>
        /// Creates a new instance of the HC-SCR04 sonar.
        /// </summary>
        /// <param name="triggerPin">Trigger pulse input.</param>
        /// <param name="echoPin">Trigger pulse output.</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        public Hcsr04Sonar(int triggerPin, int echoPin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
        {
            _echo = echoPin;
            _trigger = triggerPin;
            _controller = new GpioController(pinNumberingScheme, new LibGpiodDriver());

            _controller.OpenPin(_echo, PinMode.Input);
            _controller.OpenPin(_trigger, PinMode.Output);
            
            _controller.Write(_trigger, PinValue.Low);
        }

        /// <summary>
        /// Gets the current distance in cm.
        /// </summary>
        private double GetDistance()
        {
            _timer.Reset();

            // Measurements should be 60ms apart, in order to prevent trigger signal mixing with echo signal
            // ref https://components101.com/sites/default/files/component_datasheet/HCSR04%20Datasheet.pdf
            while (Environment.TickCount - _lastMeasurment < 60)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Environment.TickCount - _lastMeasurment));
            }

            // Trigger input for 10uS to start ranging
            _controller.Write(_trigger, PinValue.High);
            var waitTime = TimeSpan.FromTicks(10);
            Thread.Sleep(waitTime);
            _controller.Write(_trigger, PinValue.Low);

            // Wait until the echo pin is HIGH (that marks the beginning of the pulse length we want to measure)
            while (_controller.Read(_echo) == PinValue.Low)
            {
            }

            _lastMeasurment = Environment.TickCount;

            _timer.Start();

            // Wait until the pin is LOW again, (that marks the end of the pulse we are measuring)
            while (_controller.Read(_echo) == PinValue.High)
            {
            }

            _timer.Stop();

            TimeSpan elapsed = _timer.Elapsed;

            // distance = (time / 2) × velocity of sound (34300 cm/s)
            return elapsed.TotalMilliseconds / 2.0 * 34.3;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }
    }
}
