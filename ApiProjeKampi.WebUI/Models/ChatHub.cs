using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiProjeKampi.WebUI.Models
{
    public class ChatHub : Hub
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string modelGemini = "gemini-1.5-flash";

        private static readonly Dictionary<string, List<Dictionary<string, string>>> _history = new();

        public ChatHub(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override Task OnConnectedAsync()
        {
            _history[Context.ConnectionId] = new List<Dictionary<string, string>>
            {
                new() { ["role"] = "system", ["content"] = "You are a helpful assistant. Keep answers concise." }
            };
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _history.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string userMessage)
        {
            await Clients.Caller.SendAsync("ReceiveUserEcho", userMessage);

            var history = _history[Context.ConnectionId];
            history.Add(new() { ["role"] = "user", ["content"] = userMessage });

            await CallGeminiOnce(history, Context.ConnectionAborted);
        }

        private async Task CallGeminiOnce(List<Dictionary<string, string>> history, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();

            // ApiKey’i senin istediğin gibi buradan alıyoruz
            var apiKey = Context.GetHttpContext()!
                                .RequestServices
                                .GetService<IConfiguration>()?["GoogleApiKey"];

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelGemini}:generateContent?key={apiKey}";

            var contents = history.Select(h => new
            {
                role = h["role"] == "assistant" ? "model" : "user",
                parts = new[] { new { text = h["content"] } }
            });

            var payload = new { contents };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, cancellationToken);
            resp.EnsureSuccessStatusCode();

            var respString = await resp.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<GeminiResponse>(respString);

            var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (!string.IsNullOrEmpty(text))
            {
                // Cevabı hem history’e ekliyoruz hem de SignalR ile gönderiyoruz
                history.Add(new() { ["role"] = "assistant", ["content"] = text });

                // Token mantığı olmadığı için komple mesajı gönderiyoruz
                await Clients.Caller.SendAsync("ReceiveToken", text, cancellationToken);
                await Clients.Caller.SendAsync("CompleteMessage", text, cancellationToken);
            }
            else
            {
                await Clients.Caller.SendAsync("CompleteMessage", "(Gemini'den boş cevap döndü)", cancellationToken);
            }
        }

        // Gemini response modeli
        private sealed class GeminiResponse
        {
            [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; }
        }

        private sealed class Candidate
        {
            [JsonPropertyName("content")] public GeminiContent? Content { get; set; }
        }

        private sealed class GeminiContent
        {
            [JsonPropertyName("parts")] public List<GeminiPart>? Parts { get; set; }
        }

        private sealed class GeminiPart
        {
            [JsonPropertyName("text")] public string? Text { get; set; }
        }
    }
}
