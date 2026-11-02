using System.Device.Gpio;
using System.Diagnostics;

namespace ExplorerHat.Orders
{
    /// <summary>The numbers that turn pulses into centimeters and degrees: the same ones as lesson 10</summary>
    public static class Geometry
    {
        public const double WheelDiameter = 6.5;   // centimeters
        public const double WheelDistance = 13;    // centimeters between the middle of both wheels
        public const int Slots = 20;               // slots in each disc: pulses for each turn of the wheel

        /// <summary>What a wheel goes for each pulse: 1.02 cm</summary>
        public const double CmPerPulse = WheelDiameter * Math.PI / Slots;

        /// <summary>What each wheel goes when the robot turns once on the spot</summary>
        public const double CmPerRobotTurn = WheelDistance * Math.PI;

        /// <summary>What the robot turns for each pulse of each wheel (they go in opposite ways): 9 degrees</summary>
        public const double DegreesPerPulse = 360.0 * CmPerPulse / CmPerRobotTurn;
    }

    /// <summary>
    /// The settings of the movements by pulses (centimeters and degrees). The brake is how many pulses before the target
    /// each wheel stops, because the robot slides a bit after the motors stop. These values are calibrated on the floor and
    /// can be changed when the service starts (see Program.cs), so a calibration does not need a new build.
    /// </summary>
    public record PulseSettings(double Speed, int BrakeForwards, int BrakeBackwards, int BrakeTurn)
    {
        public static PulseSettings Default => new PulseSettings(0.6, 6, 4, 4);
    }

    /// <summary>Counts the pulses of the speed sensors of both wheels. A pulse is the start of a slot of the disc.</summary>
    public interface IWheelCounter
    {
        /// <summary>Starts to count from zero. It must be called before the motors start.</summary>
        void Start();

        /// <summary>Stops counting. When it returns, the counts do not change until the next <see cref="Start"/>.</summary>
        void Stop();

        int Right { get; }

        int Left { get; }
    }

    /// <summary>
    /// The two LM393 speed sensors of the robot: right wheel = GPIO 25 (IN4), left wheel = GPIO 18 (the PWM pin of the
    /// 3.3 V row). While it counts, a thread reads both pins every millisecond, like lesson 10, and counts the changes
    /// from Low to High. It does nothing the rest of the time.
    /// </summary>
    public class HatWheelCounter : IWheelCounter, IDisposable
    {
        private const int RightPin = 25;
        private const int LeftPin = 18;

        private readonly GpioController _gpio = new GpioController();
        private readonly ManualResetEventSlim _counting = new ManualResetEventSlim(false);
        private readonly object _sync = new object();
        private readonly Thread _thread;
        private volatile bool _disposed;
        private bool _restart;
        private int _right;
        private int _left;

        public HatWheelCounter()
        {
            _gpio.OpenPin(RightPin, PinMode.Input);
            _gpio.OpenPin(LeftPin, PinMode.Input);
            _thread = new Thread(Loop) { IsBackground = true, Name = "WheelCounter" };
            _thread.Start();
        }

        public int Right => Volatile.Read(ref _right);

        public int Left => Volatile.Read(ref _left);

        public void Start()
        {
            lock (_sync)
            {
                _right = 0;
                _left = 0;
                _restart = true;
                _counting.Set();
            }
        }

        public void Stop()
        {
            // The thread holds the same lock while it reads: when this returns, it is not reading any more
            lock (_sync)
            {
                _counting.Reset();
            }
        }

        private void Loop()
        {
            bool rightWasHigh = true;
            bool leftWasHigh = true;
            while (!_disposed)
            {
                if (!_counting.Wait(200))
                {
                    continue;
                }

                try
                {
                    lock (_sync)
                    {
                        if (!_counting.IsSet || _disposed)
                        {
                            continue;
                        }

                        if (_restart)
                        {
                            // As in the lessons: a sensor that starts High does not count a pulse
                            rightWasHigh = true;
                            leftWasHigh = true;
                            _restart = false;
                        }

                        bool right = _gpio.Read(RightPin) == PinValue.High;
                        bool left = _gpio.Read(LeftPin) == PinValue.High;
                        if (right && !rightWasHigh)
                        {
                            _right++;
                        }

                        if (left && !leftWasHigh)
                        {
                            _left++;
                        }

                        rightWasHigh = right;
                        leftWasHigh = left;
                    }
                }
                catch (Exception)
                {
                    // A failing read counts nothing: the movement ends by itself with "sin_pulsos"
                    Thread.Sleep(50);
                    continue;
                }

                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _counting.Set();
            _thread.Join(1000);
            _gpio.Dispose();
        }
    }

    /// <summary>
    /// Wheels for the tests on the PC: no pins. The pulses come from the speed that the motors were told (60 per second at
    /// speed 1.0) and, after a motor stops, the wheel counts a few pulses more (the slide). A test can stall a wheel
    /// (PUT /simulado/ruedas): it counts nothing, whatever its motor does.
    /// </summary>
    public class SimulatedWheelCounter : IWheelCounter, IDisposable
    {
        private const double PulsesPerSecondAtFullSpeed = 60;
        private const double SlidePulses = 3;
        private const double SlideRate = 40;   // pulses per second while the wheel slides

        private readonly IRobotBody _body;
        private readonly ManualResetEventSlim _counting = new ManualResetEventSlim(false);
        private readonly object _sync = new object();
        private readonly Thread _thread;
        private volatile bool _disposed;
        private volatile bool _rightStalled;
        private volatile bool _leftStalled;
        private bool _restart;
        private double _right;
        private double _left;

        public SimulatedWheelCounter(IRobotBody body)
        {
            _body = body;
            _thread = new Thread(Loop) { IsBackground = true, Name = "SimulatedWheels" };
            _thread.Start();
        }

        public int Right
        {
            get
            {
                lock (_sync)
                {
                    return (int)Math.Floor(_right);
                }
            }
        }

        public int Left
        {
            get
            {
                lock (_sync)
                {
                    return (int)Math.Floor(_left);
                }
            }
        }

        public void SetStalled(bool right, bool left)
        {
            _rightStalled = right;
            _leftStalled = left;
        }

        public void Start()
        {
            lock (_sync)
            {
                _right = 0;
                _left = 0;
                _restart = true;
                _counting.Set();
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                _counting.Reset();
            }
        }

        private void Loop()
        {
            double previousRight = 0;
            double previousLeft = 0;
            double slideRight = 0;
            double slideLeft = 0;
            var clock = Stopwatch.StartNew();
            long last = 0;
            while (!_disposed)
            {
                if (!_counting.Wait(200))
                {
                    continue;
                }

                Thread.Sleep(2);
                lock (_sync)
                {
                    if (!_counting.IsSet || _disposed)
                    {
                        continue;
                    }

                    long now = clock.ElapsedTicks;
                    double seconds = (now - last) / (double)Stopwatch.Frequency;
                    last = now;
                    if (_restart)
                    {
                        // The first step after a Start only starts the clock
                        _restart = false;
                        previousRight = _body.Right;
                        previousLeft = _body.Left;
                        slideRight = 0;
                        slideLeft = 0;
                        continue;
                    }

                    double right = _body.Right;
                    double left = _body.Left;
                    _right += Advance(right, ref previousRight, ref slideRight, _rightStalled, seconds);
                    _left += Advance(left, ref previousLeft, ref slideLeft, _leftStalled, seconds);
                }
            }
        }

        // The pulses that one wheel counts in this step
        private static double Advance(double speed, ref double previous, ref double slide, bool stalled, double seconds)
        {
            if (stalled)
            {
                previous = speed;
                slide = 0;
                return 0;
            }

            if (previous != 0 && speed == 0)
            {
                slide = SlidePulses;
            }

            previous = speed;
            double pulses = Math.Abs(speed) * PulsesPerSecondAtFullSpeed * seconds;
            double sliding = Math.Min(slide, SlideRate * seconds);
            slide -= sliding;
            return pulses + sliding;
        }

        public void Dispose()
        {
            _disposed = true;
            _counting.Set();
            _thread.Join(1000);
        }
    }
}
