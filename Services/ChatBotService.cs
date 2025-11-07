using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace project.Services
{
    public class ChatBotService
    {
        private readonly HttpClient _httpClient;

        public ChatBotService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SendMessageAsync(string message)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { message }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://web-production-d333.up.railway.app/chat/", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("reply", out JsonElement replyElement))
            {
                return replyElement.GetString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
