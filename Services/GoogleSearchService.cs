using System.Net.Http;
using System.Text.Json;

namespace project.Services
{
    public class GoogleSearchService
    {
        private readonly string _apiKey;
        private readonly string _searchEngineId;
        private readonly HttpClient _httpClient;

        public GoogleSearchService(string apiKey, string searchEngineId)
        {
            _apiKey = apiKey;
            _searchEngineId = searchEngineId;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Выполняет поиск через Google Custom Search API
        /// </summary>
        /// <param name="query">Поисковый запрос</param>
        /// <returns>Список строк с результатами</returns>
        public async Task<List<string>> SearchAsync(string query)
        {
            string url = $"https://www.googleapis.com/customsearch/v1" +
                         $"?key={_apiKey}" +
                         $"&cx={_searchEngineId}" +
                         $"&q={Uri.EscapeDataString(query)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var results = new List<string>();

            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray().Take(3)) // первые 3 результата
                {
                    string title = item.GetProperty("title").GetString() ?? "";
                    string snippet = item.GetProperty("snippet").GetString() ?? "";
                    string link = item.GetProperty("link").GetString() ?? "";

                    results.Add($"{title}\n{snippet}\n{link}");
                }
            }

            return results;
        }
    }
}
