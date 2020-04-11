using System;
using System.Runtime.InteropServices;
using Serilog;

namespace ExplorerHat.LineTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            // Logging configuration
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Console()
               .CreateLogger();

            // Logging OS INFO
            Log.Information("**************************************************************************************");
            Log.Information("   Framework: {frameworkDescription}", RuntimeInformation.FrameworkDescription);
            Log.Information("          OS: {osDescription}", RuntimeInformation.OSDescription);
            Log.Information("     OS Arch: {osArchitecture}", RuntimeInformation.OSArchitecture);
            Log.Information("    CPU Arch: {processArchitecture}", RuntimeInformation.ProcessArchitecture);
            Log.Information("**************************************************************************************");

            Console.WriteLine("Hit any key to enter in [ObstacleAvoiding] mode");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("Hit any key again to stop");
            Console.ReadKey();
            Console.WriteLine();
        }
    }
}
