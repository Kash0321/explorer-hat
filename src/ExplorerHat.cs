using Iot.Device.ExplorerHat.Gpio;
using Iot.Device.ExplorerHat.Lighting;
using Iot.Device.ExplorerHat.Motorization;
using Serilog;
using System;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        /// <summary>
        /// Explorer HAT DCMotors collection
        /// </summary>
        public Motors Motors { get; private set; }

        /// <summary>
        /// Explorer HAT led array
        /// </summary>
        public Lights Lights { get; private set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        public ExplorerHat()
        {
            Motors = new Motors();
            Lights = new Lights();
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
                    var featureName = "Explorer HAT resources";
                    var gpioFeatureName = "Managed GpioController resources";
                    Log.Debug("Disposing {featureName}", featureName);
                    Lights.Dispose();
                    Motors.Dispose();
                    Log.Debug("Disposing {featureName}", gpioFeatureName);
                    GpioController.Current.Dispose();
                    Log.Debug("{featureName} disposed", gpioFeatureName);
                    Log.Information("{featureName} features disposed", featureName);
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
