using System.Device.Gpio;

namespace ExplorerHat.Common
{
    /// <summary>
    /// The two motors of the Explorer HAT, with the same members as <c>Iot.Device.ExplorerHat.Motors</c>
    /// but driven with <see cref="HatMotor"/> (PWM on both pins).
    /// </summary>
    public class HatMotors : IDisposable
    {
        /// <summary>
        /// Motor 1: the right wheel of the robot (PWM on GPIO 19 forwards and on GPIO 20 backwards)
        /// </summary>
        public HatMotor One { get; }

        /// <summary>
        /// Motor 2: the left wheel of the robot (PWM on GPIO 21 forwards and on GPIO 26 backwards)
        /// </summary>
        public HatMotor Two { get; }

        /// <summary>
        /// Initializes a <see cref="HatMotors"/> instance
        /// </summary>
        /// <param name="controller">Controller of the motor pins. The motors do not dispose it</param>
        public HatMotors(GpioController controller)
        {
            One = new HatMotor(19, 20, controller);
            Two = new HatMotor(21, 26, controller);
        }

        /// <summary>
        /// Both motors turn forwards at the indicated speed
        /// </summary>
        public void Forwards(double speed = 1)
        {
            One.Speed = speed;
            Two.Speed = speed;
        }

        /// <summary>
        /// Both motors turn backwards at the indicated speed
        /// </summary>
        public void Backwards(double speed = 1)
        {
            One.Speed = -speed;
            Two.Speed = -speed;
        }

        /// <summary>
        /// Both motors stop
        /// </summary>
        public void Stop()
        {
            One.Speed = 0.0;
            Two.Speed = 0.0;
        }

        /// <summary>
        /// Stops both motors and ends their PWM threads
        /// </summary>
        public void Dispose()
        {
            One.Dispose();
            Two.Dispose();
        }
    }
}
