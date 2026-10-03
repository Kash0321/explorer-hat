using System.Diagnostics;
using ExplorerHat.Common;
using Iot.Device.Hcsr04;
using UnitsNet;

// Lesson 12: The screen
// The robot has a screen with 4 lines of 20 characters. The lines are numbered 0, 1, 2 and 3.
// screen.Write(1, "Hello") writes "Hello" on line 1. The screen has no accents: write "Hola", "nino", "adios"...
// This lesson does not move the motors.

string name = "Harlequin";   // write your name here
int panelTime = 20000;       // part 3: how long the distance panel lasts (milliseconds)

using (var screen = new Screen())
using (var sensor = new Hcsr04(triggerPin: 6, echoPin: 23))
{
    // Part 1: text. Only 20 characters fit in a line: the rest is cut
    screen.Write(0, "Hello, " + name + "!");
    screen.Write(1, "I am a robot");
    screen.Write(2, "12345678901234567890");
    screen.Write(3, "This line is too long to fit");
    Thread.Sleep(5000);

    // Part 2: variables that change
    screen.Clear();
    screen.Write(0, "Counting...");
    for (int number = 1; number <= 10; number++)
    {
        screen.Write(1, "Number: " + number);
        screen.Write(2, "Double: " + number * 2);
        Thread.Sleep(500);
    }

    Thread.Sleep(2000);

    // Part 3: a distance panel, with the center sensor of lesson 06 and a bar
    screen.Clear();
    screen.Write(0, "Distance (center)");
    double lastReading = 400;
    var clock = Stopwatch.StartNew();
    while (clock.ElapsedMilliseconds < panelTime)
    {
        // The filter of lesson 06: the nearest of the last two readings
        double reading = MeasureDistance();
        double distance = Math.Min(reading, lastReading);
        lastReading = reading;
        screen.Write(1, distance + " cm");

        // The bar: one # for each 10 cm, 20 at most (the whole line)
        string bar = "";
        for (int block = 1; block <= distance / 10 && block <= Screen.Columns; block++)
        {
            bar = bar + "#";
        }

        screen.Write(2, bar);

        if (distance < 30)
        {
            screen.Write(3, "Too near!");
        }
        else
        {
            screen.Write(3, "");
        }

        Thread.Sleep(200);
    }

    screen.Clear();
    screen.Write(0, "Bye, " + name + "!");

    // Measures the distance once, in centimeters. No echo means there is nothing in front (400 cm)
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
}
// The screen keeps the last text after the program ends
