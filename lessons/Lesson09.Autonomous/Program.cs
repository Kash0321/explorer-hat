using System.Diagnostics;
using ExplorerHat.Common;
using Iot.Device.Hcsr04;
using UnitsNet;

// Lesson 09: Autonomous robot
// WARNING: test it first with the wheels in the air. Then put it on the floor, with plenty of room and some boxes.
// The robot goes forwards on its own. When it sees an obstacle, it stops, goes backwards a little
// and turns to the side with more room. Then it goes on. Nobody tells it what to do: it senses, thinks and acts.
// It uses the ideas of lesson 04 (methods) and lesson 06 (distance and filter), now with the three sensors.
// New ideas: "||" (or) and "&&" (and) to join conditions.

double speed = 0.8;
int obstacleDistance = 30;  // centimeters: nearer than this, there is an obstacle
int backTime = 300;         // how long the robot goes backwards (milliseconds)
int turnStepTime = 150;     // the robot turns in short steps, and looks again after each step
int maxTurnSteps = 10;      // if the way is not free after this many steps, the robot goes backwards again
int pauseTime = 100;        // a short stop after each move, so the motors do not change direction at once
int echoTime = 60;          // a short wait after each reading, so its echoes go away before the next one
int maxTime = 60000;        // the robot stops after this time (milliseconds)

// What the robot sees (centimeters), with the filter of lesson 06: the nearest of the last two readings
double left = 400;
double center = 400;
double right = 400;

// The last reading of each sensor
double leftLast = 400;
double centerLast = 400;
double rightLast = 400;

using (var hat = new SafeExplorerHat())
using (var leftSensor = new Hcsr04(triggerPin: 13, echoPin: 24))
using (var centerSensor = new Hcsr04(triggerPin: 6, echoPin: 23))
using (var rightSensor = new Hcsr04(triggerPin: 12, echoPin: 22))
{
    LookTwice();
    var clock = Stopwatch.StartNew();

    while (clock.ElapsedMilliseconds < maxTime)
    {
        // 1. Sense
        Look();

        // 2. Think and 3. act
        if (center < obstacleDistance || left < obstacleDistance || right < obstacleDistance)
        {
            Console.WriteLine("Obstacle!");
            Stop();
            MoveBackwards(backTime);
            LookTwice();

            if (left < right)
            {
                // More room on the right: turn right while there is something in front or on the left
                for (int step = 1; step <= maxTurnSteps; step++)
                {
                    TurnRight(turnStepTime);
                    LookTwice();
                    if (center >= obstacleDistance && left >= obstacleDistance)
                    {
                        break;
                    }
                }
            }
            else
            {
                // More room on the left: turn left while there is something in front or on the right
                for (int step = 1; step <= maxTurnSteps; step++)
                {
                    TurnLeft(turnStepTime);
                    LookTwice();
                    if (center >= obstacleDistance && right >= obstacleDistance)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            MoveForwards();
        }
    }

    Stop();
    Console.WriteLine("Time is up!");

    // Measures the three sensors, one after the other, and keeps the nearest of the last two readings of each one
    void Look()
    {
        double reading;

        reading = Measure(centerSensor);
        center = Math.Min(reading, centerLast);
        centerLast = reading;

        reading = Measure(leftSensor);
        left = Math.Min(reading, leftLast);
        leftLast = reading;

        reading = Measure(rightSensor);
        right = Math.Min(reading, rightLast);
        rightLast = reading;

        Console.WriteLine("Left " + left + " cm   Center " + center + " cm   Right " + right + " cm");
    }

    // Looks twice, so the filter only uses readings taken now (for example, after the robot moves)
    void LookTwice()
    {
        Look();
        Look();
    }

    // Measures the distance once with a sensor, in centimeters. No echo means there is nothing in front (400 cm)
    double Measure(Hcsr04 sensor)
    {
        double reading = 400;
        if (sensor.TryGetDistance(out Length length))
        {
            reading = Math.Round(length.Centimeters);
        }

        Thread.Sleep(echoTime);
        return reading;
    }

    // Starts both motors forwards. The robot goes on until the program stops it
    void MoveForwards()
    {
        hat.Lights.Green.On();
        hat.Motors.One.Speed = speed;
        hat.Motors.Two.Speed = speed;
    }

    // Moves the robot backwards for some milliseconds and then stops it
    void MoveBackwards(int milliseconds)
    {
        hat.Lights.Red.On();
        hat.Motors.One.Speed = -speed;
        hat.Motors.Two.Speed = -speed;
        Thread.Sleep(milliseconds);
        Stop();
    }

    // Turns the robot to the right for some milliseconds and then stops it (motor One is the right wheel)
    void TurnRight(int milliseconds)
    {
        hat.Lights.Yellow.On();
        hat.Motors.One.Speed = -speed;
        hat.Motors.Two.Speed = speed;
        Thread.Sleep(milliseconds);
        Stop();
    }

    // Turns the robot to the left for some milliseconds and then stops it
    void TurnLeft(int milliseconds)
    {
        hat.Lights.Blue.On();
        hat.Motors.One.Speed = speed;
        hat.Motors.Two.Speed = -speed;
        Thread.Sleep(milliseconds);
        Stop();
    }

    // Stops both motors, turns the lights off and waits a moment
    void Stop()
    {
        hat.Motors.One.Speed = 0.0;
        hat.Motors.Two.Speed = 0.0;
        hat.Lights.Off();
        Thread.Sleep(pauseTime);
    }
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
