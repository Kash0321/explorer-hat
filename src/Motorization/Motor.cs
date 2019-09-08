using Iot.Device.ExplorerHat.Gpio;
using System;

namespace Iot.Device.ExplorerHat.Motorization
{
    /// <summary>
    /// Represent one of the onboard motors
    /// </summary>
    public class Motor : IDisposable
    {
        /// <summary>
        /// Managed <see cref="DCMotor"/>
        /// </summary>
        public DCMotor.DCMotor InnerMotor { get; private set; }

        /// <summary>
        /// Motor number on Hat
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// GPIO pin to control speed
        /// </summary>
        public int SpeedControlPin { get; private set; }

        /// <summary>
        /// GPIO pin to control direction
        /// </summary>
        public int DirectionPin { get; private set; }

        /// <summary>
        /// Current speed
        /// </summary>
        public double Speed
        {
            get
            {
                return InnerMotor.Speed;
            }
            set
            {
                double speed;

                if (value > 1)
                {
                    speed = 1;
                }
                else if (value < -1)
                {
                    speed = -1;
                }
                else
                {
                    speed = value;
                }

                InnerMotor.Speed = speed;
            }
        }

        /// <summary>
        /// Motor turns forwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Forwards(double speed = 1)
        {
            Speed = speed;
        }

        /// <summary>
        /// Motor turns backwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Backwards(double speed = 1)
        {
            Speed = Math.Abs(speed) * -1;
        }

        /// <summary>
        /// Stops the <see cref="Motor"/>
        /// </summary>
        public void Stop()
        {
            InnerMotor.Speed = 0;
        }

        /// <summary>
        /// Initializes a <see cref="Motor"/> instance
        /// </summary>
        /// <param name="number">Motor #</param>
        /// <param name="speedControlPin">GPIO pin to control speed</param>
        /// <param name="directionPin">GPIO pin to control direction</param>
        internal Motor(int number, int speedControlPin, int directionPin)
        {
            Number = number;
            SpeedControlPin = speedControlPin;
            DirectionPin = directionPin;

            InnerMotor = DCMotor.DCMotor.Create(SpeedControlPin, DirectionPin, GpioController.Current);
            Stop();
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
                    Stop();
                    InnerMotor.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Motor"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
