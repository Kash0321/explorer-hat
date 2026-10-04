using ExplorerHat.Common;

// Lesson 04: Draw a square
// WARNING: test it first with the wheels in the air. Then put it on the floor, with plenty of room.
// A square has 4 equal sides and 4 equal corners, so the robot repeats "forwards, turn right" 4 times.
// New idea: methods. A method gives a name to a group of instructions, and we can use that name many times.

double speed = 0.8;
int sideTime = 1000;     // how long each side takes (milliseconds)
int turnTime = 200;      // how long a quarter turn (90 degrees) takes: try other values until the corners are right
int pauseTime = 300;     // a short stop after each move, so that the robot does not slide

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
{
    for (int side = 1; side <= 4; side++)
    {
        Console.WriteLine("Side " + side);
        MoveForwards(sideTime);
        TurnRight(turnTime);
        if (waitAfterTurns)
        {
            Console.WriteLine("Press a key to go on");
            Console.ReadKey(true);
        }
    }

    Console.WriteLine("Square finished!");

    // Moves the robot forwards for some milliseconds and then stops it
    void MoveForwards(int milliseconds)
    {
        hat.Lights.Green.On();
        hat.Motors.One.Speed = speed;
        hat.Motors.Two.Speed = speed;
        Thread.Sleep(milliseconds);
        Stop();
    }

    // Turns the robot to the right for some milliseconds and then stops it
    void TurnRight(int milliseconds)
    {
        hat.Lights.Yellow.On();
        hat.Motors.One.Speed = -speed;
        hat.Motors.Two.Speed = speed;
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
