using System.Device.Gpio;
using System.Diagnostics;
using ExplorerHat.Common;

// Lesson 11: Counting wheel turns (odometry)
// WARNING: test it first with the wheels in the air. Then put it on the floor, with a tape measure next to it.
// The right wheel has a disc with 20 slots. The speed sensor sees light through each slot: 20 pulses for each turn.
// Counting pulses, the robot knows how far its wheel has gone. It is like the button of lesson 05,
// but the wheel presses it.
// New ideas: pi (3.14...) to know how far one turn of the wheel goes, and a "while" with two conditions.

double speed = 0.6;
int measureTime = 2000;       // part 1: how long the robot goes forwards to measure its speed (milliseconds)
double distance = 50;         // part 2: how far the robot goes (centimeters)
int maxTime = 5000;           // part 2 stops after this time (milliseconds), also if the sensor does not work
double wheelDiameter = 6.5;   // centimeters
int slots = 20;               // slots in the disc: pulses for each turn of the wheel
int sensorPin = 25;           // IN4: the speed sensor of the right wheel

// One turn of the wheel moves the robot as far as the edge of the wheel: the diameter times pi
double cmPerTurn = wheelDiameter * Math.PI;
double cmPerPulse = cmPerTurn / slots;
Console.WriteLine("One turn of the wheel: " + Math.Round(cmPerTurn, 1) + " cm. One pulse: " + Math.Round(cmPerPulse, 2) + " cm");

bool wasHigh = true;

using (var hat = new SafeExplorerHat())
using (var gpio = new GpioController())
{
    gpio.OpenPin(sensorPin, PinMode.Input);

    // Part 1: how fast does the robot go?
    Console.WriteLine("Part 1: measuring the speed for " + measureTime / 1000 + " seconds");
    hat.Lights.Blue.On();
    MoveForwards();
    int pulses = CountPulses(measureTime);
    Stop();
    double cmPerSecond = pulses * cmPerPulse / (measureTime / 1000.0);
    Console.WriteLine(pulses + " pulses = " + Cm(pulses) + " cm in " + measureTime / 1000 + " s: "
        + Math.Round(cmPerSecond) + " cm per second");
    ShowSlide(pulses);

    Console.WriteLine("Measure with the tape how far the robot went. Then press a key for part 2");
    Console.ReadKey(true);

    // Part 2: go forwards exactly "distance" centimeters
    Console.WriteLine("Part 2: going forwards " + distance + " cm");
    hat.Lights.Green.On();
    MoveForwards();
    pulses = 0;
    var clock = Stopwatch.StartNew();
    while (pulses * cmPerPulse < distance && clock.ElapsedMilliseconds < maxTime)
    {
        if (NewPulse())
        {
            pulses = pulses + 1;
        }

        Thread.Sleep(1);
    }

    Stop();
    if (clock.ElapsedMilliseconds >= maxTime)
    {
        Console.WriteLine("Time is up! Is the speed sensor connected to IN4?");
    }

    Console.WriteLine("Stopped after " + pulses + " pulses = " + Cm(pulses) + " cm");
    ShowSlide(pulses);

    // Counts the new pulses for some milliseconds
    int CountPulses(int milliseconds)
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        while (timer.ElapsedMilliseconds < milliseconds)
        {
            if (NewPulse())
            {
                count = count + 1;
            }

            Thread.Sleep(1);
        }

        return count;
    }

    // The robot does not stop at once: counts the pulses for a moment after stopping the motors
    void ShowSlide(int pulsesBefore)
    {
        int extraPulses = CountPulses(500);
        Console.WriteLine("After stopping, the wheel still turned " + extraPulses + " pulses = " + Cm(extraPulses) + " cm");
        Console.WriteLine("In total: " + Cm(pulsesBefore + extraPulses) + " cm");
    }

    // Changes pulses into centimeters, with one decimal
    double Cm(int count)
    {
        return Math.Round(count * cmPerPulse, 1);
    }

    // Reads the speed sensor: true only when a new pulse starts (the sensor changes from Low to High).
    // A pulse that has already started when the program begins is not counted (wasHigh starts as true).
    bool NewPulse()
    {
        bool high = gpio.Read(sensorPin) == PinValue.High;
        bool isNew = false;
        if (high && wasHigh == false)
        {
            isNew = true;
        }

        wasHigh = high;
        return isNew;
    }

    // Starts both motors forwards
    void MoveForwards()
    {
        hat.Lights.Red.Off();
        hat.Motors.One.Speed = speed;
        hat.Motors.Two.Speed = speed;
    }

    // Stops both motors and turns the lights off. The red light is on while the robot stops
    void Stop()
    {
        hat.Motors.One.Speed = 0.0;
        hat.Motors.Two.Speed = 0.0;
        hat.Lights.Off();
        hat.Lights.Red.On();
    }
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
