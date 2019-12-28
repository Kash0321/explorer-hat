using Serilog;
using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExplorerHat.ObstacleAvoidance
{
    class Program
    {
        const int TRIG = 6;
        const int ECHO = 23;
        static void Main(string[] args)
        {
            // Logging configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            // Priting OS INFO
            Console.WriteLine("**************************************************************************************");
            Console.WriteLine($"   Framework: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"          OS: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"     OS Arch: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"    CPU Arch: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine("**************************************************************************************");

            using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
            {
                var pulseStart = DateTime.MinValue;
                var pulseEnd = DateTime.MinValue;
                var pulseDuration = 0d;
                var distance = 0d;
                var gpio = Iot.Device.ExplorerHat.Gpio.GpioController.Current;

                Log.Information("Medición de distancia en progreso...");

                Iot.Device.ExplorerHat.Gpio.GpioController.EnsureOpenPin(TRIG, PinMode.Output);
                Iot.Device.ExplorerHat.Gpio.GpioController.EnsureOpenPin(ECHO, PinMode.Input);

                Log.Information("Esperando a que el sensor se inicialice...");

                Thread.Sleep(1000);

                Log.Information("Arrancamos! Motores al 70%");
                hat.Motors.Forwards(0.7);

                while (true)
                {
                    gpio.Write(TRIG, PinValue.High);
                    Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
                    gpio.Write(TRIG, PinValue.Low);

                    // while (gpio.Read(ECHO) == 0)
                    // {
                    //    pulseStart = DateTime.Now;
                    // }

                    // while (gpio.Read(ECHO) == 1)
                    // {
                    //    pulseEnd = DateTime.Now;
                    // }

                    pulseDuration = (pulseEnd - pulseStart).TotalMilliseconds;

                    distance = Math.Round(pulseDuration * 17150, 2, MidpointRounding.AwayFromZero);

                    if (distance < 40d)
                    {
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.On();
                        hat.Lights.Four.On();

                        // Maniobras para evitar un obstáculo
                        // Log.Information("Obstáculo detectado. Ejecutando maniobras para evitarlo...");
                        // hat.Motors.Stop();
                        // Log.Information("Motores parados");
                        // hat.Motors.Backwards(0.6);
                        // Log.Information("Atrás...");
                        // Thread.Sleep(TimeSpan.FromMilliseconds(0.5));
                        // Log.Information("Girando...");
                        // hat.Motors.One.Forwards(0.6);
                        // hat.Motors.Two.Backwards(0.6);
                        // Thread.Sleep(TimeSpan.FromMilliseconds(0.75));
                        // Log.Information("Giro completado");
                        // Log.Information("Motores al 70%");
                        // hat.Motors.Forwards(0.7);
                    }
                    else if (distance < 60d )
                    {
                        Log.Information("Motores al 65%");
                        hat.Motors.Forwards(0.65);
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.On();
                        hat.Lights.Four.Off();
                    }
                    else if (distance < 120d )
                    {
                        Log.Information("Motores al 70%");
                        hat.Motors.Forwards(0.70);
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.Off();
                        hat.Lights.Four.Off();
                    }
                    else if (distance < 240d )
                    {
                        Log.Information("Motores al 75%");
                        hat.Motors.Forwards(0.75);
                        hat.Lights.One.On();
                        hat.Lights.Two.Off();
                        hat.Lights.Three.Off();
                        hat.Lights.Four.Off();
                    }
                    else
                    {
                        Log.Information("Motores al 80%");
                        hat.Motors.Forwards(0.80);
                        hat.Lights.One.Off();
                        hat.Lights.Two.Off();
                        hat.Lights.Three.Off();
                        hat.Lights.Four.Off();
                    }

                    Log.Information($"Distancia al obstáculo más cercano: {distance} cm [{pulseStart.ToShortDateString()} {pulseStart.ToShortTimeString()}/{pulseEnd.ToShortDateString()} {pulseEnd.ToShortTimeString()}]");
                    Thread.Sleep(TimeSpan.FromMilliseconds(0.2));
                }
            }
        }
    }
}
