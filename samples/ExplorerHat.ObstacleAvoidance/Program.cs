using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExplorerHat.ObstacleAvoidance
{
    class Program
    {
        static void Main(string[] args)
        {
            //Logging configuration
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
                var distance = SonarSingleton.Distance;

                Log.Information("Medición de distancia en progreso...");

                Log.Information("Arrancamos! Motores al 70%");
                hat.Motors.Forwards(0.7);
                while (true)
                {
                    distance = SonarSingleton.Distance;

                    if (distance < 40d)
                    {
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.On();
                        hat.Lights.Four.On();

                        //Maniobras para evitar un obstáculo
                        Log.Information("Obstáculo detectado. Ejecutando maniobras para evitarlo...");
                        hat.Motors.Stop();
                        Log.Information("Motores parados");
                        hat.Motors.Backwards(0.6);
                        Log.Information("Atrás...");
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        Log.Information("Girando...");
                        hat.Motors.One.Forwards(0.6);
                        hat.Motors.Two.Backwards(0.6);
                        Thread.Sleep(TimeSpan.FromSeconds(0.75));
                        Log.Information("Giro completado");
                        Log.Information("Motores al 70%");
                        hat.Motors.Forwards(0.7);
                    }
                    else if (distance < 60d)
                    {
                        Log.Information("Motores al 65%");
                        hat.Motors.Forwards(0.65);
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.On();
                        hat.Lights.Four.Off();
                    }
                    else if (distance < 120d)
                    {
                        Log.Information("Motores al 70%");
                        hat.Motors.Forwards(0.70);
                        hat.Lights.One.On();
                        hat.Lights.Two.On();
                        hat.Lights.Three.Off();
                        hat.Lights.Four.Off();
                    }
                    else if (distance < 240d)
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

                    Log.Information($"Distancia al obstáculo más cercano: {distance} cm.");
                    Thread.Sleep(TimeSpan.FromSeconds(0.2));
                }
            }
        }
    }
}
