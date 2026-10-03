using System.Device.Gpio;
using ExplorerHat.Common;

// Lesson 05: Buttons
// A button is an input: the program reads it and decides what to do with "if".
// Our button is on the breadboard, between 5V and the input IN4 of the HAT (GPIO 25).
// Pressed, the input reads High (5 V). Released, it reads Low (0 V).
// Our own LED is on the output OUT4 of the HAT (GPIO 16), with a 330 ohm resistor.

int buttonPin = 25;      // IN4
int ledPin = 16;         // OUT4
int partTime = 10000;    // how long parts 1 and 2 take (milliseconds)
int checkTime = 20;      // the program reads the button every 20 milliseconds

using (var hat = new SafeExplorerHat())
using (var gpio = new GpioController())
{
    gpio.OpenPin(buttonPin, PinMode.Input);
    gpio.OpenPin(ledPin, PinMode.Output);

    // Part 1: the red light and our LED are on while the button is pressed
    Console.WriteLine("Part 1: press the button and the lights turn on");
    for (int time = 0; time < partTime; time = time + checkTime)
    {
        if (IsPressed())
        {
            hat.Lights.Red.On();
            gpio.Write(ledPin, PinValue.High);
        }
        else
        {
            hat.Lights.Red.Off();
            gpio.Write(ledPin, PinValue.Low);
        }

        Thread.Sleep(checkTime);
    }

    hat.Lights.Red.Off();
    gpio.Write(ledPin, PinValue.Low);
    Thread.Sleep(1000);

    // Part 2: count the presses. A press is counted only when the button changes from released to pressed
    Console.WriteLine("Part 2: press the button as many times as you can in " + partTime / 1000 + " seconds. Go!");
    int presses = 0;
    bool wasPressed = false;
    for (int time = 0; time < partTime; time = time + checkTime)
    {
        bool pressed = IsPressed();
        if (pressed)
        {
            if (wasPressed == false)
            {
                presses = presses + 1;
                Console.WriteLine("Presses: " + presses);
            }
        }

        wasPressed = pressed;
        Thread.Sleep(checkTime);
    }

    Console.WriteLine("Time is up! You pressed the button " + presses + " times");
    Thread.Sleep(1000);

    // Part 3: our LED blinks once for each press
    Console.WriteLine("Part 3: the LED blinks " + presses + " times");
    for (int blink = 1; blink <= presses; blink++)
    {
        gpio.Write(ledPin, PinValue.High);
        Thread.Sleep(300);
        gpio.Write(ledPin, PinValue.Low);
        Thread.Sleep(300);
    }

    Console.WriteLine("The end");

    // Reads the button: true when it is pressed
    bool IsPressed()
    {
        return gpio.Read(buttonPin) == PinValue.High;
    }
}
// At the end of the "using" block, SafeExplorerHat turns off the lights of the robot
