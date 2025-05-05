using System.Text.Json;
using System.Text.Json.Serialization;
using BookDiscovery.Models;

namespace BookDiscovery.Services
{
    public class OpenLibraryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenLibraryService> _logger;

        public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://openlibrary.org");
        }

        public async Task<BookSearchResult> SearchBooksAsync(string query, int page = 1, int limit = 20)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(
                    $"/search.json?q={Uri.EscapeDataString(query)}&page={page}&limit={limit}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Deserialize<BookSearchResult>(response, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books for query: {Query}", query);
                throw;
            }
        }
    }

    // Updated DTO classes to match OpenLibrary API
    public class BookSearchResult
    {
        [JsonPropertyName("numFound")]
        public int NumFound { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("docs")]
        public List<BookSearchItem> Docs { get; set; }
    }

    public class BookSearchItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string> Author_Name { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? First_Publish_Year { get; set; }

        [JsonPropertyName("isbn")]
        public List<string> Isbn { get; set; }

        [JsonPropertyName("cover_i")]
        public int? Cover_I { get; set; }  // Changed from string to int?

        [JsonPropertyName("number_of_pages_median")]
        public int? Number_Of_Pages_Median { get; set; }
    }
}