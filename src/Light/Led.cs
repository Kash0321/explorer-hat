using ExplorerHat.Gpio;

namespace ExplorerHat.Light
{
    /// <summary>
    /// Represents a led light
    /// </summary>
    public class Led
    {
        /// <summary>
        /// GPIO pin to which pin is attached
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
        /// Initializes a <see cref="Led"/> instance
        /// </summary>
        public Led(int number, string name, int pin)
        {
            Number = number;
            Name = name;
            Pin = pin;
        }

        /// <summary>
        /// 
        /// </summary>
        public void On()
        {
            GpioController.EnsureOpenPin(Pin, System.Device.Gpio.PinMode.Output);
        }
    }
}
