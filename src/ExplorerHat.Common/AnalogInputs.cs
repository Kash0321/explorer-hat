using System.Device.I2c;
using Iot.Device.Ads1115;
using UnitsNet;

namespace ExplorerHat.Common
{
    /// <summary>
    /// The four analog inputs of the Explorer HAT Pro, measured by its ADS1015 converter (I2C bus 1, address 0x48).
    /// </summary>
    /// <remarks>
    /// The converter is powered at 5 V: each input accepts from 0 V to 5 V. The HAT numbers the inputs the other way
    /// round than the chip: analog input 1 is channel 3 (AIN3), input 2 is AIN2, input 3 is AIN1 and input 4 is AIN0.
    /// <para>
    /// This class has the shape of the analog inputs proposed for the <c>Iot.Device.ExplorerHat</c> binding of
    /// dotnet/iot, but uses <see cref="Ads1115"/> of <c>Iot.Device.Bindings</c> 4.2.0 (the ADS1015 has the same
    /// registers, with the 12 bits in the upper part). When the binding has analog inputs in a published version,
    /// delete <see cref="AnalogInputs"/> and <see cref="AnalogInput"/>: the lessons do not change.
    /// </para>
    /// </remarks>
    public class AnalogInputs : IDisposable
    {
        private const int I2cBus = 1;
        private const int Address = 0x48;

        // The same scale as the binding: 6.144 V full scale (3 mV per step) and one measurement at a time
        private const MeasuringRange Range = MeasuringRange.FS6144;
        private const DataRate Rate = DataRate.SPS128;

        private readonly object _lock = new object();
        private Ads1115? _adc;
        private bool _disposed;

        /// <summary>
        /// Analog input 1 (ADS1015 channel 3)
        /// </summary>
        public AnalogInput One { get; }

        /// <summary>
        /// Analog input 2 (ADS1015 channel 2)
        /// </summary>
        public AnalogInput Two { get; }

        /// <summary>
        /// Analog input 3 (ADS1015 channel 1)
        /// </summary>
        public AnalogInput Three { get; }

        /// <summary>
        /// Analog input 4 (ADS1015 channel 0)
        /// </summary>
        public AnalogInput Four { get; }

        /// <summary>
        /// Initializes an <see cref="AnalogInputs"/> instance. The I2C device is not opened until an input is read,
        /// so a program that never reads one does not depend on the converter.
        /// </summary>
        public AnalogInputs()
        {
            One = new AnalogInput(this, InputMultiplexer.AIN3);
            Two = new AnalogInput(this, InputMultiplexer.AIN2);
            Three = new AnalogInput(this, InputMultiplexer.AIN1);
            Four = new AnalogInput(this, InputMultiplexer.AIN0);
        }

        internal ElectricPotential ReadVoltage(InputMultiplexer channel)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(AnalogInputs));
                }

                _adc ??= new Ads1115(I2cDevice.Create(new I2cConnectionSettings(I2cBus, Address)), channel, Range, Rate, DeviceMode.PowerDown);
                return _adc.ReadVoltage(channel);
            }
        }

        /// <summary>
        /// Releases the converter and its I2C device, if an input was read
        /// </summary>
        public void Dispose()
        {
            // An I2C read that never ends must not block the emergency stop: do not wait for it more than half a second
            if (!Monitor.TryEnter(_lock, TimeSpan.FromMilliseconds(500)))
            {
                return;
            }

            try
            {
                _disposed = true;
                _adc?.Dispose();
                _adc = null;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
