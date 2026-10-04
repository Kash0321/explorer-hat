using System.Diagnostics;
using System.Device.Gpio;
using ExplorerHat.Common;

// Lesson 11, part 2: Going straight
// WARNING: test it first with the wheels in the air. Then put it on the floor, with 2 meters free in a straight line.
// Now both wheels have a speed sensor. Two motors never turn exactly at the same speed, so the robot curves.
// The robot counts the pulses of both wheels: if one wheel goes ahead of the other, it slows that wheel down
// and speeds the other one up. It measures what it does and corrects it: this is called feedback.
// Lights: green = both wheels are even, yellow = the right wheel is ahead, blue = the left wheel is ahead.

bool correct = true;          // false: both motors always at the same speed. true: the robot corrects them
double speed = 0.6;
double correction = 0.02;     // how much the speed changes for each pulse of difference between the wheels
int maxDifference = 10;       // with corrections, a bigger difference means that a sensor is not counting: stop
double distance = 100;        // centimeters
int maxTime = 6000;           // the robot stops after this time (milliseconds), also if a sensor does not work
double wheelDiameter = 6.5;   // centimeters
int slots = 20;               // slots in each disc: pulses for each turn of the wheel
int leftPin = 18;             // PWM pin of the 3.3V row: speed sensor of the left wheel (motor Two), powered at 3.3 V
int rightPin = 25;            // IN4: speed sensor of the right wheel (motor One)

double cmPerPulse = wheelDiameter * Math.PI / slots;
bool leftWasHigh = true;
bool rightWasHigh = true;

using (var hat = new SafeExplorerHat())
using (var gpio = new GpioController())
{
    gpio.OpenPin(leftPin, PinMode.Input);
    gpio.OpenPin(rightPin, PinMode.Input);

    if (correct)
    {
        Console.WriteLine("Going forwards " + distance + " cm, correcting the speed of the wheels");
    }
    else
    {
        Console.WriteLine("Going forwards " + distance + " cm, without corrections");
    }

    int left = 0;
    int right = 0;
    bool sensorProblem = false;
    double leftSpeed = speed;
    double rightSpeed = speed;
    var clock = Stopwatch.StartNew();

    // The robot goes on while the middle of the robot has not gone "distance" and there is still time
    while ((left + right) / 2.0 * cmPerPulse < distance && clock.ElapsedMilliseconds < maxTime)
    {
        if (NewLeftPulse())
        {
            left = left + 1;
        }

        if (NewRightPulse())
        {
            right = right + 1;
        }

        // Positive: the right wheel is ahead. Negative: the left wheel is ahead
        int difference = right - left;

        // If a sensor does not count, the robot thinks that the other wheel is far ahead and turns more and more.
        // Math.Abs gives the difference without its sign: 5 for 5 and also for -5
        if (correct && Math.Abs(difference) > maxDifference)
        {
            sensorProblem = true;
            break;
        }

        if (correct)
        {
            // The wheel that is ahead goes slower, and the other one goes faster
            rightSpeed = Math.Clamp(speed - difference * correction, 0.0, 1.0);
            leftSpeed = Math.Clamp(speed + difference * correction, 0.0, 1.0);
        }

        hat.Motors.One.Speed = rightSpeed;
        hat.Motors.Two.Speed = leftSpeed;

        if (difference > 0)
        {
            hat.Lights.Green.Off();
            hat.Lights.Blue.Off();
            hat.Lights.Yellow.On();
        }
        else if (difference < 0)
        {
            hat.Lights.Green.Off();
            hat.Lights.Yellow.Off();
            hat.Lights.Blue.On();
        }
        else
        {
            hat.Lights.Yellow.Off();
            hat.Lights.Blue.Off();
            hat.Lights.Green.On();
        }

        Thread.Sleep(1);
    }

    hat.Motors.One.Speed = 0.0;
    hat.Motors.Two.Speed = 0.0;
    hat.Lights.Off();
    hat.Lights.Red.On();

    if (sensorProblem)
    {
        Console.WriteLine("The wheels are too different! Is a speed sensor out of its place?");
    }
    else if (clock.ElapsedMilliseconds >= maxTime)
    {
        Console.WriteLine("Time is up! Do both wheels turn? Are both speed sensors connected (PWM and IN4)?");
    }

    // The robot does not stop at once: count the pulses for a moment after stopping the motors
    clock.Restart();
    while (clock.ElapsedMilliseconds < 500)
    {
        if (NewLeftPulse())
        {
            left = left + 1;
        }

        if (NewRightPulse())
        {
            right = right + 1;
        }

        Thread.Sleep(1);
    }

    Console.WriteLine("Left wheel: " + left + " pulses = " + Math.Round(left * cmPerPulse, 1) + " cm");
    Console.WriteLine("Right wheel: " + right + " pulses = " + Math.Round(right * cmPerPulse, 1) + " cm");
    Console.WriteLine("Difference (right - left): " + (right - left) + " pulses");

    // Reads the speed sensor of the left wheel: true only when a new pulse starts (Low to High)
    bool NewLeftPulse()
    {
        bool high = gpio.Read(leftPin) == PinValue.High;
        bool isNew = false;
        if (high && leftWasHigh == false)
        {
            isNew = true;
        }

        leftWasHigh = high;
        return isNew;
    }

    // Reads the speed sensor of the right wheel: true only when a new pulse starts (Low to High)
    bool NewRightPulse()
    {
        bool high = gpio.Read(rightPin) == PinValue.High;
        bool isNew = false;
        if (high && rightWasHigh == false)
        {
            isNew = true;
        }

        rightWasHigh = high;
        return isNew;
    }
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
