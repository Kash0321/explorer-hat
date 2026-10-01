using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ExplorerHat.UpsDashboard
{
    /// <summary>
    /// Console dashboard with everything the Waveshare UPS HAT (B) can tell (batteries voltage, current, power and charge),
    /// and the power state of the Raspberry Pi. Motors are not used.
    /// </summary>
    class Program
    {
        // Time between two readings, in milliseconds
        const int REFRESH_TIME = 500;
        const int BAR_WIDTH = 20;

        // Below this current (in amperes, with or without sign) the batteries are neither charging nor discharging
        const double NO_CURRENT = 0.02;

        static volatile bool _running = true;

        // Lowest battery voltage and highest current taken from the batteries since the dashboard started
        static double? _lowestVoltage = null;
        static double? _highestDischarge = null;

        static void Main(string[] args)
        {
            // Ctrl+C ends the loop, so the I2C connection is released before exiting
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _running = false;
            };

            using (var ups = new UpsHat())
            {
                AnsiConsole.Live(new Text("Leyendo la UPS...")).Start(context =>
                {
                    while (_running)
                    {
                        string? error = null;
                        try
                        {
                            ups.Read();
                            SaveLowestAndHighest(ups);
                        }
                        catch (IOException ex)
                        {
                            error = ex.Message;
                        }

                        context.UpdateTarget(BuildDashboard(ups, error));
                        Thread.Sleep(REFRESH_TIME);
                    }
                });
            }

            AnsiConsole.MarkupLine("[grey]UPS liberada. ¡Hasta luego![/]");
        }

        static void SaveLowestAndHighest(UpsHat ups)
        {
            if (_lowestVoltage is null || ups.BatteryVoltage < _lowestVoltage)
            {
                _lowestVoltage = ups.BatteryVoltage;
            }

            double discharge = -ups.Current;
            if (_highestDischarge is null || discharge > _highestDischarge)
            {
                _highestDischarge = discharge;
            }
        }

        static IRenderable BuildDashboard(UpsHat ups, string? error)
        {
            var battery = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[bold]Waveshare UPS HAT (B)[/] [grey]· INA219 en I2C 0x42[/]")
                .HideHeaders();
            battery.AddColumn("Dato");
            battery.AddColumn(new TableColumn("Valor").NoWrap());

            if (error != null)
            {
                battery.AddRow("[red]✖ La UPS no responde[/]", $"[grey]{Markup.Escape(error)}[/]");
            }
            else
            {
                battery.AddRow("Baterías (2×18650)", $"[bold]{ups.BatteryVoltage:0.000} V[/]");
                battery.AddRow("Carga estimada", ChargeBar(ups.BatteryPercent));
                battery.AddRow("Corriente", $"{ups.Current:+0.000;-0.000;0.000} A");
                battery.AddRow("Potencia", $"{Math.Abs(ups.Power):0.00} W");
                battery.AddRow("Estado", StateText(ups.Current));
                battery.AddRow("[grey]Tensión mínima de la sesión[/]", $"[grey]{_lowestVoltage:0.000} V[/]");
                battery.AddRow("[grey]Descarga máxima de la sesión[/]", $"[grey]{Math.Max(0, _highestDischarge ?? 0):0.000} A[/]");
            }

            var registers = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[bold]Registros del INA219[/] [grey](datos en bruto)[/]");
            registers.AddColumn("Registro");
            registers.AddColumn(new TableColumn("Valor").NoWrap());
            registers.AddColumn("Significado");
            registers.AddRow("0x00 Configuración", $"0x{ups.ConfigurationRegister:X4}", "[grey]Rango, ganancia y muestras del chip[/]");
            registers.AddRow("0x01 Tensión en la resistencia", $"{ups.ShuntVoltageRegister}", $"{ups.ShuntVoltage:+0.00;-0.00;0.00} mV sobre 0,1 Ω");
            registers.AddRow("0x02 Tensión de las baterías", $"0x{ups.BusVoltageRegister:X4}",
                $"{ups.BatteryVoltage:0.000} V · lectura nueva: {YesNo(ups.ConversionReady)} · desbordamiento: {YesNo(ups.Overflow)}");

            var pi = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[bold]Raspberry Pi[/] [grey]· vcgencmd get_throttled[/]");
            pi.AddColumn("Dato");
            pi.AddColumn(new TableColumn("Ahora").Centered());
            pi.AddColumn(new TableColumn("Desde el arranque").Centered());

            int? throttled = ReadThrottled();
            if (throttled is null)
            {
                pi.AddRow("[grey]vcgencmd no disponible[/]", "", "");
            }
            else
            {
                pi.AddRow("Tensión baja", Alarm(throttled.Value, 0), Alarm(throttled.Value, 16));
                pi.AddRow("Frecuencia limitada", Alarm(throttled.Value, 1), Alarm(throttled.Value, 17));
                pi.AddRow("CPU ralentizada", Alarm(throttled.Value, 2), Alarm(throttled.Value, 18));
                pi.AddRow("Límite de temperatura", Alarm(throttled.Value, 3), Alarm(throttled.Value, 19));
                pi.AddRow("[grey]Valor[/]", $"[grey]0x{throttled.Value:X}[/]", "");
            }
            pi.AddRow("Temperatura de la CPU", TemperatureText(), "");

            var caption = new Markup($"[grey]{DateTime.Now:HH:mm:ss} · Ctrl+C para salir · " +
                "Corriente positiva: las baterías se cargan; negativa: se descargan[/]");

            return new Rows(battery, registers, pi, caption);
        }

        static string ChargeBar(double percent)
        {
            var filled = (int)Math.Round(percent / 100 * BAR_WIDTH);
            string color;
            if (percent < 20)
            {
                color = "red";
            }
            else if (percent < 50)
            {
                color = "yellow";
            }
            else
            {
                color = "green";
            }

            return $"[{color}]{new string('█', filled)}[/][grey]{new string('░', BAR_WIDTH - filled)}[/] {percent:0} %";
        }

        static string StateText(double current)
        {
            if (current > NO_CURRENT)
            {
                return "[green]⚡ Cargando[/]";
            }
            else if (current < -NO_CURRENT)
            {
                return "[yellow]🔋 Descargando (la Pi funciona con las baterías)[/]";
            }
            else
            {
                return "[grey]Casi sin corriente (cargador conectado y baterías llenas)[/]";
            }
        }

        static string YesNo(bool value)
        {
            if (value)
            {
                return "sí";
            }

            return "no";
        }

        /// <summary>
        /// Text for one bit of get_throttled: red when it is on
        /// </summary>
        static string Alarm(int throttled, int bit)
        {
            if ((throttled & (1 << bit)) != 0)
            {
                return "[red]✖ sí[/]";
            }

            return "[green]✔ no[/]";
        }

        /// <summary>
        /// Value of "vcgencmd get_throttled" (null when the command is not available)
        /// </summary>
        static int? ReadThrottled()
        {
            try
            {
                var info = new ProcessStartInfo("vcgencmd", "get_throttled") { RedirectStandardOutput = true };
                using (var process = Process.Start(info)!)
                {
                    // The output is like "throttled=0x50000"
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return Convert.ToInt32(output.Split('=')[1], 16);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static string TemperatureText()
        {
            try
            {
                // In thousandths of a degree
                double temperature = int.Parse(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp").Trim()) / 1000.0;
                return $"{temperature:0.0} °C";
            }
            catch (Exception)
            {
                return "[grey]no disponible[/]";
            }
        }
    }
}
