using Iot.Device.ExplorerHat.Gpio;
using Iot.Device.ExplorerHat.Lighting;
using Iot.Device.ExplorerHat.Motorization;
using System;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        public Motors Motors { get; set; }
        public Lights Lights { get; set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        public ExplorerHat()
        {
            Motors = new Motors();
            Lights = new Lights();
        }

        #region IDisposable Support

        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Lights.Dispose();
                    Motors.Dispose();
                    GpioController.Current.Dispose();
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
