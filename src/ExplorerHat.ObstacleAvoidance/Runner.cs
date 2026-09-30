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
        // The robot turns in short steps of this long (in milliseconds), and looks again after each step
        const int TURN_STEP_TIME = 150;
        // With the smooth start, the motors start and stop little by little in this number of steps...
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
        /// Stops both motors: at once, or little by little when the smooth start is on,
        /// so the robot doesn't tip forwards when it brakes
        /// </summary>
        static void SlowDown(SafeExplorerHat hat)
        {
            if (_smoothStart)
            {
                double speedOne = hat.Motors.One.Speed;
                double speedTwo = hat.Motors.Two.Speed;

                for (int step = SPEED_UP_STEPS - 1; step >= 1; step--)
                {
                    hat.Motors.One.Speed = speedOne * step / SPEED_UP_STEPS;
                    hat.Motors.Two.Speed = speedTwo * step / SPEED_UP_STEPS;
                    Thread.Sleep(SPEED_UP_STEP_TIME);
                }
            }

            hat.Motors.Stop();
        }

        /// <summary>
        /// Whether the robot must keep turning: there is something in front, or on the side
        /// it is turning away from (the left side when it turns right)
        /// </summary>
        static bool IsBlocked(DistanceTuple distance, bool turnRight)
        {
            if (distance.CenterDistance < OBSTACLE_DISTANCE)
            {
                return true;
            }

            if (turnRight)
            {
                return distance.LeftDistance < OBSTACLE_DISTANCE;
            }
            else
            {
                return distance.RightDistance < OBSTACLE_DISTANCE;
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
                                    SlowDown(hat);
                                    Log.Debug("Motors stopped");
                                    Thread.Sleep(PAUSE_TIME);
                                    Log.Debug("Backwards...");
                                    SpeedUp(hat, -MDM_POWER, -MDM_POWER);
                                    Thread.Sleep(TimeSpan.FromSeconds(0.25));
                                    SlowDown(hat);
                                    Thread.Sleep(PAUSE_TIME);

                                    // Choose the side with readings taken after going backwards
                                    Log.Debug("Waiting for new readings...");
                                    sonar.WaitForNewReadings();
                                    Log.Information("Distance to the nearest obstacle: Left {leftDistance} cm. Center {centerDistance} cm. Right {rightDistance} cm.",
                                        sonar.Distance.LeftDistance,
                                        sonar.Distance.CenterDistance,
                                        sonar.Distance.RightDistance);
                                    Log.Debug("Turning to avoid the obstacle ...");

                                    // Turn to the side with more room (obstacle on the left: turn right).
                                    // The robot turns a little, stops and looks again, while there is something
                                    // in front or on the side it is turning away from. The readings are taken
                                    // with the robot stopped, so it doesn't turn too much.
                                    bool turnRight = sonar.Distance.LeftDistance <= sonar.Distance.RightDistance;
                                    int turnSteps = 0;

                                    do
                                    {
                                        // Motor One is the right wheel and motor Two is the left wheel
                                        if (turnRight)
                                        {
                                            hat.Motors.One.Speed = -MDM_POWER;
                                            hat.Motors.Two.Speed = MDM_POWER;
                                        }
                                        else
                                        {
                                            hat.Motors.One.Speed = MDM_POWER;
                                            hat.Motors.Two.Speed = -MDM_POWER;
                                        }

                                        Thread.Sleep(TURN_STEP_TIME);
                                        hat.Motors.Stop();
                                        Thread.Sleep(PAUSE_TIME);
                                        sonar.WaitForNewReadings();
                                        turnSteps++;
                                    }
                                    while (_running && IsBlocked(sonar.Distance, turnRight));

                                    if (!_running)
                                    {
                                        // Stopped while turning: don't go forwards again
                                        break;
                                    }

                                    Log.Debug("Turn completed in {turnSteps} steps", turnSteps);

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