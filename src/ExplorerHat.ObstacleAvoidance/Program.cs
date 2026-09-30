using System;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;

namespace ExplorerHat.ObstacleAvoidance
{
    class Program
    {
        static void Main(string[] args)
        {
            // Logging configuration
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .CreateLogger();

            // Logging OS INFO
            Log.Information("**************************************************************************************");
            Log.Information("   Framework: {frameworkDescription}", RuntimeInformation.FrameworkDescription);
            Log.Information("          OS: {osDescription}", RuntimeInformation.OSDescription);
            Log.Information("     OS Arch: {osArchitecture}", RuntimeInformation.OSArchitecture);
            Log.Information("    CPU Arch: {processArchitecture}", RuntimeInformation.ProcessArchitecture);
            Log.Information("**************************************************************************************");

            Console.WriteLine("Hit a key to enter in [ObstacleAvoiding] mode:");
            Console.WriteLine("  [N] Normal start: the motors start at full speed at once");
            Console.WriteLine("  [S] Smooth start: the motors start and brake little by little");

            // Wait until N or S is pressed
            ConsoleKey key = Console.ReadKey(true).Key;
            while (key != ConsoleKey.N && key != ConsoleKey.S)
            {
                key = Console.ReadKey(true).Key;
            }

            bool smoothStart = key == ConsoleKey.S;
            Log.Information("Smooth start: {smoothStart}", smoothStart);

            var task = Runner.RunAsync(smoothStart);

            Console.WriteLine("Hit any key again to stop");
            Console.ReadKey();
            Console.WriteLine();

            Runner.Stop();
            // Wait for the runner to stop the motors and release the hat before exiting
            task.Wait();
        }
    }
}
