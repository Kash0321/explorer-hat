namespace ExplorerHat.Brain
{
    /// <summary>A language-model server (Ollama) and the model to ask it</summary>
    public record LlmServer(string Url, string Model);

    /// <summary>The settings of the brain, read from the command line. Nothing about the network is written in the code.</summary>
    public class Settings
    {
        public const string KeyVariable = "EXPLORERHAT_CLAVE";

        public string RobotUrl { get; private set; } = "";

        /// <summary>The key of the order service. It comes from a file or a variable, never from the command line (it would stay in the history).</summary>
        public string Key { get; private set; } = "";

        /// <summary>In order: the first one is asked first, the others are the fallbacks</summary>
        public List<LlmServer> Servers { get; } = new List<LlmServer>();

        /// <summary>Show the plan, but do not send anything to the robot</summary>
        public bool OnlyShow { get; private set; }

        public int NumCtx { get; private set; } = 2048;

        public const string Usage =
            "Uso: ExplorerHat.Brain --llm <url>=<modelo> [--llm <url>=<modelo> ...] --robot <url> [opciones]\n" +
            "  --llm            servidor de Ollama y su modelo; se puede repetir: el primero es el principal y los demás, el respaldo\n" +
            "                   por ejemplo: --llm http://<ip-del-pc>:11434=qwen2.5:7b --llm http://127.0.0.1:11434=qwen2.5:3b\n" +
            "  --robot          servicio de órdenes de la Pi, por ejemplo http://<ip-de-la-pi>:5000\n" +
            "  --clave-archivo  archivo con la clave del robot (por defecto ~/.explorerhat-clave). También vale la variable " + KeyVariable + "\n" +
            "  --solo-mostrar   enseña lo que haría, sin mandar nada al robot (no hace falta --robot)\n" +
            "  --num-ctx        contexto del modelo (por defecto 2048, como en las medidas)";

        public static Settings? Parse(string[] args, out string error)
        {
            var settings = new Settings();
            string keyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".explorerhat-clave");
            error = "";

            for (int i = 0; i < args.Length; i++)
            {
                string name = args[i];
                if (name == "--solo-mostrar")
                {
                    settings.OnlyShow = true;
                    continue;
                }

                if (i + 1 >= args.Length)
                {
                    error = $"Falta el valor de {name}.";
                    return null;
                }

                string value = args[++i];
                switch (name)
                {
                    case "--robot":
                        settings.RobotUrl = value.TrimEnd('/');
                        break;
                    case "--clave-archivo":
                        keyFile = value;
                        break;
                    case "--num-ctx":
                        if (!int.TryParse(value, out int numCtx) || numCtx < 512)
                        {
                            error = "--num-ctx tiene que ser un número entero de 512 o más.";
                            return null;
                        }

                        settings.NumCtx = numCtx;
                        break;
                    case "--llm":
                        int equals = value.LastIndexOf('=');
                        if (equals <= 0 || equals == value.Length - 1)
                        {
                            error = "--llm se escribe <url>=<modelo>, por ejemplo http://127.0.0.1:11434=qwen2.5:3b.";
                            return null;
                        }

                        settings.Servers.Add(new LlmServer(value.Substring(0, equals).TrimEnd('/'), value.Substring(equals + 1)));
                        break;
                    default:
                        error = $"Opción desconocida: {name}.";
                        return null;
                }
            }

            if (settings.Servers.Count == 0)
            {
                error = "Falta al menos un --llm.";
                return null;
            }

            if (!settings.OnlyShow)
            {
                if (settings.RobotUrl == "")
                {
                    error = "Falta --robot (o usa --solo-mostrar).";
                    return null;
                }

                string? key = Environment.GetEnvironmentVariable(KeyVariable);
                if (string.IsNullOrWhiteSpace(key) && File.Exists(keyFile))
                {
                    key = File.ReadAllText(keyFile);
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    error = $"No encuentro la clave del robot: copia ~/.explorerhat-clave de la Pi a {keyFile} o usa la variable {KeyVariable}.";
                    return null;
                }

                settings.Key = key.Trim();
            }

            return settings;
        }
    }
}
