using System.Diagnostics;
using ExplorerHat.Common;
using Iot.Device.Hcsr04;
using UnitsNet;

// Lesson 06: Distance
// The center HC-SR04 sensor measures the distance to the obstacle in front of the robot.
// The lights show how near it is, and the robot slows down and stops before it touches the obstacle.
// New ideas: a sensor that gives numbers, "if / else if / else" with three choices, and a "while" loop.

bool moveMotors = false;   // first try it with false: the robot only measures. Then change it to true
double fastSpeed = 0.8;
double slowSpeed = 0.6;
int slowDistance = 60;     // centimeters: nearer than this, the robot goes slowly (yellow light)
int stopDistance = 30;     // centimeters: nearer than this, the robot stops (red light)
int maxTime = 10000;       // the program ends after this time (milliseconds), also if there is no obstacle
int pauseTime = 60;        // a short wait between two readings, so the old echoes go away

// WARNING: with moveMotors = true, test it first with the wheels in the air (put your hand in front of the sensor).
// Then put it on the floor, with a wall or a box in front of it.

using (var hat = new SafeExplorerHat())
using (var sensor = new Hcsr04(triggerPin: 6, echoPin: 23))
{
    double lastReading = 400;
    var clock = Stopwatch.StartNew();

    while (clock.ElapsedMilliseconds < maxTime)
    {
        // Sometimes the sensor misses the echo and gives a distance much farther than the real one.
        // That is dangerous: the robot would think the way is free. So we use the nearest of the last two readings.
        double reading = MeasureDistance();
        double distance = Math.Min(reading, lastReading);
        lastReading = reading;

        hat.Lights.Off();
        if (distance < stopDistance)
        {
            Console.WriteLine(distance + " cm: stop!");
            hat.Lights.Red.On();
            SetSpeed(0.0);
            break;
        }
        else if (distance < slowDistance)
        {
            Console.WriteLine(distance + " cm: slowly");
            hat.Lights.Yellow.On();
            SetSpeed(slowSpeed);
        }
        else
        {
            Console.WriteLine(distance + " cm: the way is free");
            hat.Lights.Green.On();
            SetSpeed(fastSpeed);
        }

        Thread.Sleep(pauseTime);
    }

    SetSpeed(0.0);
    Console.WriteLine("The end");
    Thread.Sleep(2000);    // keep the last light on for a moment

    // Measures the distance once, in centimeters. No echo means there is nothing in front (the sensor measures up to 400 cm)
    double MeasureDistance()
    {
        if (sensor.TryGetDistance(out Length length))
        {
            return Math.Round(length.Centimeters);
        }
        else
        {
            return 400;
        }
    }

    // Sets the speed of both motors, only if moveMotors is true
    void SetSpeed(double speed)
    {
        if (moveMotors)
        {
            hat.Motors.One.Speed = speed;
            hat.Motors.Two.Speed = speed;
        }
    }
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
