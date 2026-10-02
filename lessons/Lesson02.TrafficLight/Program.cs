using ExplorerHat.Common;

// Lesson 02: Traffic light
// A traffic light repeats the same steps again and again: green, yellow, red.
// The times are in variables: change a number here and the whole program changes.

int greenTime = 3000;    // milliseconds
int yellowTime = 1000;
int redTime = 3000;
int cycles = 3;          // how many times the traffic light repeats green, yellow and red

using (var hat = new SafeExplorerHat())
{
    for (int cycle = 1; cycle <= cycles; cycle++)
    {
        Console.WriteLine("Cycle " + cycle + " of " + cycles);

        Console.WriteLine("  Green: go");
        hat.Lights.Green.On();
        Thread.Sleep(greenTime);
        hat.Lights.Green.Off();

        Console.WriteLine("  Yellow: be careful");
        hat.Lights.Yellow.On();
        Thread.Sleep(yellowTime);
        hat.Lights.Yellow.Off();

        Console.WriteLine("  Red: stop");
        hat.Lights.Red.On();
        Thread.Sleep(redTime);
        hat.Lights.Red.Off();
    }

    // At night, some traffic lights only blink the yellow light
    Console.WriteLine("Night mode: blinking yellow");
    for (int i = 1; i <= 6; i++)
    {
        hat.Lights.Yellow.On();
        Thread.Sleep(500);
        hat.Lights.Yellow.Off();
        Thread.Sleep(500);
    }

    Console.WriteLine("The end");
}
