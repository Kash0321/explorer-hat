using System;
using System.Device.Gpio;
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
        const string LOG_PWR_MSG = "Motors at {pwr}%";

        static bool _running;

        static Runner()
        {
            _running = false;
        }

        public static async Task RunAsync()
        {
            await Task.Run(() => { 
                try
                {
                    _running = true;
                    using (var hat = new Iot.Device.ExplorerHat.ExplorerHat(new GpioController(PinNumberingScheme.Logical)))
                    {
                        using (var sonar = new Sonar())
                        {
                            Log.Debug("Time to settle sonar and motors!");
                            Thread.Sleep(1000);
                            Log.Debug("GO!");
                            Log.Debug(LOG_PWR_MSG, 80);
                            hat.Motors.Forwards(0.8);
                            while (_running)
                            {
                                var distance = sonar.Distance;
                                Log.Information("Distance to the nearest obstacle: {distance} cm.", Math.Round(distance, 4, MidpointRounding.AwayFromZero));

                                if (distance < 20d)
                                {
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.On();

                                    //Maniobras para evitar un obstáculo
                                    Log.Debug("Obstacle detected. Maneuvering to avoid it...");
                                    hat.Motors.Stop();
                                    Log.Debug("Motors stopped");
                                    hat.Motors.Backwards(1);
                                    Log.Debug("Backwards...");
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    Log.Debug("Detouring...");
                                    hat.Motors.One.Forwards(1);
                                    hat.Motors.Two.Backwards(1);
                                    Thread.Sleep(TimeSpan.FromSeconds(0.35));
                                    Log.Debug("Detour completed");
                                    Log.Debug(LOG_PWR_MSG, 80);
                                    hat.Motors.Forwards(0.8);
                                }
                                else if (distance < 50d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 30);
                                    hat.Motors.Forwards(0.3);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.Off();
                                }
                                else if (distance < 80d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 40);
                                    hat.Motors.Forwards(0.4);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else if (distance < 110d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 60);
                                    hat.Motors.Forwards(0.6);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else
                                {
                                    Log.Debug(LOG_PWR_MSG, 80);
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

                    Log.Debug("Hat disposed");
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            });
        }

        public static void Stop()
        {
            _running = false;
        }
    }
}