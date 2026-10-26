using System.Text;
using ExplorerHat.Brain;

// The brain of the robot (Phase 7), on a laptop: it reads an order written in Spanish, asks a language model (Ollama) for a
// plan with the closed list of actions, and sends the actions one by one to the order service of the Raspberry Pi.
// The safety is in the service, never here: this program only sends what the service accepts. A message that starts with
// a stop word ("para", "stop"...) stops the robot without asking the model. See README.md in this folder.
//
//   dotnet ExplorerHat.Brain.dll --llm http://<pc>:11434=qwen2.5:7b --llm http://127.0.0.1:11434=qwen2.5:3b --robot http://<pi>:5000

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var settings = Settings.Parse(args, out string error);
if (settings is null)
{
    Console.WriteLine(error);
    Console.WriteLine(Settings.Usage);
    return 2;
}

var session = new Session(settings);
Console.CancelKeyPress += (sender, e) => session.EmergencyStop();

session.Say(settings.OnlyShow ? "Cerebro en modo --solo-mostrar: no se manda nada al robot." : $"Cerebro conectado al robot {settings.RobotUrl}.");
session.Say("Escribe una orden. Para parar: 'para' o 'stop'. Para salir: Ctrl+C.");
await session.RunAsync();
return 0;
