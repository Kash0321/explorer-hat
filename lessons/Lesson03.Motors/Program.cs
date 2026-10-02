using ExplorerHat.Common;

// Lesson 03: Motors
// WARNING: test it first with the wheels in the air!
// Each motor has a speed from -1.0 (full speed backwards) to 1.0 (full speed forwards). 0.0 is stopped.
// Motor One is the right wheel and motor Two is the left wheel.

double speed = 0.8;
int moveTime = 1000;     // milliseconds
int pauseTime = 500;     // a short stop between two moves

using (var hat = new SafeExplorerHat())
{
    // 1. Forwards: both wheels forwards. Green light.
    Console.WriteLine("Forwards");
    hat.Lights.Green.On();
    hat.Motors.One.Speed = speed;
    hat.Motors.Two.Speed = speed;
    Thread.Sleep(moveTime);
    hat.Motors.One.Speed = 0.0;
    hat.Motors.Two.Speed = 0.0;
    hat.Lights.Green.Off();
    Thread.Sleep(pauseTime);

    // 2. Backwards: both wheels backwards (negative speed). Red light.
    Console.WriteLine("Backwards");
    hat.Lights.Red.On();
    hat.Motors.One.Speed = -speed;
    hat.Motors.Two.Speed = -speed;
    Thread.Sleep(moveTime);
    hat.Motors.One.Speed = 0.0;
    hat.Motors.Two.Speed = 0.0;
    hat.Lights.Red.Off();
    Thread.Sleep(pauseTime);

    // 3. Turn right: the right wheel goes backwards and the left wheel goes forwards. Yellow light.
    Console.WriteLine("Turn right");
    hat.Lights.Yellow.On();
    hat.Motors.One.Speed = -speed;
    hat.Motors.Two.Speed = speed;
    Thread.Sleep(moveTime);
    hat.Motors.One.Speed = 0.0;
    hat.Motors.Two.Speed = 0.0;
    hat.Lights.Yellow.Off();
    Thread.Sleep(pauseTime);

    // 4. Turn left: the right wheel goes forwards and the left wheel goes backwards. Blue light.
    Console.WriteLine("Turn left");
    hat.Lights.Blue.On();
    hat.Motors.One.Speed = speed;
    hat.Motors.Two.Speed = -speed;
    Thread.Sleep(moveTime);
    hat.Motors.One.Speed = 0.0;
    hat.Motors.Two.Speed = 0.0;
    hat.Lights.Blue.Off();

    Console.WriteLine("The end");
}
// At the end of the "using" block, SafeExplorerHat stops both motors, also if there is an error
