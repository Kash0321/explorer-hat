using System.Device.Gpio;

namespace ExplorerHat.Gpio
{
    /// <summary>
    /// Gpio controller
    /// </summary>
    internal class GpioController
    {
        /// <summary>
        /// Gets current <see cref="GpioController"/> instance
        /// </summary>
        internal static System.Device.Gpio.GpioController Current { get; set; } = null;

        /// <summary>
        /// Ensures pin opening
        /// </summary>
        /// <param name="pin">Pin number</param>
        /// <param name="pinMode">Pin opening mode to apply</param>
        internal static void EnsureOpenPin(int pin, PinMode pinMode)
        {
            if (!Current.IsPinOpen(pin) || Current.GetPinMode(pin) != pinMode)
            {
                if (Current.IsPinOpen(pin))
                {
                    Current.ClosePin(pin);
                }
                Current.OpenPin(pin, pinMode);
            }
        }

        /// <summary>
        /// Static initializer for <see cref="GpioInstanceManager"/>
        /// </summary>
        static GpioController()
        {
            Current = new System.Device.Gpio.GpioController();
        }
    }
}
