using Spectre.Console;
using Spectre.Console.Rendering;

namespace ExplorerHat.SonarDashboard
{
    /// <summary>
    /// Console dashboard with the distance measured by every HC-SR04 sensor. Motors are not used.
    /// Useful to check which sensors are wired correctly.
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
