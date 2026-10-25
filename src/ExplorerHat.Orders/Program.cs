using System.Security.Cryptography;
using System.Text;
using ExplorerHat.Common;
using ExplorerHat.Orders;

// Order service of the robot (Phase 7). The brain (an LLM on a laptop) sends one action at a time by HTTP and
// the service checks it against a closed list, cuts the values to their limits and runs it. Safety is here,
// never in the LLM: a movement lasts 3 seconds at most, forwards it stops by itself in front of an obstacle,
// and /parar stops the motors at once. See README.md in this folder.
//
//   dotnet ExplorerHat.Orders.dll --simulado                    (no pins: tests on the PC)
//   dotnet ExplorerHat.Orders.dll --velocidad-cero              (real pins, the wheels never turn)
//   dotnet ExplorerHat.Orders.dll --real                        (the wheels turn: wheels in the air first!)
//   Options: --puerto 5000   --clave-archivo <path>

string mode = args.Contains("--simulado") ? "simulado" : args.Contains("--velocidad-cero") ? "velocidadCero" : args.Contains("--real") ? "real" : "";
if (mode == "")
{
    Console.WriteLine("Elige el modo: --simulado, --velocidad-cero o --real.");
    return 2;
}

string Option(string name, string fallback)
{
    int index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
}

int port = int.Parse(Option("--puerto", "5000"));
string keyFile = Option("--clave-archivo", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".explorerhat-clave"));
string key = ReadOrCreateKey(keyFile);

SafeExplorerHat? hat = null;
HatSensors? hatSensors = null;
IRobotBody body;
IDistanceSensors sensors;
SimulatedSensors? simulatedSensors = null;

if (mode == "simulado")
{
    body = new SimulatedBody();
    simulatedSensors = new SimulatedSensors();
    sensors = simulatedSensors;
}
else
{
    hat = new SafeExplorerHat();
    hatSensors = new HatSensors();
    body = new HatBody(hat, zeroSpeed: mode == "velocidadCero");
    sensors = hatSensors;
}

// SafeExplorerHat stops the motors on Ctrl+C, kill, SSH loss or an unhandled error: Run() stays inside "using"
using (hat)
using (hatSensors)
using (var monitor = new ObstacleMonitor(sensors))
{
    var runner = new OrderRunner(body, monitor);

    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    var app = builder.Build();

    // Every request needs the key (header X-Clave): nobody else in the network can move the robot
    app.Use(async (context, next) =>
    {
        string sent = context.Request.Headers["X-Clave"].ToString();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sent), Encoding.UTF8.GetBytes(key)))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "clave incorrecta" });
            return;
        }

        await next();
    });

    app.MapPost("/accion", async (HttpRequest request) =>
    {
        string json = await new StreamReader(request.Body).ReadToEndAsync();
        var order = OrderParser.Parse(json, out string error);
        if (order is null)
        {
            Log($"Orden rechazada: {error}");
            return Results.Json(new { error }, statusCode: 400);
        }

        Log($"Orden: {json}");
        var outcome = await runner.RunAsync(order);
        Log($"  -> {outcome.Status} {System.Text.Json.JsonSerializer.Serialize(outcome.Body)}");
        return Results.Json(outcome.Body, statusCode: outcome.Status);
    });

    app.MapPost("/parar", () =>
    {
        var outcome = runner.Stop();
        Log("Parada");
        return Results.Json(outcome.Body, statusCode: outcome.Status);
    });

    app.MapGet("/estado", () =>
    {
        var distances = monitor.Latest;
        return Results.Json(new
        {
            modo = mode,
            ocupado = runner.Current is not null,
            accion = runner.Current,
            motores = new { derecha = body.Right, izquierda = body.Left },
            luces = new
            {
                azul = body.IsLightOn("azul"),
                amarilla = body.IsLightOn("amarilla"),
                roja = body.IsLightOn("roja"),
                verde = body.IsLightOn("verde"),
            },
            distanciasCm = distances is null ? null : new { izquierda = distances.Left, centro = distances.Center, derecha = distances.Right },
        });
    });

    if (simulatedSensors is not null)
    {
        // Only in --simulado: the tests set the distances. It waits for the monitor to read them.
        app.MapPut("/simulado/sensores", async (HttpRequest request) =>
        {
            var values = await request.ReadFromJsonAsync<SimulatedValues>();
            simulatedSensors.Set(values is null || values.SinDatos ? null : new Distances(values.Izquierda, values.Centro, values.Derecha));
            await Task.Delay(150);
            return Results.Json(new { estado = "ok" });
        });
    }

    Log($"Servicio de órdenes en el puerto {port}, modo {mode}. Clave en {keyFile}");
    app.Run();
}

return 0;

// A closed terminal (lost SSH session) must not break an order
static void Log(string message)
{
    try
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}");
    }
    catch (IOException)
    {
        // Nobody reads the console any more
    }
}

// The key is a file only the user can read. If it does not exist, it is created with a random one.
static string ReadOrCreateKey(string path)
{
    if (File.Exists(path))
    {
        return File.ReadAllText(path).Trim();
    }

    string created = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
    File.WriteAllText(path, created + Environment.NewLine);
    if (!OperatingSystem.IsWindows())
    {
        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    Console.WriteLine($"Clave nueva creada en {path}");
    return created;
}

record SimulatedValues(double Izquierda = 400, double Centro = 400, double Derecha = 400, bool SinDatos = false);
