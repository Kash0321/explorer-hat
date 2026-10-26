using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExplorerHat.Brain
{
    /// <summary>The answer of the order service. Status 0 means that it did not answer at all.</summary>
    public record RobotReply(int Status, JsonNode? Body);

    /// <summary>The order service of the Raspberry Pi (src/ExplorerHat.Orders). The safety is there: this only sends what it is told.</summary>
    public class RobotClient
    {
        private readonly HttpClient _http;
        private readonly HttpClient _fast;

        public RobotClient(string url, string key)
        {
            // An action lasts 3 seconds at most. The stop has a shorter time: it must not wait for a robot that does not answer.
            _http = Create(url, key, 10);
            _fast = Create(url, key, 3);
        }

        private static HttpClient Create(string url, string key, int seconds)
        {
            var client = new HttpClient(new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(3) })
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(seconds),
            };
            client.DefaultRequestHeaders.Add("X-Clave", key);
            return client;
        }

        /// <summary>Sends one action and waits until it ends</summary>
        public async Task<RobotReply> ActionAsync(string name, JsonObject args)
        {
            var body = new JsonObject { ["nombre"] = name, ["args"] = args.DeepClone() };
            using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            return await SendAsync(_http, "/accion", content);
        }

        /// <summary>Stops the motors at once. It is safe to call it again and again</summary>
        public async Task<RobotReply> StopAsync()
        {
            using var content = new StringContent("", Encoding.UTF8, "application/json");
            return await SendAsync(_fast, "/parar", content);
        }

        private static async Task<RobotReply> SendAsync(HttpClient client, string path, HttpContent content)
        {
            try
            {
                using var response = await client.PostAsync(path, content);
                string text = await response.Content.ReadAsStringAsync();
                JsonNode? body = null;
                try
                {
                    body = JsonNode.Parse(text);
                }
                catch (JsonException)
                {
                    body = new JsonObject { ["error"] = text };
                }

                return new RobotReply((int)response.StatusCode, body);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                return new RobotReply(0, new JsonObject { ["error"] = "el robot no responde (" + exception.Message + ")" });
            }
        }
    }
}
