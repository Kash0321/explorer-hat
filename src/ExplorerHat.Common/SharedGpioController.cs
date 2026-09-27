using System.Device.Gpio;

namespace ExplorerHat.Common
{
    /// <summary>
    /// <see cref="GpioController"/> that ignores <see cref="GpioController.Dispose()"/> calls until
    /// <see cref="Release"/> is invoked.
    /// </summary>
    /// <remarks>
    /// Workaround for Iot.Device.Bindings 4.2.0: <c>ExplorerHat</c> hands its controller to <c>Motors</c>,
    /// <c>Lights</c>, every <c>Led</c> and every <c>DCMotor</c> with <c>shouldDispose = true</c>, so the first
    /// one disposed closes every pin while the motors' software PWM threads keep writing to them, and the
    /// process dies with "Can not write to pin 19 because it is not open".
    /// </remarks>
    internal class SharedGpioController : GpioController
    {
        private bool _released;

        /// <summary>
        /// Actually disposes the controller, closing every pin
        /// </summary>
        public void Release()
        {
            _released = true;
            Dispose();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_released)
            {
                base.Dispose(disposing);
            }
        }
    }
}
