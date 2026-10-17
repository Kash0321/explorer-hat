using System.Device.I2c;

namespace ExplorerHat.Common
{
    /// <summary>
    /// The eight touch pads of the Explorer HAT Pro, read from its CAP1208 touch sensor (I2C bus 1, address 0x28).
    /// </summary>
    /// <remarks>
    /// The HAT numbers the pads differently than the chip: pads 1 to 4 are the chip inputs 5 to 8 and pads 5 to 8
    /// are the chip inputs 1 to 4.
    /// <para>
    /// This class has the shape of the touch pads proposed for the <c>Iot.Device.ExplorerHat</c> binding of
    /// dotnet/iot, and follows <c>Cap1208</c> (dotnet/iot pull request 2616), which is not in <c>Iot.Device.Bindings</c>
    /// 4.2.0. When the binding has touch pads in a published version, delete <see cref="TouchPads"/> and
    /// <see cref="TouchPad"/>: the lessons do not change.
    /// </para>
    /// <para>
    /// The configuration of the chip is not changed: by default it reports one touch at a time. The only write is
    /// the clearing of its INT bit, which makes the chip drop the pads that are no longer touched. Without it, a pad
    /// would stay "touched" for ever.
    /// </para>
    /// </remarks>
    public class TouchPads : IDisposable
    {
        private const int I2cBus = 1;
        private const int Address = 0x28;

        // Registers of the CAP1208
        private const byte MainControlRegister = 0x00;
        private const byte SensorInputStatusRegister = 0x03;
        private const byte ProductIdRegister = 0xFD;
        private const byte ManufacturerIdRegister = 0xFE;

        private const byte Cap1208ProductId = 0x6B;
        private const byte MicrochipManufacturerId = 0x5D;
        private const byte InterruptBit = 0x01;

        private readonly object _lock = new object();
        private I2cDevice? _device;
        private bool _disposed;

        /// <summary>
        /// Touch pad 1 (CAP1208 input 5)
        /// </summary>
        public TouchPad One { get; }

        /// <summary>
        /// Touch pad 2 (CAP1208 input 6)
        /// </summary>
        public TouchPad Two { get; }

        /// <summary>
        /// Touch pad 3 (CAP1208 input 7)
        /// </summary>
        public TouchPad Three { get; }

        /// <summary>
        /// Touch pad 4 (CAP1208 input 8)
        /// </summary>
        public TouchPad Four { get; }

        /// <summary>
        /// Touch pad 5 (CAP1208 input 1)
        /// </summary>
        public TouchPad Five { get; }

        /// <summary>
        /// Touch pad 6 (CAP1208 input 2)
        /// </summary>
        public TouchPad Six { get; }

        /// <summary>
        /// Touch pad 7 (CAP1208 input 3)
        /// </summary>
        public TouchPad Seven { get; }

        /// <summary>
        /// Touch pad 8 (CAP1208 input 4)
        /// </summary>
        public TouchPad Eight { get; }

        /// <summary>
        /// Initializes a <see cref="TouchPads"/> instance. The I2C device is not opened until a pad is read,
        /// so a program that never reads one does not depend on the touch sensor.
        /// </summary>
        public TouchPads()
        {
            One = new TouchPad(this, 0x10);
            Two = new TouchPad(this, 0x20);
            Three = new TouchPad(this, 0x40);
            Four = new TouchPad(this, 0x80);
            Five = new TouchPad(this, 0x01);
            Six = new TouchPad(this, 0x02);
            Seven = new TouchPad(this, 0x04);
            Eight = new TouchPad(this, 0x08);
        }

        internal bool IsTouched(byte inputBit)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(TouchPads));
                }

                _device ??= OpenDevice();

                // The status register keeps a touch until the INT bit is cleared, so a short touch between two
                // reads is not lost. After reading it, clear INT: the chip then drops the pads that are released.
                byte status = ReadRegister(_device, SensorInputStatusRegister);

                byte mainControl = ReadRegister(_device, MainControlRegister);
                if ((mainControl & InterruptBit) != 0)
                {
                    _device.Write(new byte[] { MainControlRegister, (byte)(mainControl & ~InterruptBit) });
                }

                return (status & inputBit) != 0;
            }
        }

        private static I2cDevice OpenDevice()
        {
            I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(I2cBus, Address));
            try
            {
                byte productId = ReadRegister(device, ProductIdRegister);
                byte manufacturerId = ReadRegister(device, ManufacturerIdRegister);
                if (productId != Cap1208ProductId || manufacturerId != MicrochipManufacturerId)
                {
                    throw new IOException($"The device at 0x{Address:X2} is not a CAP1208: product ID 0x{productId:X2} " +
                        $"(expected 0x{Cap1208ProductId:X2}), manufacturer ID 0x{manufacturerId:X2} (expected 0x{MicrochipManufacturerId:X2}).");
                }
            }
            catch
            {
                device.Dispose();
                throw;
            }

            return device;
        }

        private static byte ReadRegister(I2cDevice device, byte register)
        {
            Span<byte> buffer = stackalloc byte[1];
            device.WriteRead(new[] { register }, buffer);
            return buffer[0];
        }

        /// <summary>
        /// Releases the I2C device, if a pad was read
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
                _device?.Dispose();
                _device = null;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
