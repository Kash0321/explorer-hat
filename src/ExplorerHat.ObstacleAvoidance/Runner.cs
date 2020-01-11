using System;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.ExplorerHat;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Obstacle avoidance runner
    /// </summary>
    public class Runner
    {
        const string LOG_PWR_MSG = "Motores al {pwr}%";

        static bool _running;

        static Runner()
        {
            _running = false;
        }

        public static async Task RunAsync()
        {
            try
            {
                _running = true;
                using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
                {
                    var distance = Sonar.Distance;

                    Log.Information("Arrancamos!");
                    Log.Information(LOG_PWR_MSG, 80);
                    hat.Motors.Forwards(0.8);
                    while (_running)
                    {
                        distance = Sonar.Distance;
                        Log.Information("Distancia al obstáculo más cercano: {distance} cm.", Math.Round(distance, 4, MidpointRounding.AwayFromZero));

                        if (distance < 20d)
                        {
                            hat.Lights.One.On();
                            hat.Lights.Two.On();
                            hat.Lights.Three.On();
                            hat.Lights.Four.On();

                            //Maniobras para evitar un obstáculo
                            Log.Information("Obstáculo detectado. Ejecutando maniobras para evitarlo...");
                            hat.Motors.Stop();
                            Log.Information("Motores parados");
                            hat.Motors.Backwards(1);
                            Log.Information("Atrás...");
                            Thread.Sleep(TimeSpan.FromSeconds(0.25));
                            Log.Information("Girando...");
                            hat.Motors.One.Forwards(1);
                            hat.Motors.Two.Backwards(1);
                            Thread.Sleep(TimeSpan.FromSeconds(0.35));
                            Log.Information("Giro completado");
                            Log.Information(LOG_PWR_MSG, 80);
                            hat.Motors.Forwards(0.8);
                        }
                        else if (distance < 50d)
                        {
                            Log.Information(LOG_PWR_MSG, 30);
                            hat.Motors.Forwards(0.3);
                            hat.Lights.One.On();
                            hat.Lights.Two.On();
                            hat.Lights.Three.On();
                            hat.Lights.Four.Off();
                        }
                        else if (distance < 80d)
                        {
                            Log.Information(LOG_PWR_MSG, 40);
                            hat.Motors.Forwards(0.4);
                            hat.Lights.One.On();
                            hat.Lights.Two.On();
                            hat.Lights.Three.Off();
                            hat.Lights.Four.Off();
                        }
                        else if (distance < 110d)
                        {
                            Log.Information(LOG_PWR_MSG, 60);
                            hat.Motors.Forwards(0.6);
                            hat.Lights.One.On();
                            hat.Lights.Two.Off();
                            hat.Lights.Three.Off();
                            hat.Lights.Four.Off();
                        }
                        else
                        {
                            Log.Information(LOG_PWR_MSG, 80);
                            hat.Motors.Forwards(0.80);
                            hat.Lights.One.Off();
                            hat.Lights.Two.Off();
                            hat.Lights.Three.Off();
                            hat.Lights.Four.Off();
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static void Stop()
        {
            _running = false;
        }
    }
}