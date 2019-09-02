using Iot.Device.ExplorerHat.Lighting;
using Iot.Device.ExplorerHat.Motorization;
using System;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        public Motor Motor { get; set; }
        public Light Light { get; set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        public ExplorerHat()
        {
            Motor = new Motor();
            Light = new Light();
        }

        #region IDisposable Support

        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: elimine el estado administrado (objetos administrados).
                }

                // TODO: libere los recursos no administrados (objetos no administrados) y reemplace el siguiente finalizador.
                // TODO: configure los campos grandes en nulos.

                disposedValue = true;
            }
        }

        // TODO: reemplace un finalizador solo si el anterior Dispose(bool disposing) tiene c�digo para liberar los recursos no administrados.
        // ~ExplorerHat()
        // {
        //   // No cambie este c�digo. Coloque el c�digo de limpieza en el anterior Dispose(colocaci�n de bool).
        //   Dispose(false);
        // }

        // Este c�digo se agrega para implementar correctamente el patr�n descartable.
        public void Dispose()
        {
            // No cambie este c�digo. Coloque el c�digo de limpieza en el anterior Dispose(colocaci�n de bool).
            Dispose(true);
            // TODO: quite la marca de comentario de la siguiente l�nea si el finalizador se ha reemplazado antes.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
