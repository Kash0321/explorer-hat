using System.Device.Gpio;

namespace Iot.Device.ExplorerHat.Gpio
{
    /// <summary>
    /// Gpio controller
    /// </summary>
    public class GpioController
    {
        /// <summary>
        /// Gets current <see cref="GpioController"/> instance
        /// </summary>
        public static System.Device.Gpio.GpioController Current { get; set; } = null;

        /// <summary>
        /// Ensures pin opening
        /// </summary>
        /// <param name="pin">Pin number</param>
        /// <param name="pinMode">Pin opening mode to apply</param>
        public static void EnsureOpenPin(int pin, PinMode pinMode)
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
