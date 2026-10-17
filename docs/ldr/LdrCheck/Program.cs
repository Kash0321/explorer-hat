// Measures the resistance of photoresistors (LDR) with the ADS1015 of the Explorer HAT Pro.
// Wiring (voltage divider): HAT 5V -> LDR -> node -> fixed resistor -> HAT GND, and the node -> Analog 1.
// Optional: Analog 2 -> 10 kOhm -> HAT 5V, to measure the supply instead of using 5.30 V.
// R(LDR) = R(fixed) * (Vsupply / Vnode - 1). Nothing else of the HAT is used: no motors, no pins.
using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Ads1115;

const double FixedResistor = 10_000;   // ohms, the resistor between the node and GND
const double DefaultSupply = 5.30;     // volts, measured on the 5V pin on 04/10/2026
const int DarkSeconds = 10;            // datasheets measure the dark resistance after 10 s

// Analog 1 = AIN3 and Analog 2 = AIN2 of the chip (the HAT numbers them the other way round)
const InputMultiplexer Node = InputMultiplexer.AIN3;
const InputMultiplexer Supply = InputMultiplexer.AIN2;

(MeasuringRange Range, double Volts)[] ranges =
{
    (MeasuringRange.FS0256, 0.256), (MeasuringRange.FS0512, 0.512), (MeasuringRange.FS1024, 1.024),
    (MeasuringRange.FS2048, 2.048), (MeasuringRange.FS4096, 4.096), (MeasuringRange.FS6144, 6.144),
};

using var adc = new Ads1115(I2cDevice.Create(new I2cConnectionSettings(1, 0x48)), Node,
    MeasuringRange.FS6144, DataRate.SPS128, DeviceMode.PowerDown);

double supply = Average(Supply, 0.5, out _, out _);
bool supplyMeasured = supply > 4.5 && supply < 5.6;
if (!supplyMeasured)
{
    supply = DefaultSupply;
}

Console.WriteLine("LDR check: HAT 5V -> LDR -> node -> 10 kOhm -> GND; node -> Analog 1.");
Console.WriteLine(supplyMeasured
    ? $"Supply measured on Analog 2: {supply:0.000} V"
    : $"Analog 2 not connected to 5V: using {supply:0.00} V for the supply");

var results = new List<string>();
while (true)
{
    Console.WriteLine();
    Console.Write("LDR name (Enter = finish): ");
    string? name = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(name))
    {
        break;
    }

    Console.Write("Lux from the phone, room light (Enter = skip): ");
    string lux = Console.ReadLine() ?? "";

    Console.Write("1/3 Room light: leave the LDR uncovered and press Enter. ");
    Console.ReadLine();
    double room = Measure("room light");

    Console.Write("2/3 Shadow: cover it with a finger and press Enter. ");
    Console.ReadLine();
    double shadow = Measure("finger");

    Console.Write($"3/3 Dark: cover it completely (cap, black tape) and press Enter, then wait {DarkSeconds} s. ");
    Console.ReadLine();
    for (int s = DarkSeconds; s > 0; s--)
    {
        Console.Write($"{s} ");
        Thread.Sleep(1000);
    }
    Console.WriteLine();
    double dark = Measure("dark");

    results.Add($"{name,-8} {Format(room),12} {Format(shadow),12} {Format(dark),12} {lux,8}");
}

Console.WriteLine();
Console.WriteLine($"Supply {supply:0.000} V ({(supplyMeasured ? "measured" : "assumed")}), fixed resistor {Format(FixedResistor)}");
Console.WriteLine($"{"LDR",-8} {"room",12} {"finger",12} {"dark",12} {"lux",8}");
foreach (string line in results)
{
    Console.WriteLine(line);
}

// Measures the node for 2 s and prints the voltage and the resistance of the LDR
double Measure(string what)
{
    double volts = Average(Node, 2.0, out double min, out double max);
    double ohms = Resistance(volts);
    Console.WriteLine($"   {what}: {volts:0.0000} V (min {min:0.0000}, max {max:0.0000}) -> LDR {Format(ohms)}");
    if (volts < 0.002)
    {
        Console.WriteLine("   Almost 0 V: is the LDR connected to 5V and the node to Analog 1?");
    }
    else if (volts > supply - 0.05)
    {
        Console.WriteLine("   Almost the supply: is the fixed resistor connected to GND?");
    }
    return ohms;
}

double Resistance(double volts) =>
    volts >= supply ? 0 : volts <= 0 ? double.PositiveInfinity : FixedResistor * (supply / volts - 1);

// Averages single-shot readings, each one with the smallest range that holds the voltage
double Average(InputMultiplexer input, double seconds, out double min, out double max)
{
    double sum = 0;
    int count = 0;
    min = double.MaxValue;
    max = double.MinValue;
    var clock = Stopwatch.StartNew();
    while (clock.Elapsed.TotalSeconds < seconds)
    {
        double volts = Read(input, ranges.Length - 1);
        for (int i = 0; i < ranges.Length - 1; i++)
        {
            if (volts < ranges[i].Volts * 0.8)
            {
                volts = Read(input, i);
                break;
            }
        }
        sum += volts;
        count++;
        min = Math.Min(min, volts);
        max = Math.Max(max, volts);
    }
    return sum / count;
}

// The ADS1015 puts its 12 bits in the high part of the 16-bit register, like the ADS1115
double Read(InputMultiplexer input, int range) =>
    adc.ReadRaw(input, ranges[range].Range, DataRate.SPS128) * ranges[range].Volts / 32768.0;

static string Format(double ohms) =>
    double.IsPositiveInfinity(ohms) ? "infinite"
    : ohms >= 1_000_000 ? $"{ohms / 1_000_000:0.00} MOhm"
    : ohms >= 1_000 ? $"{ohms / 1_000:0.0} kOhm"
    : $"{ohms:0} Ohm";
