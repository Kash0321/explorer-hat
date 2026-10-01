using System.Buffers.Binary;
using System.Device.I2c;

namespace ExplorerHat.UpsDashboard
{
    /// <summary>
    /// Waveshare UPS HAT (B): two 18650 batteries in series, under the Raspberry Pi.
    /// Its only data connection is an INA219 chip on I2C (address 0x42) that measures the batteries:
    /// their voltage and the voltage on a 0.1 ohm resistor, which gives the current.
    /// The Ina219 binding of Iot.Device.Bindings 4.2.0 reads the resistor voltage and the current as numbers
    /// without sign, so it gets negative currents (batteries discharging) wrong: this class reads the registers itself.
    /// It only reads: the chip settings are not changed.
    /// </summary>
    public class UpsHat : IDisposable
    {
        const int I2C_BUS = 1;
        const int I2C_ADDRESS = 0x42;

        // INA219 registers
        const byte CONFIGURATION_REGISTER = 0x00;
        const byte SHUNT_VOLTAGE_REGISTER = 0x01;
        const byte BUS_VOLTAGE_REGISTER = 0x02;

        // Resistor that measures the current, in ohms
        const double SHUNT_RESISTANCE = 0.1;

        // Two 18650 batteries in series: empty at 6.0 V and full at 8.4 V (the values of the Waveshare example)
        const double EMPTY_VOLTAGE = 6.0;
        const double FULL_VOLTAGE = 8.4;

        private readonly I2cDevice _device;

        /// <summary>
        /// Configuration register, as read from the chip
        /// </summary>
        public ushort ConfigurationRegister { get; private set; }

        /// <summary>
        /// Resistor voltage register, as read from the chip (with sign, 10 µV each)
        /// </summary>
        public short ShuntVoltageRegister { get; private set; }

        /// <summary>
        /// Battery voltage register, as read from the chip (bits 15-3: voltage, 4 mV each; bit 1: new reading ready;
        /// bit 0: overflow)
        /// </summary>
        public ushort BusVoltageRegister { get; private set; }

        /// <summary>
        /// Voltage of the two batteries together, in volts
        /// </summary>
        public double BatteryVoltage => (BusVoltageRegister >> 3) * 4 / 1000.0;

        /// <summary>
        /// Voltage on the resistor that measures the current, in millivolts
        /// </summary>
        public double ShuntVoltage => ShuntVoltageRegister * 10 / 1000.0;

        /// <summary>
        /// Battery current, in amperes: positive when the batteries are charging, negative when they are discharging
        /// </summary>
        public double Current => ShuntVoltage / 1000.0 / SHUNT_RESISTANCE;

        /// <summary>
        /// Power going into (positive) or out of (negative) the batteries, in watts
        /// </summary>
        public double Power => BatteryVoltage * Current;

        /// <summary>
        /// Battery charge estimated from the voltage, from 0 to 100
        /// </summary>
        public double BatteryPercent
        {
            get
            {
                double percent = (BatteryVoltage - EMPTY_VOLTAGE) / (FULL_VOLTAGE - EMPTY_VOLTAGE) * 100;
                return Math.Clamp(percent, 0, 100);
            }
        }

        /// <summary>
        /// Whether the chip has a new reading since the last time (bit 1 of the battery voltage register)
        /// </summary>
        public bool ConversionReady => (BusVoltageRegister & 0b10) != 0;

        /// <summary>
        /// Whether the chip could not compute a value because it was too big (bit 0 of the battery voltage register)
        /// </summary>
        public bool Overflow => (BusVoltageRegister & 0b01) != 0;

        /// <summary>
        /// Initializes a <see cref="UpsHat"/> instance
        /// </summary>
        public UpsHat()
        {
            _device = I2cDevice.Create(new I2cConnectionSettings(I2C_BUS, I2C_ADDRESS));
        }

        /// <summary>
        /// Reads all the registers from the chip
        /// </summary>
        public void Read()
        {
            ConfigurationRegister = ReadRegister(CONFIGURATION_REGISTER);
            ShuntVoltageRegister = (short)ReadRegister(SHUNT_VOLTAGE_REGISTER);
            BusVoltageRegister = ReadRegister(BUS_VOLTAGE_REGISTER);
        }

        /// <summary>
        /// Reads a 16-bit register (the chip sends the high byte first)
        /// </summary>
        private ushort ReadRegister(byte register)
        {
            Span<byte> buffer = stackalloc byte[2];
            _device.WriteRead(new byte[] { register }, buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        /// <summary>
        /// Releases the I2C connection
        /// </summary>
        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
