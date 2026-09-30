using Spectre.Console;
using Spectre.Console.Rendering;

namespace ExplorerHat.SonarDashboard
{
    /// <summary>
    /// Console dashboard with the distance measured by every HC-SR04 sensor. Motors are not used.
    /// Useful to check which sensors are wired correctly.
    /// With "--registro <file> [seconds]" there is no dashboard: every reading is saved to a CSV file,
    /// to study failed or wrong readings.
    /// </summary>
    class Program
    {
        // Distance drawn as a full bar, and bar size
        const double BAR_MAX_CM = 200;
        const int BAR_WIDTH = 15;

        // Colors: closer than 20 cm is red, closer than 50 cm is yellow
        const double NEAR_CM = 20;
        const double MEDIUM_CM = 50;

        // A sensor "works" when at least 75% of its latest readings are right
        const double WORKS_RATIO = 0.75;

        static volatile bool _running = true;

        static void Main(string[] args)
        {
            // Ctrl+C ends the loop, so the sensor pins are released before exiting
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _running = false;
            };

            // Same wiring as ExplorerHat.ObstacleAvoidance
            var sensors = new List<SonarSensor>
            {
                new SonarSensor("Izquierda", triggerPin: 13, triggerLabel: "OUT3", echoPin: 24, echoLabel: "IN3"),
                new SonarSensor("Centro", triggerPin: 6, triggerLabel: "OUT1", echoPin: 23, echoLabel: "IN1"),
                new SonarSensor("Derecha", triggerPin: 12, triggerLabel: "OUT2", echoPin: 22, echoLabel: "IN2"),
            };

            try
            {
                if (args.Length >= 2 && args[0] == "--registro")
                {
                    int seconds = args.Length >= 3 ? int.Parse(args[2]) : 60;
                    Record(sensors, args[1], seconds);
                    return;
                }

                AnsiConsole.Live(BuildDashboard(sensors)).Start(context =>
                {
                    while (_running)
                    {
                        foreach (var sensor in sensors)
                        {
                            sensor.Measure();
                            // Wait for the echoes to fade away before using the next sensor
                            Thread.Sleep(60);
                        }

                        context.UpdateTarget(BuildDashboard(sensors));
                    }
                });
            }
            finally
            {
                foreach (var sensor in sensors)
                {
                    sensor.Dispose();
                }
            }

            AnsiConsole.MarkupLine("[grey]Sensores liberados. ¡Hasta luego![/]");
        }

        /// <summary>
        /// Measures like the dashboard does, but saves every reading to a CSV file:
        /// time, sensor, whether it worked, distance in centimeters and how long it took in milliseconds
        /// </summary>
        static void Record(List<SonarSensor> sensors, string path, int seconds)
        {
            Console.WriteLine($"Registrando {seconds} s en {path} (Ctrl+C para terminar antes)...");

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("hora;sensor;ok;cm;ms");
                var end = DateTime.Now.AddSeconds(seconds);

                while (_running && DateTime.Now < end)
                {
                    foreach (var sensor in sensors)
                    {
                        sensor.Measure();
                        var centimeters = sensor.LastReadingOk ? $"{sensor.LastDistance:0.0}" : "";
                        writer.WriteLine($"{DateTime.Now:HH:mm:ss.fff};{sensor.Name};{sensor.LastReadingOk};{centimeters};{sensor.LastReadingTime:0.0}");
                        // Wait for the echoes to fade away before using the next sensor
                        Thread.Sleep(60);
                    }
                }
            }

            Console.WriteLine("Registro terminado.");
        }

        static IRenderable BuildDashboard(List<SonarSensor> sensors)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[bold]Sensores de distancia HC-SR04[/]")
                .Caption($"[grey]{DateTime.Now:HH:mm:ss} · Ctrl+C para salir[/]");

            table.AddColumn("Sensor");
            table.AddColumn("TRIG → ECHO");
            table.AddColumn(new TableColumn("Distancia").RightAligned().NoWrap());
            table.AddColumn(new TableColumn($"0 – {BAR_MAX_CM / 100} m").NoWrap());
            table.AddColumn(new TableColumn("Estado").NoWrap());
            table.AddColumn(new TableColumn("OK").RightAligned().NoWrap());

            foreach (var sensor in sensors)
            {
                table.AddRow(
                    $"[bold]{sensor.Name}[/]",
                    $"{sensor.TriggerLabel} → {sensor.EchoLabel}\n[grey]GPIO {sensor.TriggerPin} → {sensor.EchoPin}[/]",
                    DistanceText(sensor),
                    DistanceBar(sensor),
                    StatusText(sensor),
                    $"{sensor.ReadingsOk}/{sensor.Readings}");
            }

            var legend = new Markup(
                $"[red]■[/] menos de {NEAR_CM} cm   [yellow]■[/] menos de {MEDIUM_CM} cm   [green]■[/] más lejos\n" +
                "[grey]Pines libres del HAT: OUT4 (GPIO 16) e IN4 (GPIO 25)[/]");

            return new Rows(table, legend);
        }

        static string DistanceColor(double centimeters)
        {
            if (centimeters < NEAR_CM)
            {
                return "red";
            }
            else if (centimeters < MEDIUM_CM)
            {
                return "yellow";
            }
            else
            {
                return "green";
            }
        }

        static string DistanceText(SonarSensor sensor)
        {
            if (sensor.LastDistance is null)
            {
                return "[grey]—[/]";
            }

            var text = $"{sensor.LastDistance:0.0} cm";

            if (sensor.LastReadingOk)
            {
                return $"[{DistanceColor(sensor.LastDistance.Value)}]{text}[/]";
            }

            // Old value in grey: the latest reading failed
            return $"[grey]{text}[/]";
        }

        static string DistanceBar(SonarSensor sensor)
        {
            if (sensor.LastDistance is null || !sensor.LastReadingOk)
            {
                return $"[grey]{new string('░', BAR_WIDTH)}[/]";
            }

            var distance = Math.Min(sensor.LastDistance.Value, BAR_MAX_CM);
            var filled = (int)Math.Round(distance / BAR_MAX_CM * BAR_WIDTH);
            var color = DistanceColor(sensor.LastDistance.Value);

            return $"[{color}]{new string('█', filled)}[/][grey]{new string('░', BAR_WIDTH - filled)}[/]";
        }

        static string StatusText(SonarSensor sensor)
        {
            if (sensor.Readings == 0)
            {
                return "[grey]Esperando…[/]";
            }
            else if (sensor.ReadingsOk == 0)
            {
                return "[red]✖ Sin respuesta[/]";
            }
            else if (sensor.ReadingsOk >= sensor.Readings * WORKS_RATIO)
            {
                return "[green]✔ Funciona[/]";
            }
            else
            {
                return "[yellow]⚠ Falla a veces[/]";
            }
        }
    }
}
