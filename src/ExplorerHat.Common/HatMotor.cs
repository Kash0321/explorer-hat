using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using Iot.Device.DCMotor;

namespace ExplorerHat.Common
{
    /// <summary>
    /// Motor of the Explorer HAT driven like the Pimoroni Python library does: software PWM on both inputs of
    /// the DRV8833, so the motor coasts during the off part of the PWM cycle in both directions.
    /// </summary>
    /// <remarks>
    /// <c>DCMotor.Create(speedPin, directionPin)</c> of Iot.Device.Bindings 4.2.0 puts the PWM on one pin only.
    /// Backwards it sets the other pin high and inverts the PWM, so in the off part of every cycle both inputs
    /// are high and the DRV8833 brakes the motor (it shorts its terminals). Measured on the robot with the wheels
    /// in the air: backwards at 0.4 the wheels turned at ~40 % of the speed forwards. With the PWM on both pins
    /// both directions are the same.
    /// <para>
    /// Same direction as <c>DCMotor</c> in the binding: a positive speed puts the PWM on
    /// <paramref name="forwardPin"/> (19 or 21). The Pimoroni library calls those pins "backward", so with it
    /// the robot would drive the other way round.
    /// </para>
    /// </remarks>
    public class HatMotor : DCMotor
    {
        private SoftwarePwmChannel? _forward;
        private SoftwarePwmChannel? _backward;
        private double _speed;

        /// <summary>
        /// Initializes a <see cref="HatMotor"/> instance
        /// </summary>
        /// <param name="forwardPin">Pin with the PWM when the speed is positive</param>
        /// <param name="backwardPin">Pin with the PWM when the speed is negative</param>
        /// <param name="controller">Controller of both pins. The motor does not dispose it</param>
        /// <param name="frequency">PWM frequency in Hz (the Pimoroni library uses 100)</param>
        public HatMotor(int forwardPin, int backwardPin, GpioController controller, int frequency = 100)
            : base(controller, shouldDispose: false)
        {
            _forward = new SoftwarePwmChannel(forwardPin, frequency, 0.0, controller: controller, shouldDispose: false);
            _backward = new SoftwarePwmChannel(backwardPin, frequency, 0.0, controller: controller, shouldDispose: false);
            _forward.Start();
            _backward.Start();
        }

        /// <summary>
        /// Speed from -1.0 (full speed backwards) to 1.0 (full speed forwards). 0.0 is stopped.
        /// </summary>
        public override double Speed
        {
            get => _speed;
            set
            {
                if (_forward is null || _backward is null)
                {
                    throw new ObjectDisposedException(nameof(HatMotor));
                }

                double speed = Math.Clamp(value, -1.0, 1.0);

                // The pin of the other direction goes to 0 first, so both pins are never driven on purpose
                if (speed >= 0.0)
                {
                    _backward.DutyCycle = 0.0;
                    _forward.DutyCycle = speed;
                }
                else
                {
                    _forward.DutyCycle = 0.0;
                    _backward.DutyCycle = -speed;
                }

                _speed = speed;
            }
        }

        /// <summary>
        /// Stops the motor and ends both PWM threads, which leave their pins low
        /// </summary>
        public override void Dispose()
        {
            if (_forward is not null && _backward is not null)
            {
                _forward.DutyCycle = 0.0;
                _backward.DutyCycle = 0.0;
                _speed = 0.0;
            }

            _forward?.Dispose();
            _forward = null;
            _backward?.Dispose();
            _backward = null;
            base.Dispose();
        }
    }
}
