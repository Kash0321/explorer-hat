using System;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerHat.Light
{
    /// <summary>
    /// Represents the Explorer HAT led array
    /// </summary>
    public class Light
    {
        List<Led> LedArray { get; set; }

        /// <summary>
        /// Blue led (number 1)
        /// </summary>
        public Led Blue
        {
            get => LedArray[0];
        }

        /// <summary>
        /// Yellow led (number 2)
        /// </summary>
        public Led Yellow
        {
            get => LedArray[1];
        }

        /// <summary>
        /// Red led (number 3)
        /// </summary>
        public Led Red
        {
            get => LedArray[2];
        }

        /// <summary>
        /// Green led (number 4)
        /// </summary>
        public Led Green
        {
            get => LedArray[3];
        }

        /// <summary>
        /// Gets the <see cref="Led"/> at the specified index
        /// </summary>
        /// <param name="key">The zero-based (0 to 3) of the led to get</param>
        /// <returns>The <see cref="Led"/> at the specified index</returns>
        public Led this[int key]
        {
            get
            {
                if (key < 0 || key > 3)
                    throw new Exception("Leds are 0..3");

                return LedArray[key];
            }
        }

        /// <summary>
        /// Gets the <see cref="Led"/> at the specified index
        /// </summary>
        /// <param name="key">The color-string-index (blue, yellow, red or green) of the led to get</param>
        /// <returns>The <see cref="Led"/> at the specified index</returns>
        public Led this[string key]
        {
            get
            {
                var strKey = key.ToLower();
                if (strKey != "blue" && strKey != "yellow" && strKey != "red" && strKey != "green")
                    throw new Exception("Leds are blue, yellow, red or green");

                var result = LedArray.Where(l => l.Name.ToLower() == strKey).First();

                return result;
            }
        }

        /// <summary>
        /// Initializes a <see cref="Light"/> instance
        /// </summary>
        public Light()
        {
            LedArray = new List<Led>()
            {
                new Led(1, "blue", 4),
                new Led(2, "yellow", 17),
                new Led(3, "red", 27),
                new Led(4, "green", 5)
            };
        }
    }
}
