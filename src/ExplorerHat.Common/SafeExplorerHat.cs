using Iot.Device.ExplorerHat;

namespace ExplorerHat.Common
{
    /// <summary>
    /// Explorer HAT that can be safely disposed: stops both motors, turns the lights off and
    /// releases every GPIO pin without crashing the process.
    /// </summary>
    /// <remarks>
    /// Use it exactly like <see cref="Iot.Device.ExplorerHat.ExplorerHat"/>, inside a <c>using</c> block.
    /// See <see cref="SharedGpioController"/> for the binding issue it works around.
    /// </remarks>
    public class SafeExplorerHat : IDisposable
    {
        private SharedGpioController? _controller;
        private Iot.Device.ExplorerHat.ExplorerHat? _hat;

        /// <summary>
        /// Explorer HAT motors
        /// </summary>
        public Motors Motors { get; }

        /// <summary>
        /// Explorer HAT lights
        /// </summary>
        public Lights Lights { get; }

        /// <summary>
        /// Initializes a <see cref="SafeExplorerHat"/> instance
        /// </summary>
        public SafeExplorerHat()
        {
            _controller = new SharedGpioController();
            _hat = new Iot.Device.ExplorerHat.ExplorerHat(_controller, shouldDispose: false);
            Motors = _hat.Motors;
            Lights = _hat.Lights;
        }

        /// <summary>
        /// Stops both motors, turns the lights off and releases every GPIO pin
        /// </summary>
        public void Dispose()
        {
            if (_hat is null || _controller is null)
            {
                return;
            }

            Motors.One.Speed = 0.0;
            Motors.Two.Speed = 0.0;
            Lights.Off();

            _hat.Dispose();
            _hat = null;

            _controller.Release();
            _controller = null;
        }
    }
}
