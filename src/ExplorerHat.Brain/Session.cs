using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading.Channels;

namespace ExplorerHat.Brain
{
    /// <summary>
    /// Reads orders from the keyboard, asks the model for a plan and sends the actions to the robot one by one.
    /// A second thread reads the keyboard all the time: a stop word stops the robot even while the model thinks or while an
    /// action runs. Every stop adds one to a counter, and a plan that finds the counter changed is dropped.
    /// </summary>
    public class Session
    {
        // A movement without "segundos" gets this many seconds (the service needs the value; the 3B model leaves it out sometimes)
        private const double DefaultSeconds = 1.0;

        // A model that loops must not send an endless plan
        private const int MaxActions = 12;

        private readonly LlmClient _llm;
        private readonly RobotClient? _robot;
        private readonly object _sayLock = new object();
        private readonly List<Task> _stops = new List<Task>();
        private int _stopCount;

        public Session(Settings settings)
        {
            _llm = new LlmClient(settings.Servers, settings.NumCtx, Say);
            _robot = settings.OnlyShow ? null : new RobotClient(settings.RobotUrl, settings.Key);
        }

        public void Say(string text)
        {
            lock (_sayLock)
            {
                Console.WriteLine(text);
            }
        }

        /// <summary>Called on Ctrl+C: the action in progress would run to its end otherwise</summary>
        public void EmergencyStop()
        {
            // Twice, like a stop word: an action that was already on its way would start after the first stop
            Interlocked.Increment(ref _stopCount);
            _robot?.StopAsync().Wait(3000);
            Thread.Sleep(300);
            _robot?.StopAsync().Wait(3000);
        }

        public async Task RunAsync()
        {
            var orders = Channel.CreateUnbounded<(string Text, int StopsWhenRead)>();
            var keyboard = new Thread(() => ReadKeyboard(orders.Writer)) { IsBackground = true };
            keyboard.Start();

            await foreach (var order in orders.Reader.ReadAllAsync())
            {
                await HandleAsync(order.Text, order.StopsWhenRead);
            }

            Task[] pending;
            lock (_stops)
            {
                pending = _stops.ToArray();
            }

            await Task.WhenAll(pending);
        }

        // The keyboard thread. It never waits for the model or the robot, so a stop word always gets through.
        private void ReadKeyboard(ChannelWriter<(string, int)> orders)
        {
            string? line;
            while ((line = Console.In.ReadLine()) != null)
            {
                string text = line.Trim();
                if (text == "")
                {
                    continue;
                }

                if (StopRule.IsStop(text))
                {
                    // First the counter, then the stop: a plan that is about to send an action sees the change
                    Interlocked.Increment(ref _stopCount);
                    Say($"PARADA: {text}");
                    StartStop();
                }
                else
                {
                    orders.TryWrite((text, Volatile.Read(ref _stopCount)));
                }
            }

            orders.Complete();
        }

        // Stops the robot now and once more 300 ms later: an action that was already on its way when the first stop arrived would
        // start after it. The stop is safe to repeat.
        private void StartStop()
        {
            if (_robot is null)
            {
                return;
            }

            var task = Task.Run(async () =>
            {
                await _robot.StopAsync();
                await Task.Delay(300);
                await _robot.StopAsync();
            });
            lock (_stops)
            {
                _stops.Add(task);
            }
        }

        private bool Stopped(int stopsWhenRead) => Volatile.Read(ref _stopCount) != stopsWhenRead;

        private async Task HandleAsync(string order, int stopsWhenRead)
        {
            if (Stopped(stopsWhenRead))
            {
                Say($"Orden descartada por la parada: {order}");
                return;
            }

            Say($"Orden: {order}");
            var reply = await _llm.AskAsync(order);
            if (reply is null)
            {
                Say("  Ningún servidor de modelos responde: el robot no se mueve.");
                return;
            }

            Say($"  (modelo {reply.Server.Model} en {reply.Server.Url})");
            if (reply.Text != "")
            {
                Say($"  Modelo: {reply.Text}");
            }

            if (reply.Calls.Count == 0)
            {
                Say("  El modelo no propone ninguna acción.");
                return;
            }

            var calls = reply.Calls;
            if (calls.Count > MaxActions)
            {
                Say($"  El modelo propone {calls.Count} acciones: hago solo las {MaxActions} primeras.");
                calls = calls.Take(MaxActions).ToList();
            }

            foreach (var call in calls)
            {
                if (Stopped(stopsWhenRead))
                {
                    Say("  Plan cancelado por la parada.");
                    return;
                }

                var args = (JsonObject)call.Args.DeepClone();
                bool movement = call.Name is "avanzar" or "retroceder" or "girar";
                if (movement && !args.ContainsKey("segundos"))
                {
                    args["segundos"] = DefaultSeconds;
                    Say($"  Falta 'segundos': pongo {DefaultSeconds.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',')} s.");
                }

                Say($"  -> {Describe(call.Name, args)}");
                if (_robot is null)
                {
                    continue;
                }

                var result = await _robot.ActionAsync(call.Name, args);
                if (Stopped(stopsWhenRead))
                {
                    await _robot.StopAsync();
                    Say("  Plan cancelado por la parada.");
                    return;
                }

                if (!Report(result))
                {
                    return;
                }
            }
        }

        private static string Describe(string name, JsonObject args)
        {
            return $"{name}({string.Join(", ", args.Select(pair => pair.Key + "=" + (pair.Value?.ToJsonString() ?? "null")))})";
        }

        // Says what the robot answered. Returns false if the plan must stop here.
        private bool Report(RobotReply result)
        {
            string? error = result.Body?["error"]?.ToString();
            if (result.Status == 401)
            {
                Say("     El robot rechaza la clave (401). Plan cancelado.");
                return false;
            }

            if (result.Status != 200)
            {
                Say($"     El robot no lo hizo ({(result.Status == 0 ? "sin respuesta" : result.Status.ToString())}): {error}. Plan cancelado.");
                return false;
            }

            string state = result.Body?["estado"]?.ToString() ?? "";
            if (state != "terminada")
            {
                Say("     hecho");
                return true;
            }

            string reason = result.Body?["motivo"]?.ToString() ?? "";
            string seconds = (result.Body?["segundosReales"]?.GetValue<double>() ?? 0).ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',');
            string adjustments = result.Body?["ajustes"] is JsonArray list && list.Count > 0
                ? $" (el robot ajustó: {string.Join(", ", list.Select(item => item?.ToString()))})"
                : "";
            switch (reason)
            {
                case "tiempo":
                    Say($"     terminó a los {seconds} s{adjustments}");
                    return true;
                case "obstaculo":
                    Say($"     se paró por un obstáculo a los {seconds} s{adjustments}. Plan cancelado.");
                    return false;
                case "sensores":
                    Say($"     se paró porque los sensores de distancia dejaron de dar datos, a los {seconds} s. Plan cancelado.");
                    return false;
                default:
                    Say($"     se paró (motivo: {reason}) a los {seconds} s. Plan cancelado.");
                    return false;
            }
        }
    }
}
