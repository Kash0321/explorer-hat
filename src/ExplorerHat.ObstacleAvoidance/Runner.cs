using System;
using System.Threading;
using System.Threading.Tasks;
using ExplorerHat.Common;
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
        // Closer than this (in centimeters) is an obstacle: stop, go backwards and turn
        const double OBSTACLE_DISTANCE = 30d;
        // Short pause before changing the direction of the motors, so they don't draw so much current
        const int PAUSE_TIME = 100;
        // The robot always turns at least this long (in milliseconds), so the sensors see the new direction
        const int MIN_TURN_TIME = 300;
        // With the smooth start, the motors start little by little in this number of steps...
        const int SPEED_UP_STEPS = 4;
        // ...and this long each step (in milliseconds)
        const int SPEED_UP_STEP_TIME = 75;

        static volatile bool _running;
        static bool _smoothStart;

        static Runner()
        {
            _running = false;
        }

        /// <summary>
        /// Starts both motors, from stopped to the speed wanted: at once, or little by little
        /// when the smooth start is on. A positive speed goes forwards and a negative speed goes backwards.
        /// </summary>
        static void SpeedUp(SafeExplorerHat hat, double speedOne, double speedTwo)
        {
            if (!_smoothStart)
            {
                hat.Motors.One.Speed = speedOne;
                hat.Motors.Two.Speed = speedTwo;
                return;
            }

            for (int step = 1; step <= SPEED_UP_STEPS; step++)
            {
                hat.Motors.One.Speed = speedOne * step / SPEED_UP_STEPS;
                hat.Motors.Two.Speed = speedTwo * step / SPEED_UP_STEPS;
                Thread.Sleep(SPEED_UP_STEP_TIME);
            }
        }

        /// <summary>
        /// Executes the task managed by the runner, asynchronously
        /// </summary>
        /// <param name="smoothStart">Whether the motors start little by little</param>
        /// <returns>Task managed</returns>
        public static async Task RunAsync(bool smoothStart)
        {
            _smoothStart = smoothStart;

            await Task.Run(() => {
                try
                {
                    _running = true;
                    using (var hat = new SafeExplorerHat())
                    {
                        using (var sonar = new Sonar())
                        {
                            Log.Debug("Settling sonar devices and motors!");
                            sonar.WaitForNewReadings();
                            Log.Debug("GO!");
                            Log.Debug(LOG_PWR_MSG, FLL_POWER * 100);
                            SpeedUp(hat, FLL_POWER, FLL_POWER);

                            while (_running)
                            {
                                Log.Information("Distance to the nearest obstacle: Left {leftDistance} cm. Center {centerDistance} cm. Right {rightDistance} cm.", 
                                    sonar.Distance.LeftDistance,
                                    sonar.Distance.CenterDistance,
                                    sonar.Distance.RightDistance);

                                if (sonar.Distance.MinimumDistance.Value < OBSTACLE_DISTANCE)
                                {
                                    hat.Lights.One.On();
                                    hat.Lights.Two.On();
                                    hat.Lights.Three.On();
                                    hat.Lights.Four.On();

                                    Log.Debug("Obstacle detected. Maneuvering to avoid it...");
                                    hat.Motors.Stop();
                                    Log.Debug("Motors stopped");
                                    Thread.Sleep(PAUSE_TIME);
                                    Log.Debug("Backwards...");
                                    SpeedUp(hat, -MDM_POWER, -MDM_POWER);
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    hat.Motors.Stop();
                                    Thread.Sleep(PAUSE_TIME);

                                    // Choose the side with readings taken after going backwards
                                    Log.Debug("Waiting for new readings...");
                                    sonar.WaitForNewReadings();
                                    Log.Information("Distance to the nearest obstacle: Left {leftDistance} cm. Center {centerDistance} cm. Right {rightDistance} cm.",
                                        sonar.Distance.LeftDistance,
                                        sonar.Distance.CenterDistance,
                                        sonar.Distance.RightDistance);
                                    Log.Debug("Turning to avoid the obstacle ...");

                                    // Turn to the side with more room, while there is something in front
                                    // or on the side the robot is turning away from.
                                    // Motor One is the right wheel and motor Two is the left wheel.
                                    if (sonar.Distance.LeftDistance <= sonar.Distance.RightDistance)
                                    {
                                        // Obstacle on the left: turn right
                                        SpeedUp(hat, -MDM_POWER, MDM_POWER);
                                        Thread.Sleep(MIN_TURN_TIME);

                                        while (_running && (sonar.Distance.CenterDistance < OBSTACLE_DISTANCE || sonar.Distance.LeftDistance < OBSTACLE_DISTANCE))
                                        {
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }
                                    else
                                    {
                                        // Obstacle on the right: turn left
                                        SpeedUp(hat, MDM_POWER, -MDM_POWER);
                                        Thread.Sleep(MIN_TURN_TIME);

                                        while (_running && (sonar.Distance.CenterDistance < OBSTACLE_DISTANCE || sonar.Distance.RightDistance < OBSTACLE_DISTANCE))
                                        {
                                            Thread.Sleep(TimeSpan.FromSeconds(0.2));
                                        }
                                    }

                                    hat.Motors.Stop();
                                    Thread.Sleep(PAUSE_TIME);


                                    if (!_running)
                                    {
                                        // Stopped while turning: don't go forwards again
                                        break;
                                    }

                                    Log.Debug("Turn completed");

                                    // Look again before going forwards, with readings taken after the turn
                                    Log.Debug("Waiting for new readings...");
                                    sonar.WaitForNewReadings();
                                    if (sonar.Distance.MinimumDistance.Value < OBSTACLE_DISTANCE)
                                    {
                                        // There is still an obstacle: the next loop avoids it again
                                        Log.Debug("There is still an obstacle");
                                        continue;
                                    }

                                    Log.Debug(LOG_PWR_MSG, FLL_POWER * 100);
                                    SpeedUp(hat, FLL_POWER, FLL_POWER);
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