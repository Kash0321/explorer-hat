using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;

namespace ExplorerHat.Common
{
    /// <summary>
    /// 20x4 character LCD (2004) with a PCF8574 I2C backpack, on I2C bus 1.
    /// </summary>
    /// <remarks>
    /// The backpack is powered at 3.3 V from the 3v3 pin of the Explorer HAT side row, so the I2C bus never goes
    /// above 3.3 V. Its blue potentiometer sets the contrast. The screen keeps the last text after the program ends.
    /// The screen only has ASCII characters: no accents.
    /// </remarks>
    public class Screen : IDisposable
    {
        /// <summary>
        /// Characters in each line
        /// </summary>
        public const int Columns = 20;

        /// <summary>
        /// Lines of the screen
        /// </summary>
        public const int Lines = 4;

        private readonly I2cDevice _device;
        private readonly Pcf8574 _driver;
        private readonly Lcd2004 _lcd;

        /// <summary>
        /// Initializes a <see cref="Screen"/> instance and clears the screen
        /// </summary>
        /// <param name="address">I2C address of the backpack (0x27 with A0, A1 and A2 open)</param>
        public Screen(int address = 0x27)
        {
            _device = I2cDevice.Create(new I2cConnectionSettings(1, address));
            _driver = new Pcf8574(_device);
            // Backpack wiring: RS = P0, RW = P1, E = P2, backlight = P3, data D4-D7 = P4-P7
            _lcd = new Lcd2004(registerSelectPin: 0, enablePin: 2, dataPins: new[] { 4, 5, 6, 7 },
                backlightPin: 3, backlightBrightness: 1f, readWritePin: 1, controller: new GpioController(_driver));
            _lcd.Clear();
        }

        /// <summary>
        /// Clears the whole screen
        /// </summary>
        public void Clear()
        {
            _lcd.Clear();
        }

        /// <summary>
        /// Writes a text on a line (0 to 3). The text replaces the whole line: it is cut at 20 characters,
        /// and shorter texts are filled with spaces so nothing from the old text stays.
        /// </summary>
        public void Write(int line, string text)
        {
            if (line < 0 || line >= Lines)
            {
                throw new ArgumentOutOfRangeException(nameof(line), line, $"The screen has lines 0 to {Lines - 1}");
            }

            string fitted = text.Length > Columns ? text.Substring(0, Columns) : text.PadRight(Columns);
            _lcd.SetCursorPosition(0, line);
            _lcd.Write(fitted);
        }

        /// <summary>
        /// Releases the I2C device. The text stays on the screen.
        /// </summary>
        public void Dispose()
        {
            _lcd.Dispose();
            _driver.Dispose();
            _device.Dispose();
        }
    }
}
