using System;
using System.Collections.Generic;

namespace Iot.Device.ExplorerHat.Motorization
{
    /// <summary>
    /// Represents the Explorer HAT DCMotors collection
    /// </summary>
    public class Motors : IDisposable
    {
        const int MOTOR1_SPDPIN = 19;
        const int MOTOR1_DIRPIN = 20;
        const int MOTOR2_SPDPIN = 21;
        const int MOTOR2_DIRPIN = 26;

        List<Motor> MotorArray { get; set; } = null;

        /// <summary>
        /// Gets the <see cref="Motor"/> at the specified index
        /// </summary>
        /// <param name="key">The zero-based (0 to 1) of the motor to get</param>
        /// <returns>The <see cref="Motor"/> at the specified index</returns>
        public Motor this[int key]
        {
            get
            {
                if (key < 0 || key > 1)
                    throw new Exception("Motors are 0..1");

                return MotorArray[key];
            }
        }

        /// <summary>
        /// Motor #1
        /// </summary>
        /// <value></value>
        public Motor One { get => MotorArray[0]; }

        /// <summary>
        /// Motor #2
        /// </summary>
        /// <value></value>
        public Motor Two { get => MotorArray[1]; }

        /// <summary>
        /// Both motors turns forwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Forwards(double speed = 1)
        {
            MotorArray[0].Forwards(speed);
            MotorArray[1].Forwards(speed);
        }

        /// <summary>
        /// Both motors turns backwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Backwards(double speed = 1)
        {
            MotorArray[0].Backwards(speed);
            MotorArray[1].Backwards(speed);
        }

        /// <summary>
        /// Both motors stops
        /// </summary>
        public void Stop()
        {
            MotorArray[0].Stop();
            MotorArray[1].Stop();
        }

        /// <summary>
        /// Initializes a <see cref="Motors"/> instance
        /// </summary>
        internal Motors()
        {
            MotorArray = new List<Motor>()
            {
                new Motor(1, MOTOR1_SPDPIN, MOTOR1_DIRPIN),
                new Motor(2, MOTOR2_SPDPIN, MOTOR2_DIRPIN)
            };
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MotorArray[0].Dispose();
                    MotorArray[1].Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Motors"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}