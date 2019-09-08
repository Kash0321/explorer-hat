using Iot.Device.ExplorerHat.Gpio;
using Serilog;
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
                Log.Information("Motor #{motorNumber} speed setted at {speed}", Number, Speed);
            }
        }

        /// <summary>
        /// Motor turns forwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Forwards(double speed = 1)
        {
            Speed = speed;
            Log.Information("Motor #{motorNumber} {direction} at {speed}", Number, "forwards", Speed);
        }

        /// <summary>
        /// Motor turns backwards at indicated speed
        /// </summary>
        /// <param name="speed"></param>
        public void Backwards(double speed = 1)
        {
            Speed = Math.Abs(speed) * -1;
            Log.Information("Motor #{motorNumber} {direction} at {speed}", Number, "backwards", Speed);
        }

        /// <summary>
        /// Stops the <see cref="Motor"/>
        /// </summary>
        public void Stop()
        {
            InnerMotor.Speed = 0;
            Log.Information("Motor #{motorNumber} {action}", Number, "stopped");
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
            Speed = 0;
            Log.Information("Motor #{motorNumber} initialized", Number);
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
                    Log.Debug("Disposing motor #{motorNumber}", Number);
                    Stop();
                    InnerMotor.Dispose();
                    Log.Information("Motor #{motorNumber} disposed", Number);
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
