using System.Diagnostics;

namespace ExplorerHat.Orders
{
    /// <summary>The answer to an order: an HTTP status and the body to send</summary>
    public record Outcome(int Status, object Body);

    /// <summary>
    /// Runs one order at a time. A movement runs in a thread of its own and ends when its time is up, when
    /// <see cref="Stop"/> is called or when the obstacle layer says so. There is no queue: if the connection
    /// with the brain is lost, the movement in progress is the last one (3 seconds at most).
    /// </summary>
    public class OrderRunner
    {
        // How often a movement looks at the clock, the stop and the obstacle layer (milliseconds)
        private const int Step = 20;

        private readonly IRobotBody _body;
        private readonly ObstacleMonitor _monitor;

        // Everything that writes to the motors does it holding this lock, so only one thread writes at a time
        private readonly object _lock = new object();
        private string? _current;
        private CancellationTokenSource? _cancel;

        public OrderRunner(IRobotBody body, ObstacleMonitor monitor)
        {
            _body = body;
            _monitor = monitor;
        }

        /// <summary>The name of the movement in progress, or null</summary>
        public string? Current
        {
            get
            {
                lock (_lock)
                {
                    return _current;
                }
            }
        }

        /// <summary>Stops the motors right now and ends the movement in progress</summary>
        public Outcome Stop()
        {
            lock (_lock)
            {
                _cancel?.Cancel();
                _body.SetMotors(0.0, 0.0);
            }

            return new Outcome(200, new { estado = "parado" });
        }

        public Task<Outcome> RunAsync(ValidOrder order)
        {
            if (order.Name == "parar")
            {
                return Task.FromResult(Stop());
            }

            CancellationToken token;
            lock (_lock)
            {
                if (_current is not null)
                {
                    return Task.FromResult(new Outcome(409, new { error = "ocupado", accion = _current }));
                }

                if (!order.IsMovement)
                {
                    _body.SetLight(order.Color, order.On);
                    return Task.FromResult(new Outcome(200, new { estado = "ok" }));
                }

                if (order.Name == "avanzar" && _monitor.Way == WayState.NoData)
                {
                    return Task.FromResult(new Outcome(503, new { error = "sin datos de los sensores de distancia: no se avanza" }));
                }

                _current = order.Name;
                _cancel = new CancellationTokenSource();
                token = _cancel.Token;
            }

            var done = new TaskCompletionSource<Outcome>();
            var thread = new Thread(() => Move(order, token, done)) { Name = "Movement" };
            thread.Start();
            return done.Task;
        }

        private void Move(ValidOrder order, CancellationToken cancel, TaskCompletionSource<Outcome> done)
        {
            string reason = "tiempo";
            var clock = Stopwatch.StartNew();
            Outcome? failure = null;

            try
            {
                lock (_lock)
                {
                    // Stop() may have come before the movement started: then nothing is written
                    if (cancel.IsCancellationRequested)
                    {
                        reason = "parada";
                    }
                    else if (order.Name == "avanzar" && _monitor.Way != WayState.Clear)
                    {
                        reason = _monitor.Way == WayState.NoData ? "sensores" : "obstaculo";
                    }
                    else
                    {
                        StartMotors(order);
                    }
                }

                while (reason == "tiempo")
                {
                    if (clock.Elapsed.TotalSeconds >= order.Seconds)
                    {
                        break;
                    }

                    if (cancel.WaitHandle.WaitOne(Step))
                    {
                        reason = "parada";
                    }
                    else if (order.Name == "avanzar" && _monitor.Way != WayState.Clear)
                    {
                        reason = _monitor.Way == WayState.NoData ? "sensores" : "obstaculo";
                    }
                }
            }
            catch (Exception exception)
            {
                failure = new Outcome(500, new { error = "fallo al mover los motores: " + exception.Message });
            }
            finally
            {
                // First the motors, then the robot is free again: a new order cannot start before they are stopped
                lock (_lock)
                {
                    try
                    {
                        _body.SetMotors(0.0, 0.0);
                    }
                    catch (Exception)
                    {
                        // The hat is already released (emergency stop): the pins are low
                    }

                    _current = null;
                    _cancel = null;
                }
            }

            done.SetResult(failure ?? new Outcome(200, new
            {
                estado = "terminada",
                motivo = reason,
                segundosReales = Math.Round(clock.Elapsed.TotalSeconds, 2),
                ajustes = order.Adjustments,
            }));
        }

        // Motor One is the right wheel and Two the left one: turning right is One backwards and Two forwards
        private void StartMotors(ValidOrder order)
        {
            double speed = order.Speed;
            switch (order.Name)
            {
                case "avanzar":
                    _body.SetMotors(speed, speed);
                    break;
                case "retroceder":
                    _body.SetMotors(-speed, -speed);
                    break;
                default:
                    _body.SetMotors(order.Direction == "derecha" ? -speed : speed, order.Direction == "derecha" ? speed : -speed);
                    break;
            }
        }
    }
}
