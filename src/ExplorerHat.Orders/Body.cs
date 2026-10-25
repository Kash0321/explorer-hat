using ExplorerHat.Common;

namespace ExplorerHat.Orders
{
    /// <summary>
    /// What the service needs from the robot: two wheels and four lights.
    /// Right wheel = motor One, left wheel = motor Two.
    /// </summary>
    public interface IRobotBody
    {
        /// <summary>Speed last commanded to the right wheel, from -1.0 to 1.0</summary>
        double Right { get; }

        /// <summary>Speed last commanded to the left wheel, from -1.0 to 1.0</summary>
        double Left { get; }

        void SetMotors(double right, double left);

        void SetLight(string color, bool on);

        bool IsLightOn(string color);
    }

    /// <summary>
    /// Robot that only remembers what it was told: for tests on the PC and on the Raspberry Pi without pins
    /// </summary>
    public class SimulatedBody : IRobotBody
    {
        private readonly Dictionary<string, bool> _lights = new() { ["azul"] = false, ["amarilla"] = false, ["roja"] = false, ["verde"] = false };

        public double Right { get; private set; }

        public double Left { get; private set; }

        public void SetMotors(double right, double left)
        {
            Right = right;
            Left = left;
        }

        public void SetLight(string color, bool on) => _lights[color] = on;

        public bool IsLightOn(string color) => _lights[color];
    }

    /// <summary>
    /// The real Explorer HAT. With <c>zeroSpeed</c> the pins always get speed 0.0 (the lights do work), but
    /// <see cref="Right"/> and <see cref="Left"/> still report what was commanded: it tests the service with the
    /// real pins and without moving the wheels.
    /// </summary>
    public class HatBody : IRobotBody
    {
        private readonly SafeExplorerHat _hat;
        private readonly bool _zeroSpeed;
        private readonly Dictionary<string, bool> _lights = new() { ["azul"] = false, ["amarilla"] = false, ["roja"] = false, ["verde"] = false };

        public HatBody(SafeExplorerHat hat, bool zeroSpeed)
        {
            _hat = hat;
            _zeroSpeed = zeroSpeed;
        }

        public double Right { get; private set; }

        public double Left { get; private set; }

        public void SetMotors(double right, double left)
        {
            Right = right;
            Left = left;
            _hat.Motors.One.Speed = _zeroSpeed ? 0.0 : right;
            _hat.Motors.Two.Speed = _zeroSpeed ? 0.0 : left;
        }

        public void SetLight(string color, bool on)
        {
            var led = color switch
            {
                "azul" => _hat.Lights.Blue,
                "amarilla" => _hat.Lights.Yellow,
                "roja" => _hat.Lights.Red,
                _ => _hat.Lights.Green,
            };

            if (on)
            {
                led.On();
            }
            else
            {
                led.Off();
            }

            _lights[color] = on;
        }

        public bool IsLightOn(string color) => _lights[color];
    }
}
