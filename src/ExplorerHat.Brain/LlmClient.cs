using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExplorerHat.Brain
{
    public record ToolCall(string Name, JsonObject Args);

    public record LlmReply(LlmServer Server, List<ToolCall> Calls, string Text);

    /// <summary>
    /// Asks a language model (Ollama, /api/chat) what to do with an order. The request is the one tools/medir-llm/medir.py
    /// measured: the same system text and tools, only the order as the user message, temperature 0, no keep_alive.
    /// If a server does not answer, it asks the next one. If none answers, the robot does not move.
    /// </summary>
    public class LlmClient
    {
        private readonly HttpClient _http;
        private readonly List<LlmServer> _servers;
        private readonly int _numCtx;
        private readonly JsonNode _tools;
        private readonly string _system;
        private readonly Action<string> _say;

        // When each server that failed will be asked again
        private readonly Dictionary<string, DateTime> _downUntil = new Dictionary<string, DateTime>();
        private static readonly TimeSpan SkipTime = TimeSpan.FromSeconds(30);

        public LlmClient(List<LlmServer> servers, int numCtx, Action<string> say)
        {
            _servers = servers;
            _numCtx = numCtx;
            _say = say;

            string path = Path.Combine(AppContext.BaseDirectory, "herramientas.json");
            var config = JsonNode.Parse(File.ReadAllText(path))!;
            _tools = config["herramientas"]!;
            _system = config["sistema"]!.GetValue<string>();

            // A server that is down or unreachable must not hold the brain: a short time to connect, a longer one to answer.
            // (The slowest answer measured was 8.5 s, with 6 clients at the same time.)
            _http = new HttpClient(new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(3) })
            {
                Timeout = TimeSpan.FromSeconds(20),
            };
        }

        public async Task<LlmReply?> AskAsync(string order)
        {
            // A server that failed is skipped for a while: waiting 3 s for it in every order would be slow. If all of them
            // failed, they are all asked again.
            var now = DateTime.UtcNow;
            var servers = _servers.Where(s => !_downUntil.TryGetValue(s.Url, out var until) || until <= now).ToList();
            if (servers.Count == 0)
            {
                servers = _servers;
            }

            foreach (var server in servers)
            {
                try
                {
                    var reply = await AskServerAsync(server, order);
                    _downUntil.Remove(server.Url);
                    return reply;
                }
                catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
                {
                    // A time-out (to connect or to answer) is a TaskCanceledException: its own text is in English and says nothing
                    string reason = exception is TaskCanceledException ? "no contesta a tiempo" : exception.Message;
                    _say($"  El servidor {server.Url} ({server.Model}) no responde: {reason}");
                    _downUntil[server.Url] = DateTime.UtcNow + SkipTime;
                }
            }

            return null;
        }

        private async Task<LlmReply> AskServerAsync(LlmServer server, string order)
        {
            var body = new JsonObject
            {
                ["model"] = server.Model,
                ["stream"] = false,
                ["tools"] = _tools.DeepClone(),
                ["options"] = new JsonObject { ["temperature"] = 0, ["seed"] = 1, ["num_ctx"] = _numCtx },
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["role"] = "system", ["content"] = _system },
                    new JsonObject { ["role"] = "user", ["content"] = order },
                },
            };

            using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(server.Url + "/api/chat", content);
            response.EnsureSuccessStatusCode();

            var reply = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
            var message = reply["message"] ?? throw new InvalidOperationException("la respuesta no tiene mensaje");
            var calls = new List<ToolCall>();
            if (message["tool_calls"] is JsonArray array)
            {
                foreach (var call in array)
                {
                    var function = call?["function"];
                    string name = function?["name"]?.GetValue<string>() ?? "";
                    calls.Add(new ToolCall(name, function?["arguments"] as JsonObject ?? new JsonObject()));
                }
            }

            return new LlmReply(server, calls, message["content"]?.GetValue<string>() ?? "");
        }
    }
}
