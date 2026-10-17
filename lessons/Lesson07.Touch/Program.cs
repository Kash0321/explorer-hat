using System.Diagnostics;
using ExplorerHat.Common;

// Lesson 07: Touch pads
// The touch pads of the HAT feel a finger without any button: the finger changes the electricity of the pad.
// Pad 1 = forwards, pad 2 = backwards, pad 3 = turn left and pad 4 = turn right.
// Each pad has its own light: pad 1 = blue, pad 2 = yellow, pad 3 = red and pad 4 = green.
// The robot moves only while your finger is on the pad.
// New ideas: a remote control, a sensor that is "touched" or "not touched", and "if / else if" with four choices.

bool moveMotors = false;   // first try it with false: only the lights and the text. Then change it to true
double speed = 0.6;
int maxTime = 60000;       // the program ends after this time (milliseconds)
int pauseTime = 50;        // a short wait between two readings

// WARNING: with moveMotors = true, test it first with the wheels in the air.
// The robot stops when you lift your finger. If it does not stop, press Ctrl+C.

using (var hat = new SafeExplorerHat())
{
    var clock = Stopwatch.StartNew();
    string lastText = "";

    while (clock.ElapsedMilliseconds < maxTime)
    {
        // Ask every pad once in each turn of the loop
        bool forwards = hat.Touch.One.IsTouched();
        bool backwards = hat.Touch.Two.IsTouched();
        bool left = hat.Touch.Three.IsTouched();
        bool right = hat.Touch.Four.IsTouched();

        // The chip tells one pad at a time, so only one of them is true. If no pad is touched, the robot stops
        hat.Lights.Off();
        string text;
        if (forwards)
        {
            text = "forwards";
            hat.Lights.Blue.On();
            SetSpeeds(speed, speed);
        }
        else if (backwards)
        {
            text = "backwards";
            hat.Lights.Yellow.On();
            SetSpeeds(-speed, -speed);
        }
        else if (left)
        {
            text = "turn left";
            hat.Lights.Red.On();
            SetSpeeds(speed, -speed);
        }
        else if (right)
        {
            text = "turn right";
            hat.Lights.Green.On();
            SetSpeeds(-speed, speed);
        }
        else
        {
            text = "stopped";
            SetSpeeds(0.0, 0.0);
        }

        // Write the text only when it changes, so the screen is not full of lines
        if (text != lastText)
        {
            Console.WriteLine(text);
            lastText = text;
        }

        Thread.Sleep(pauseTime);
    }

    SetSpeeds(0.0, 0.0);
    Console.WriteLine("The end");

    // Sets the speed of the right wheel (motor One) and of the left wheel (motor Two), only if moveMotors is true
    void SetSpeeds(double rightWheel, double leftWheel)
    {
        if (moveMotors)
        {
            hat.Motors.One.Speed = rightWheel;
            hat.Motors.Two.Speed = leftWheel;
        }
    }
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
