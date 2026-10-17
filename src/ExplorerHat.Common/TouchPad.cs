namespace ExplorerHat.Common
{
    /// <summary>
    /// One of the eight touch pads of the Explorer HAT Pro (see <see cref="TouchPads"/>)
    /// </summary>
    public class TouchPad
    {
        private readonly TouchPads _pads;
        private readonly byte _inputBit;

        internal TouchPad(TouchPads pads, byte inputBit)
        {
            _pads = pads;
            _inputBit = inputBit;
        }

        /// <summary>
        /// Tells if a finger touches the pad. The first read of any pad opens the I2C device.
        /// </summary>
        /// <remarks>
        /// A pad that is held stays touched in every read. A very short touch is seen only by the next read of
        /// any pad, and that read cleans it: read the pads once in every turn of a loop, and hold them.
        /// By default the sensor reports one touch at a time.
        /// </remarks>
        public bool IsTouched() => _pads.IsTouched(_inputBit);
    }
}
