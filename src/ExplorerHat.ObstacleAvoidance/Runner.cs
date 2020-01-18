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
                                var distance = sonar.Distance;
                                Log.Information("Distance to the nearest obstacle: Left {leftDistance} cm. Center {centerDistance} cm. Right {rightDistance} cm.", 
                                    distance.LeftDistance,
                                    distance.CenterDistance,
                                    distance.RightDistance);

                                if (distance.MinimumDistance.Value < 40d)
                                {
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.On();

                                    Log.Debug("Obstacle detected. Maneuvering to avoid it...");
                                    hat.Motors.Stop();
                                    Log.Debug("Motors stopped");
                                    hat.Motors.Backwards(1);
                                    Log.Debug("Backwards...");
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    Log.Debug("Turning to avoid the obstacle ...");

                                    if (distance.LeftDistance <= distance.RightDistance)
                                    {
                                        hat.Motors.One.Forwards(1);
                                        hat.Motors.Two.Backwards(1);
                                    }
                                    else
                                    {
                                        hat.Motors.One.Backwards(1);
                                        hat.Motors.Two.Forwards(1);
                                    }
                                    
                                    Thread.Sleep(TimeSpan.FromSeconds(0.35));

                                    Log.Debug("Turn completed");
                                    Log.Debug(LOG_PWR_MSG, 100);
                                    hat.Motors.Forwards(1);
                                }
                                else if (distance.MinimumDistance.Value < 50d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 100);
                                    hat.Motors.Forwards(1);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.Off();
                                }
                                else if (distance.MinimumDistance.Value < 80d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 100);
                                    hat.Motors.Forwards(1);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else if (distance.MinimumDistance.Value < 110d)
                                {
                                    Log.Debug(LOG_PWR_MSG, 100);
                                    hat.Motors.Forwards(1);
                                    hat.Lights.One.On();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }
                                else
                                {
                                    Log.Debug(LOG_PWR_MSG, 100);
                                    hat.Motors.Forwards(1);
                                    hat.Lights.One.Off();
                                    hat.Lights.Two.Off();
                                    hat.Lights.Three.Off();
                                    hat.Lights.Four.Off();
                                }

                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
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

        /// <summary>
        /// Stops runner
        /// </summary>
        public static void Stop()
        {
            _running = false;
        }
    }
}