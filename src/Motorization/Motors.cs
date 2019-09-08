using System;
using System.Collections.Generic;

namespace Iot.Device.ExplorerHat.Motorization
{
    /// <summary>
    /// DCMotors: Collection and management
    /// </summary>
    public class Motors : IDisposable
    {
        List<Motor> MotorArray { get; set; } = null;

        /// <summary>
        /// Gets the <see cref="Motor"/> at the specified index
        /// </summary>
        /// <param name="key">The zero-based (0 to 3) of the led to get</param>
        /// <returns>The <see cref="Led"/> at the specified index</returns>
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
        public Motor One 
        { 
            get
            {
                return this[0];
            } 
        }

        /// <summary>
        /// Motor #2
        /// </summary>
        /// <value></value>
        public Motor Two 
        { 
            get
            {
                return this[1];
            } 
        }

        public void Forwards(double speed = 1)
        {
            this[0].Forwards(speed);
            this[1].Forwards(speed);
        }

        public void Backwards(double speed = 1)
        {
            this[0].Backwards(speed);
            this[1].Backwards(speed);
        }

        public void Stop()
        {
            this[0].Stop();
            this[1].Stop();
        }

        internal Motors()
        {
            MotorArray = new List<Motor>()
            {
                new Motor(1, 19, 20),
                new Motor(2, 21, 26)
            };
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this[0].Dispose();
                    this[1].Dispose();
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