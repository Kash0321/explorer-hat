using Iot.Device.ExplorerHat.Gpio;
using System;

namespace Iot.Device.ExplorerHat.Motorization
{
    /// <summary>
    /// Represent one of the motors on Explorer Hat
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
        /// GPIO pin to which motor bw is attached
        /// </summary>
        public int PinBw { get; private set; }

        /// <summary>
        /// GPIO pin to which motor fw is attached
        /// </summary>
        public int PinFw { get; private set; }

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

        public void Stop()
        {
            InnerMotor.Speed = 0;
        }

        /// <summary>
        /// Initializes a <see cref="Motor"/> instance
        /// </summary>
        /// <param name="number"></param>
        /// <param name="pinBw"></param>
        /// <param name="pinFw"></param>
        internal Motor(int number, int pinBw, int pinFw)
        {
            Number = number;
            PinBw = pinBw;
            PinFw = pinFw;

            InnerMotor = DCMotor.DCMotor.Create(PinBw, PinFw, GpioController.Current);
            Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    InnerMotor.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
