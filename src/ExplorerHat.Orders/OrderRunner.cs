using System.Diagnostics;

namespace ExplorerHat.Orders
{
    /// <summary>The answer to an order: an HTTP status and the body to send</summary>
    public record Outcome(int Status, object Body);

    /// <summary>
    /// Runs one order at a time. A movement runs in a thread of its own and ends when its time is up (or, by pulses, when
    /// the wheels count its centimeters or degrees), when <see cref="Stop"/> is called or when the obstacle layer says so.
    /// There is no queue: if the connection with the brain is lost, the movement in progress is the last one (3 seconds at
    /// most, 1 second backwards).
    /// </summary>
    public class OrderRunner
    {
        // How often a movement by time looks at the clock, the stop and the obstacle layer (milliseconds)
        private const int Step = 20;

        // A movement by pulses reads the counters more often
        private const int PulseStep = 2;

        // A wheel that is driven and gives no pulse for this long (milliseconds) does not turn, or its sensor does not count
        private const int StallTime = 700;

        // After the motors stop, the pulses are counted for this long (milliseconds): the robot slides a bit
        private const int SlideTime = 300;

        // Going forwards, a bigger difference between the wheels (pulses) means that a sensor does not count: stop
        private const int MaxDifference = 10;

        // Going forwards, how much the speed of a wheel changes for each pulse of difference (as in lesson 10)
        private const double Correction = 0.02;

        private readonly IRobotBody _body;
        private readonly ObstacleMonitor _monitor;
        private readonly IWheelCounter _wheels;
        private readonly PulseSettings _pulses;

        // Everything that writes to the motors does it holding this lock, so only one thread writes at a time
        private readonly object _lock = new object();
        private string? _current;
        private CancellationTokenSource? _cancel;

        public OrderRunner(IRobotBody body, ObstacleMonitor monitor, IWheelCounter wheels, PulseSettings pulses)
        {
            _body = body;
            _monitor = monitor;
            _wheels = wheels;
            _pulses = pulses;
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

        /// <summary>What the wheel counters said when the motors stopped and at the end (after the slide)</summary>
        private class Counts
        {
            public int AtStopRight;
            public int AtStopLeft;
            public int FinalRight;
            public int FinalLeft;
        }

        private void Move(ValidOrder order, CancellationToken cancel, TaskCompletionSource<Outcome> done)
        {
            string reason = "tiempo";
            var clock = Stopwatch.StartNew();
            var counts = new Counts();
            Outcome? failure = null;

            try
            {
                // The counters start before the motors, in every movement
                _wheels.Start();
                reason = order.ByPulses ? MoveByPulses(order, cancel, clock, counts) : MoveByTime(order, cancel, clock);
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

                    try
                    {
                        _wheels.Stop();
                    }
                    catch (Exception)
                    {
                        // Nothing to count any more
                    }

                    counts.FinalRight = _wheels.Right;
                    counts.FinalLeft = _wheels.Left;
                    _current = null;
                    _cancel = null;
                }
            }

            if (failure is not null)
            {
                done.SetResult(failure);
                return;
            }

            var result = new Dictionary<string, object>
            {
                ["estado"] = "terminada",
                ["motivo"] = reason,
                ["segundosReales"] = Math.Round(clock.Elapsed.TotalSeconds, 2),
                ["ajustes"] = order.Adjustments,
                ["pulsos"] = new { derecha = counts.FinalRight, izquierda = counts.FinalLeft },
            };

            if (order.ByPulses)
            {
                // What the robot slid after the motors stopped, and what it really did (to calibrate the brake)
                result["resbalon"] = new { derecha = counts.FinalRight - counts.AtStopRight, izquierda = counts.FinalLeft - counts.AtStopLeft };
                double pulses = (counts.FinalRight + counts.FinalLeft) / 2.0;
                if (order.Name == "girar")
                {
                    result["grados"] = Math.Round(pulses * Geometry.DegreesPerPulse);
                }
                else
                {
                    result["centimetros"] = Math.Round(pulses * Geometry.CmPerPulse, 1);
                }
            }

            done.SetResult(new Outcome(200, result));
        }

        private string MoveByTime(ValidOrder order, CancellationToken cancel, Stopwatch clock)
        {
            string reason = "tiempo";

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

            return reason;
        }

        /// <summary>
        /// Goes some centimeters or turns some degrees, counting the pulses of the wheels like lesson 10 (cm = pulses * 1.02,
        /// degrees = pulses * 9). Forwards, both wheels go at the same time and the speed of each one is corrected so the robot
        /// keeps straight. Backwards and turning, each wheel stops by itself when it has counted its pulses. The reason is
        /// "distancia" or "angulo" when it arrived; "tiempo_agotado", "sin_pulsos", "ruedas_distintas", "obstaculo", "sensores"
        /// or "parada" when it did not. Every write to the motors takes the lock and looks at the stop inside it, so a write
        /// can never come after /parar.
        /// </summary>
        private string MoveByPulses(ValidOrder order, CancellationToken cancel, Stopwatch clock, Counts counts)
        {
            bool forwards = order.Name == "avanzar";
            bool turn = order.Name == "girar";
            double speed = _pulses.Speed;
            if (order.SpeedGiven && order.Speed != speed && !order.Adjustments.Contains("velocidad"))
            {
                // The brake is calibrated for one speed: the speed that the brain asked for is not used
                order.Adjustments.Add("velocidad");
            }

            double centimeters = turn ? order.Degrees / 360.0 * Geometry.CmPerRobotTurn : order.Centimeters;
            int target = Math.Max(1, (int)Math.Round(centimeters / Geometry.CmPerPulse));
            int brake = turn ? _pulses.BrakeTurn : forwards ? _pulses.BrakeForwards : _pulses.BrakeBackwards;
            int stopAt = Math.Max(1, target - brake);

            // Motor One is the right wheel and Two the left one: turning right is One backwards and Two forwards
            double rightSign = forwards ? 1 : order.Name == "retroceder" ? -1 : order.Direction == "derecha" ? -1 : 1;
            double leftSign = forwards ? 1 : order.Name == "retroceder" ? -1 : order.Direction == "derecha" ? 1 : -1;

            string? reason = null;
            lock (_lock)
            {
                if (cancel.IsCancellationRequested)
                {
                    reason = "parada";
                }
                else if (forwards && _monitor.Way != WayState.Clear)
                {
                    reason = _monitor.Way == WayState.NoData ? "sensores" : "obstaculo";
                }
                else
                {
                    _body.SetMotors(rightSign * speed, leftSign * speed);
                }
            }

            bool rightRunning = true;
            bool leftRunning = true;
            int lastRight = 0;
            int lastLeft = 0;
            long lastRightPulse = clock.ElapsedMilliseconds;
            long lastLeftPulse = clock.ElapsedMilliseconds;

            while (reason is null)
            {
                if (cancel.WaitHandle.WaitOne(PulseStep))
                {
                    reason = "parada";
                    break;
                }

                if (forwards && _monitor.Way != WayState.Clear)
                {
                    reason = _monitor.Way == WayState.NoData ? "sensores" : "obstaculo";
                    break;
                }

                if (clock.Elapsed.TotalSeconds >= order.Seconds)
                {
                    reason = "tiempo_agotado";
                    break;
                }

                int right = _wheels.Right;
                int left = _wheels.Left;
                long now = clock.ElapsedMilliseconds;
                if (right != lastRight)
                {
                    lastRight = right;
                    lastRightPulse = now;
                }

                if (left != lastLeft)
                {
                    lastLeft = left;
                    lastLeftPulse = now;
                }

                if (forwards)
                {
                    if ((right + left) / 2 >= stopAt)
                    {
                        reason = "distancia";
                        break;
                    }

                    // The wheel that is ahead goes slower and the other one faster. A big difference means a sensor that does not count.
                    int difference = right - left;
                    if (Math.Abs(difference) > MaxDifference)
                    {
                        reason = "ruedas_distintas";
                        break;
                    }

                    if (now - lastRightPulse > StallTime || now - lastLeftPulse > StallTime)
                    {
                        reason = "sin_pulsos";
                        break;
                    }

                    double rightSpeed = Math.Clamp(speed - difference * Correction, 0.0, 1.0);
                    double leftSpeed = Math.Clamp(speed + difference * Correction, 0.0, 1.0);
                    if (!Drive(cancel, rightSpeed, leftSpeed))
                    {
                        reason = "parada";
                        break;
                    }
                }
                else
                {
                    // Backwards and turning: each wheel stops when it has counted its pulses. A wheel that stopped does not count any more.
                    bool changed = false;
                    if (rightRunning && right >= stopAt)
                    {
                        rightRunning = false;
                        changed = true;
                    }

                    if (leftRunning && left >= stopAt)
                    {
                        leftRunning = false;
                        changed = true;
                    }

                    if (!rightRunning && !leftRunning)
                    {
                        reason = turn ? "angulo" : "distancia";
                        break;
                    }

                    if ((rightRunning && now - lastRightPulse > StallTime) || (leftRunning && now - lastLeftPulse > StallTime))
                    {
                        reason = "sin_pulsos";
                        break;
                    }

                    if (changed && !Drive(cancel, rightRunning ? rightSign * speed : 0.0, leftRunning ? leftSign * speed : 0.0))
                    {
                        reason = "parada";
                        break;
                    }
                }
            }

            // The motors stop now (zero is always safe). Then, if the robot arrived, the slide is counted before the robot is free again.
            lock (_lock)
            {
                _body.SetMotors(0.0, 0.0);
            }

            counts.AtStopRight = _wheels.Right;
            counts.AtStopLeft = _wheels.Left;
            if (reason is "distancia" or "angulo")
            {
                cancel.WaitHandle.WaitOne(SlideTime);
            }

            return reason ?? "parada";
        }

        // Writes the speed of both wheels. After /parar nothing is written: it returns false.
        private bool Drive(CancellationToken cancel, double right, double left)
        {
            lock (_lock)
            {
                if (cancel.IsCancellationRequested)
                {
                    return false;
                }

                _body.SetMotors(right, left);
                return true;
            }
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
