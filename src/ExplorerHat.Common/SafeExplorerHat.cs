using System.Device.Gpio;
using System.Runtime.InteropServices;
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
    /// <para>
    /// Emergency stop: the process is killed without running <c>using</c> or <c>finally</c> blocks on
    /// Ctrl+C (SIGINT), Ctrl+\ (SIGQUIT), <c>kill</c> (SIGTERM) or when the SSH session is closed (SIGHUP),
    /// which would leave the motors running. This class handles those signals and disposes itself first.
    /// </para>
    /// </remarks>
    public class SafeExplorerHat : IDisposable
    {
        // Explorer HAT output pins: motor 1 (19, 20), motor 2 (21, 26) and lights (4, 17, 27, 5)
        private static readonly int[] OutputPins = { 19, 20, 21, 26, 4, 17, 27, 5 };

        private readonly object _lock = new object();
        private readonly List<PosixSignalRegistration> _signalRegistrations = new List<PosixSignalRegistration>();
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

            foreach (var signal in new[] { PosixSignal.SIGINT, PosixSignal.SIGQUIT, PosixSignal.SIGTERM, PosixSignal.SIGHUP })
            {
                _signalRegistrations.Add(PosixSignalRegistration.Create(signal, OnSignal));
            }
        }

        private void OnSignal(PosixSignalContext context)
        {
            // The process keeps its default behavior afterwards (it ends), but with the motors stopped
            Console.WriteLine();
            Console.WriteLine($"Parada de emergencia ({context.Signal}): motores parados");
            Dispose();
        }

        /// <summary>
        /// Stops both motors, turns the lights off and releases every GPIO pin
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_hat is null || _controller is null)
                {
                    return;
                }

                foreach (var registration in _signalRegistrations)
                {
                    registration.Dispose();
                }

                Motors.One.Speed = 0.0;
                Motors.Two.Speed = 0.0;
                Lights.Off();

                _hat.Dispose();
                _hat = null;

                _controller.Release();
                _controller = null;

                // The program may still be running (e.g. on an emergency stop) and could have written a pin
                // right before it was released, so force every output low. Later writes fail: pins are closed.
                using (var gpio = new GpioController())
                {
                    foreach (var pin in OutputPins)
                    {
                        gpio.OpenPin(pin, PinMode.Output, PinValue.Low);
                    }
                }
            }
        }
    }
}
