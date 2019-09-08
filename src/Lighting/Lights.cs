using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iot.Device.ExplorerHat.Lighting
{
    /// <summary>
    /// Represents the Explorer HAT led array
    /// </summary>
    public class Lights : IDisposable
    {
        const int LED1_PIN = 4;
        const int LED2_PIN = 17;
        const int LED3_PIN = 27;
        const int LED4_PIN = 5;

        List<Led> LedArray { get; set; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led One { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Two { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Three { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Four { get => LedArray[3]; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led Blue { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Yellow { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Red { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Green { get => LedArray[3]; }

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
        /// Initializes a <see cref="Lights"/> instance
        /// </summary>
        internal Lights()
        {
            LedArray = new List<Led>()
            {
                new Led(1, "blue", LED1_PIN),
                new Led(2, "yellow", LED2_PIN),
                new Led(3, "red", LED3_PIN),
                new Led(4, "green", LED4_PIN)
            };
            var featureName = "Lighting";
            Log.Information("{featureName} initialized", featureName);
        }

        #region IDisposable Support

        private bool disposedValue = false; // Para detectar llamadas redundantes

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var featureName = "Lighting";
                    Log.Debug("Disposing {featureName} features", featureName);
                    LedArray[0].Dispose();
                    LedArray[1].Dispose();
                    LedArray[2].Dispose();
                    LedArray[3].Dispose();
                    Log.Information("{featureName} features disposed", featureName);
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        
        #endregion
    }
}
