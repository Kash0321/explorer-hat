using Iot.Device.ExplorerHat.Gpio;

namespace Iot.Device.ExplorerHat.Lighting
{
    /// <summary>
    /// Represents a led light
    /// </summary>
    public class Led
    {
        /// <summary>
        /// GPIO pin to which led is attached
        /// </summary>
        public int Pin { get; private set; }
        
        /// <summary>
        /// Led number on Hat
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Led name on Hat
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets if led is switched on or not
        /// </summary>
        /// <value></value>
        public bool IsOn { get; set; }

        /// <summary>
        /// Initializes a <see cref="Led"/> instance
        /// </summary>
        internal Led(int number, string name, int pin)
        {
            Number = number;
            Name = name;
            Pin = pin;
            IsOn = false;
        }

        /// <summary>
        /// Switch on this led light
        /// </summary>
        public void On()
        {

            if (!IsOn)
            {
                GpioController.EnsureOpenPin(Pin, System.Device.Gpio.PinMode.Output);
                GpioController.Current.Write(Pin, System.Device.Gpio.PinValue.High);
                IsOn = true;
            }
        }

        /// <summary>
        /// Switch off this led light
        /// </summary>
        public void Off()
        {

            if (IsOn)
            {
                GpioController.EnsureOpenPin(Pin, System.Device.Gpio.PinMode.Output);
                GpioController.Current.Write(Pin, System.Device.Gpio.PinValue.Low);
                IsOn = false;
            }
        }
    }
}
