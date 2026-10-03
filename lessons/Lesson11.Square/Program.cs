using System.Diagnostics;
using System.Device.Gpio;
using ExplorerHat.Common;

// Lesson 11, part 3: The perfect square
// WARNING: test it first with the wheels in the air. Then put it on the floor, with 1 meter free around it.
// Lesson 04 drew a square with times: "turn for 250 milliseconds". But the angle changed with the floor and the
// batteries. Now the robot counts the pulses of its wheels: "turn until each wheel has done 10 pulses".
// Turning on the spot, each wheel goes around a circle. The distance between the wheels is its diameter.

double speed = 0.6;           // speed to go forwards
double turnSpeed = 0.6;       // speed to turn
double side = 30;             // centimeters of each side of the square
double angle = 90;            // degrees of each turn
int brakePulses = 3;          // the robot slides a bit after it stops: each wheel stops this many pulses earlier
double correction = 0.02;     // going forwards, how much the speed changes for each pulse of difference (part 2)
int maxDifference = 10;       // going forwards, a bigger difference means that a sensor is not counting: stop
int maxMoveTime = 3000;       // each move stops after this time (milliseconds), also if a sensor does not work
int pauseTime = 300;          // a short stop after each move
double wheelDiameter = 6.5;   // centimeters
double wheelDistance = 13;    // centimeters between the middle of both wheels
int slots = 20;               // slots in each disc: pulses for each turn of the wheel
int leftPin = 18;             // PWM pin of the 3.3V row: speed sensor of the left wheel (motor Two), powered at 3.3 V
int rightPin = 25;            // IN4: speed sensor of the right wheel (motor One)

double cmPerPulse = wheelDiameter * Math.PI / slots;
double cmPerRobotTurn = wheelDistance * Math.PI;   // what each wheel goes when the robot turns on the spot once
bool leftWasHigh = true;
bool rightWasHigh = true;

// Calibration mode: the robot stops after each turn and waits for a key, so we can measure the corner
bool waitAfterTurns;
Console.WriteLine("Calibration mode? The robot stops after each turn (y/n)");
char answer = Console.ReadKey(true).KeyChar;
if (answer == 'y')
{
    waitAfterTurns = true;
    Console.WriteLine("Calibration mode: press a key after each turn");
}
else
{
    waitAfterTurns = false;
    Console.WriteLine("Full square");
}

using (var hat = new SafeExplorerHat())
using (var gpio = new GpioController())
{
    gpio.OpenPin(leftPin, PinMode.Input);
    gpio.OpenPin(rightPin, PinMode.Input);

    for (int corner = 1; corner <= 4; corner++)
    {
        Console.WriteLine("Side " + corner);
        MoveForwards(side);
        TurnRight(angle);
        if (waitAfterTurns)
        {
            Console.WriteLine("Press a key to go on");
            Console.ReadKey(true);
        }
    }

    Console.WriteLine("Square finished!");

    // Goes forwards some centimeters, correcting the speed of the wheels like part 2
    void MoveForwards(double centimeters)
    {
        int target = (int)Math.Round(centimeters / cmPerPulse);
        int left = 0;
        int right = 0;
        leftWasHigh = true;
        rightWasHigh = true;
        hat.Lights.Green.On();
        var clock = Stopwatch.StartNew();

        while ((left + right) / 2 < target && clock.ElapsedMilliseconds < maxMoveTime)
        {
            if (NewLeftPulse())
            {
                left = left + 1;
            }

            if (NewRightPulse())
            {
                right = right + 1;
            }

            int difference = right - left;
            if (Math.Abs(difference) > maxDifference)
            {
                Console.WriteLine("The wheels are too different! Is a speed sensor out of its place?");
                break;
            }

            hat.Motors.One.Speed = Math.Clamp(speed - difference * correction, 0.0, 1.0);
            hat.Motors.Two.Speed = Math.Clamp(speed + difference * correction, 0.0, 1.0);
            Thread.Sleep(1);
        }

        Stop();
        Console.WriteLine("  Forwards: left " + left + " pulses, right " + right + " pulses");
        ShowSlide();
    }

    // Turns right on the spot: the right wheel goes backwards and the left wheel forwards.
    // Each wheel stops when it has done its pulses
    void TurnRight(double degrees)
    {
        double centimeters = degrees / 360 * cmPerRobotTurn;
        int target = (int)Math.Round(centimeters / cmPerPulse) - brakePulses;
        int left = 0;
        int right = 0;
        leftWasHigh = true;
        rightWasHigh = true;
        hat.Lights.Yellow.On();
        hat.Motors.One.Speed = -turnSpeed;
        hat.Motors.Two.Speed = turnSpeed;
        var clock = Stopwatch.StartNew();

        while ((left < target || right < target) && clock.ElapsedMilliseconds < maxMoveTime)
        {
            if (NewLeftPulse())
            {
                left = left + 1;
            }

            if (NewRightPulse())
            {
                right = right + 1;
            }

            if (left >= target)
            {
                hat.Motors.Two.Speed = 0.0;
            }

            if (right >= target)
            {
                hat.Motors.One.Speed = 0.0;
            }

            Thread.Sleep(1);
        }

        Stop();
        Console.WriteLine("  Turn of " + degrees + " degrees: " + target + " pulses for each wheel (left "
            + left + ", right " + right + ")");
        ShowSlide();
    }

    // The robot slides a bit after it stops: counts the pulses during the pause after each move
    void ShowSlide()
    {
        int left = 0;
        int right = 0;
        var clock = Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < pauseTime)
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

        Console.WriteLine("  It slid: left " + left + " pulses, right " + right + " pulses");
    }

    // Stops both motors and turns the lights off
    void Stop()
    {
        hat.Motors.One.Speed = 0.0;
        hat.Motors.Two.Speed = 0.0;
        hat.Lights.Off();
    }

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
