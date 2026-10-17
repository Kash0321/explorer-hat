using System.Diagnostics;
using ExplorerHat.Common;

// Lesson 08: Analog sensors
// A potentiometer gives a voltage that changes little by little, not only "on" or "off".
// The analog input of the HAT measures it, from 0 to 5 volts.
// The four lights show the level as a bar: the more volts, the more lights.
// New ideas: a voltage that changes, a number that has to fit in a scale, and "if" one after the other.

double minVolts = 0.0;     // at this voltage (or less) all the lights are off
double maxVolts = 5.3;     // at this voltage (or more) all the lights are on. The 5V pin of the HAT gives 5.3 volts
int maxTime = 30000;       // the program ends after this time (milliseconds)
int pauseTime = 100;       // a short wait between two readings

// Potentiometer: red wire to 5V, yellow wire to GND and black wire (the middle one) to Analog 1.

using (var hat = new SafeExplorerHat())
{
    var clock = Stopwatch.StartNew();

    while (clock.ElapsedMilliseconds < maxTime)
    {
        double volts = hat.Analog.One.ReadVoltage().Volts;

        // Put the voltage in a scale from 0 (minVolts) to 1 (maxVolts). Math.Clamp keeps it between 0 and 1.
        double level = Math.Clamp((volts - minVolts) / (maxVolts - minVolts), 0, 1);

        // 0 to 4 lights: the level multiplied by 4 and rounded to the nearest whole number
        int lightsOn = (int)Math.Round(level * 4);

        hat.Lights.Off();
        if (lightsOn >= 1)
        {
            hat.Lights.Blue.On();
        }
        if (lightsOn >= 2)
        {
            hat.Lights.Yellow.On();
        }
        if (lightsOn >= 3)
        {
            hat.Lights.Red.On();
        }
        if (lightsOn >= 4)
        {
            hat.Lights.Green.On();
        }

        Console.WriteLine(volts.ToString("0.00") + " V  " + new string('#', lightsOn * 5));

        Thread.Sleep(pauseTime);
    }
}
