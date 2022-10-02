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
        const double FLL_POWER = 0.95;
        const double HGH_POWER = 0.90;
        const double MDM_POWER = 0.85;
        const double LOW_POWER = 0.80;

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
                            Log.Debug(LOG_PWR_MSG, FLL_POWER * 100);
                            hat.Motors.Forwards(FLL_POWER);

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
                                    hat.Motors.Backwards(MDM_POWER);
                                    Log.Debug("Backwards...");
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    Log.Debug("Turning to avoid the obstacle ...");

                                    if (sonar.Distance.LeftDistance <= sonar.Distance.RightDistance)
                                    {
                                        while (sonar.Distance.LeftDistance <= 20d)
                                        {
                                            hat.Motors.One.Forwards(MDM_POWER);
                                            hat.Motors.Two.Backwards(MDM_POWER);
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }
                                    else
                                    {
                                        while (sonar.Distance.RightDistance <= 20d)
                                        {
                                            hat.Motors.One.Backwards(MDM_POWER);
                                            hat.Motors.Two.Forwards(MDM_POWER);
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }


                                    Log.Debug("Turn completed");
                                    Log.Debug(LOG_PWR_MSG, FLL_POWER * 100);
                                    hat.Motors.Forwards(FLL_POWER);
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 50d)
                                {
                                    Log.Debug(LOG_PWR_MSG, LOW_POWER * 100);
                                    hat.Motors.Forwards(LOW_POWER);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.Off();
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 80d)
                                {
                                    Log.Debug(LOG_PWR_MSG, MDM_POWER * 100);
                                    hat.Motors.Forwards(MDM_POWER);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else if (sonar.Distance.MinimumDistance.Value < 110d)
                                {
                                    Log.Debug(LOG_PWR_MSG, HGH_POWER * 100);
                                    hat.Motors.Forwards(HGH_POWER);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else
                                {
                                    Log.Debug(LOG_PWR_MSG, FLL_POWER * 100);
                                    hat.Motors.Forwards(FLL_POWER);
                                    hat.Lights.One.Off();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }

                                Thread.Sleep(TimeSpan.FromSeconds(0.2));
                            }

                            hat.Lights.Off();
                            Log.Information("Lights Off");
                            hat.Motors.Stop();
                            Log.Information("Motors Stopped");
                        }
                        Log.Debug("Sonar offline");
                    }
                    Log.Debug("Hat offline");
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