using ExplorerHat.Common;

// Lesson 01: Lights
// The robot has four lights: blue, yellow, red and green.
// The program runs the instructions one by one, from top to bottom.
// Thread.Sleep(1000) waits 1000 milliseconds, that is, one second.

using (var hat = new SafeExplorerHat())
{
    // 1. Turn on the blue light for one second
    Console.WriteLine("Blue light on");
    hat.Lights.Blue.On();
    Thread.Sleep(1000);
    hat.Lights.Blue.Off();
    Thread.Sleep(1000);

    // 2. Turn on the lights one after the other
    Console.WriteLine("One light after the other");
    hat.Lights.Blue.On();
    Thread.Sleep(500);
    hat.Lights.Yellow.On();
    Thread.Sleep(500);
    hat.Lights.Red.On();
    Thread.Sleep(500);
    hat.Lights.Green.On();
    Thread.Sleep(1000);

    // 3. Turn off all the lights at the same time
    Console.WriteLine("All the lights off");
    hat.Lights.Off();
    Thread.Sleep(1000);

    // 4. Blink the red light 5 times. The "for" loop repeats the instructions between { and }
    Console.WriteLine("Blinking");
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine("Blink " + i);
        hat.Lights.Red.On();
        Thread.Sleep(300);
        hat.Lights.Red.Off();
        Thread.Sleep(300);
    }

    Console.WriteLine("The end");
}
// At the end of the "using" block, SafeExplorerHat turns off all the lights
