using System.Text.Json;

namespace ExplorerHat.Orders
{
    /// <summary>An order that passed the validation, with its values already cut to the limits</summary>
    public record ValidOrder(string Name, double Seconds, double Speed, string Direction, string Color, bool On, List<string> Adjustments)
    {
        public bool IsMovement => Name is "avanzar" or "retroceder" or "girar";
    }

    /// <summary>
    /// The closed list of actions of the robot and the rules for their values. The brain (the LLM) only
    /// chooses from this list. Names, types and colors that do not fit are rejected; numbers out of range
    /// are cut to the limit and reported in <see cref="ValidOrder.Adjustments"/>.
    /// </summary>
    public static class OrderParser
    {
        public const double MinSeconds = 0.1;
        public const double MaxSeconds = 3.0;

        // Backwards the robot goes blind: the sensors look forwards
        public const double MaxSecondsBackwards = 1.0;
        public const double DefaultSpeed = 0.45;

        private static readonly string[] Colors = { "azul", "amarilla", "roja", "verde" };
        private static readonly string[] Directions = { "izquierda", "derecha" };

        /// <summary>Returns the order, or null and the reason why it was rejected</summary>
        public static ValidOrder? Parse(string json, out string error)
        {
            error = "";
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                error = "el cuerpo no es JSON";
                return null;
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("nombre", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
                {
                    error = "falta \"nombre\" (texto)";
                    return null;
                }

                var args = new Dictionary<string, JsonElement>();
                if (root.TryGetProperty("args", out var argsElement) && argsElement.ValueKind != JsonValueKind.Null)
                {
                    if (argsElement.ValueKind != JsonValueKind.Object)
                    {
                        error = "\"args\" tiene que ser un objeto";
                        return null;
                    }

                    foreach (var property in argsElement.EnumerateObject())
                    {
                        args[property.Name] = property.Value;
                    }
                }

                string name = nameElement.GetString() ?? "";
                var adjustments = new List<string>();

                switch (name)
                {
                    case "avanzar":
                    case "retroceder":
                        return Movement(name, "", args, adjustments, ref error);
                    case "girar":
                        if (!RequiredText(args, "direccion", Directions, out string direction, ref error))
                        {
                            return null;
                        }

                        return Movement(name, direction, args, adjustments, ref error);
                    case "luz":
                        if (!RequiredText(args, "color", Colors, out string color, ref error) || !RequiredBoolean(args, "encendida", out bool on, ref error))
                        {
                            return null;
                        }

                        return NoOtherArguments(args, new[] { "color", "encendida" }, ref error)
                            ? new ValidOrder(name, 0, 0, "", color, on, adjustments)
                            : null;
                    case "parar":
                        return NoOtherArguments(args, Array.Empty<string>(), ref error)
                            ? new ValidOrder(name, 0, 0, "", "", false, adjustments)
                            : null;
                    default:
                        error = $"acción desconocida: \"{name}\"";
                        return null;
                }
            }
        }

        private static ValidOrder? Movement(string name, string direction, Dictionary<string, JsonElement> args, List<string> adjustments, ref string error)
        {
            string[] allowed = name == "girar" ? new[] { "direccion", "segundos", "velocidad" } : new[] { "segundos", "velocidad" };
            if (!NoOtherArguments(args, allowed, ref error))
            {
                return null;
            }

            if (!RequiredNumber(args, "segundos", out double seconds, ref error))
            {
                return null;
            }

            double speed = DefaultSpeed;
            if (args.ContainsKey("velocidad") && !RequiredNumber(args, "velocidad", out speed, ref error))
            {
                return null;
            }

            double maxSeconds = name == "retroceder" ? MaxSecondsBackwards : MaxSeconds;
            double cutSeconds = Math.Clamp(seconds, MinSeconds, maxSeconds);
            if (cutSeconds != seconds)
            {
                adjustments.Add("segundos");
            }

            double cutSpeed = Math.Clamp(speed, 0.0, 1.0);
            if (cutSpeed != speed)
            {
                adjustments.Add("velocidad");
            }

            return new ValidOrder(name, cutSeconds, cutSpeed, direction, "", false, adjustments);
        }

        private static bool NoOtherArguments(Dictionary<string, JsonElement> args, string[] allowed, ref string error)
        {
            foreach (string key in args.Keys)
            {
                if (!allowed.Contains(key))
                {
                    error = $"argumento desconocido: \"{key}\"";
                    return false;
                }
            }

            return true;
        }

        private static bool RequiredNumber(Dictionary<string, JsonElement> args, string key, out double value, ref string error)
        {
            value = 0;
            if (!args.TryGetValue(key, out var element))
            {
                error = $"falta \"{key}\"";
                return false;
            }

            // A number written as text ("2") is rejected: the contract says number
            if (element.ValueKind != JsonValueKind.Number || !element.TryGetDouble(out value))
            {
                error = $"\"{key}\" tiene que ser un número";
                return false;
            }

            return true;
        }

        private static bool RequiredBoolean(Dictionary<string, JsonElement> args, string key, out bool value, ref string error)
        {
            value = false;
            if (!args.TryGetValue(key, out var element))
            {
                error = $"falta \"{key}\"";
                return false;
            }

            if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
            {
                error = $"\"{key}\" tiene que ser true o false";
                return false;
            }

            value = element.ValueKind == JsonValueKind.True;
            return true;
        }

        private static bool RequiredText(Dictionary<string, JsonElement> args, string key, string[] options, out string value, ref string error)
        {
            value = "";
            if (!args.TryGetValue(key, out var element))
            {
                error = $"falta \"{key}\"";
                return false;
            }

            string? text = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
            if (text is null || !options.Contains(text))
            {
                error = $"\"{key}\" tiene que ser uno de: {string.Join(", ", options)}";
                return false;
            }

            value = text;
            return true;
        }
    }
}
