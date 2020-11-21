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
        const string LOG_PWR_MSG = "Motors at {pwr}%";

        static bool _running;

        static Runner()
        {
            _running = false;
        }

        /// <summary>
        /// Executes the task managed by the runner, asynchronously
        /// </summary>
        /// <returns>Task managed</returns>
        public static async Task RunAsync()
        {
            await Task.Run(() => { 
                try
                {
                    _running = true;
                    using (var hat = new Iot.Device.ExplorerHat.ExplorerHat())
                    {
                        using (var sonar = new Sonar())
                        {
                            Log.Debug("Settling sonar devices and motors!");
                            Thread.Sleep(1000);
                            Log.Debug("GO!");
                            Log.Debug(LOG_PWR_MSG, 80);
                            hat.Motors.Forwards(0.8);
                            while (_running)
                            {
                                Log.Information("Distance to the nearest obstacle: Left {leftDistance} cm. Center {centerDistance} cm. Right {rightDistance} cm.", 
                                    sonar.Distance.LeftDistance,
                                    sonar.Distance.CenterDistance,
                                    sonar.Distance.RightDistance);

                                if (sonar.Distance.MinimumDistance.Value < 20d)
                                {
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.On();

                                    Log.Debug("Obstacle detected. Maneuvering to avoid it...");
                                    hat.Motors.Stop();
                                    Log.Debug("Motors stopped");
                                    hat.Motors.Backwards(0.6);
                                    Log.Debug("Backwards...");
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    Log.Debug("Turning to avoid the obstacle ...");

                                    if (sonar.Distance.LeftDistance <= sonar.Distance.RightDistance)
                                    {
                                        while (sonar.Distance.LeftDistance <= 20d)
                                        {
                                            hat.Motors.One.Forwards(0.6);
                                            hat.Motors.Two.Backwards(0.6);
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }
                                    else
                                    {
                                        while (sonar.Distance.RightDistance <= 20d)
                                        {
                                            hat.Motors.One.Backwards(0.6);
                                            hat.Motors.Two.Forwards(0.6);
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }


                                    Log.Debug("Turn completed");
                                    Log.Debug(LOG_PWR_MSG, 80);
                                    hat.Motors.Forwards(0.8);
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 50d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 30);
                                    hat.Motors.Forwards(0.3);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.Off();
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 80d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 40);
                                    hat.Motors.Forwards(0.4);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 110d)
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
                                    hat.Motors.Forwards(0.8);
                                    hat.Lights.One.Off();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }

                                Thread.Sleep(TimeSpan.FromSeconds(0.2));
                            }
                        }
                        Log.Debug("Sonar disposed");
                    }
                    Log.Debug("Hat disposed");
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// Stops runner
        /// </summary>
        public static void Stop()
        {
            _running = false;
        }
    }
}