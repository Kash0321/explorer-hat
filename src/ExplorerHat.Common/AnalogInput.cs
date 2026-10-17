using Iot.Device.Ads1115;
using UnitsNet;

namespace ExplorerHat.Common
{
    /// <summary>
    /// One of the four analog inputs of the Explorer HAT Pro (see <see cref="AnalogInputs"/>)
    /// </summary>
    public class AnalogInput
    {
        private readonly AnalogInputs _inputs;
        private readonly InputMultiplexer _channel;

        internal AnalogInput(AnalogInputs inputs, InputMultiplexer channel)
        {
            _inputs = inputs;
            _channel = channel;
        }

        /// <summary>
        /// Measures the voltage of the input, from 0 V to 5 V, with a resolution of 3 mV.
        /// The first measurement of any analog input opens the I2C device.
        /// </summary>
        /// <remarks>
        /// An input with nothing connected does not read 0 V: it floats around 0.5 V.
        /// </remarks>
        public ElectricPotential ReadVoltage() => _inputs.ReadVoltage(_channel);
    }
}
