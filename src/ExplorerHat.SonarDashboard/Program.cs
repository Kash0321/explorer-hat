using Spectre.Console;
using Spectre.Console.Rendering;

namespace ExplorerHat.SonarDashboard
{
    /// <summary>
    /// Console dashboard with the distance measured by every HC-SR04 sensor. Motors are not used.
    /// Useful to check which sensors are wired correctly.
    /// The F key turns on and off the filter for wrong readings (the same one ExplorerHat.ObstacleAvoidance uses).
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
        // Whether the dashboard shows the filtered distance (F key)
        static bool _filterOn = true;

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

                        // F turns the filter on and off
                        while (Console.KeyAvailable)
                        {
                            if (Console.ReadKey(true).Key == ConsoleKey.F)
                            {
                                _filterOn = !_filterOn;
                            }
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
                writer.WriteLine("hora;sensor;ok;cm;filtrada;ms");
                var end = DateTime.Now.AddSeconds(seconds);

                while (_running && DateTime.Now < end)
                {
                    foreach (var sensor in sensors)
                    {
                        sensor.Measure();
                        var centimeters = sensor.LastReadingOk ? $"{sensor.LastDistance:0.0}" : "";
                        writer.WriteLine($"{DateTime.Now:HH:mm:ss.fff};{sensor.Name};{sensor.LastReadingOk};{centimeters};{sensor.FilteredDistance:0.0};{sensor.LastReadingTime:0.0}");
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
                .Caption($"[grey]{DateTime.Now:HH:mm:ss} · Filtro {FilterText()} (F para cambiar) · Ctrl+C para salir[/]");

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

        static string FilterText()
        {
            if (_filterOn)
            {
                return "[green]activado[/]";
            }

            return "[yellow]desactivado[/]";
        }

        /// <summary>
        /// Distance shown for the sensor: the filtered one when the filter is on, the latest reading when it is off
        /// </summary>
        static double? ShownDistance(SonarSensor sensor)
        {
            if (_filterOn)
            {
                return sensor.FilteredDistance;
            }

            if (sensor.LastReadingOk)
            {
                return sensor.LastDistance;
            }

            // The latest reading failed
            return null;
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
            var distance = ShownDistance(sensor);

            if (distance is null)
            {
                if (sensor.LastDistance is null)
                {
                    return "[grey]—[/]";
                }

                // Old value in grey: the latest reading failed
                return $"[grey]{sensor.LastDistance:0.0} cm[/]";
            }

            return $"[{DistanceColor(distance.Value)}]{distance:0.0} cm[/]";
        }

        static string DistanceBar(SonarSensor sensor)
        {
            var distance = ShownDistance(sensor);

            if (distance is null)
            {
                return $"[grey]{new string('░', BAR_WIDTH)}[/]";
            }

            var filled = (int)Math.Round(Math.Min(distance.Value, BAR_MAX_CM) / BAR_MAX_CM * BAR_WIDTH);
            var color = DistanceColor(distance.Value);

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
