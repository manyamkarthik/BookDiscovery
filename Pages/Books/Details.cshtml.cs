using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookDiscovery.Data;
using BookDiscovery.Models;
using BookDiscovery.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookDiscovery.Pages.Books
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Book Book { get; set; }
        public BookDetails BookDetails { get; set; }
        public string CoverUrl { get; set; }
        public List<string> Authors { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync(string workId)
        {
            if (string.IsNullOrEmpty(workId))
            {
                return NotFound();
            }

            // First check if we have this book cached in our database
            Book = await _context.Books
                .FirstOrDefaultAsync(b => b.OpenLibraryId == workId);

            // Get full details from OpenLibrary API
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://openlibrary.org");

            try
            {
                var response = await client.GetStringAsync($"/works/{workId}.json");

                // Parse JSON manually to handle complex structures
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                BookDetails = new BookDetails
                {
                    Title = root.GetProperty("title").GetString(),
                    Subjects = root.TryGetProperty("subjects", out var subjects) && subjects.ValueKind == JsonValueKind.Array
                        ? subjects.EnumerateArray().Select(s => s.GetString()).ToList()
                        : new List<string>()
                };

                // Handle description which can be string or object
                if (root.TryGetProperty("description", out var description))
                {
                    if (description.ValueKind == JsonValueKind.String)
                    {
                        BookDetails.Description = description.GetString();
                    }
                    else if (description.ValueKind == JsonValueKind.Object && description.TryGetProperty("value", out var value))
                    {
                        BookDetails.Description = value.GetString();
                    }
                }

                // Handle authors
                if (root.TryGetProperty("authors", out var authors) && authors.ValueKind == JsonValueKind.Array)
                {
                    foreach (var author in authors.EnumerateArray())
                    {
                        if (author.TryGetProperty("author", out var authorObj) &&
                            authorObj.TryGetProperty("key", out var authorKey))
                        {
                            var authorId = authorKey.GetString().Replace("/authors/", "");
                            Authors.Add(authorId);
                        }
                    }
                }

                // Handle covers array
                if (root.TryGetProperty("covers", out var covers) && covers.ValueKind == JsonValueKind.Array)
                {
                    var coverIds = covers.EnumerateArray()
                        .Select(c => c.ValueKind == JsonValueKind.Number ? (int?)c.GetInt32() : null)
                        .Where(c => c.HasValue)
                        .Select(c => c.Value)
                        .ToList();

                    BookDetails.Covers = coverIds;

                    if (coverIds.Any())
                    {
                        CoverUrl = $"https://covers.openlibrary.org/b/id/{coverIds.First()}-L.jpg";
                    }
                }

                // If we don't have this book cached, create it
                if (Book == null && BookDetails != null)
                {
                    Book = new Book
                    {
                        OpenLibraryId = workId,
                        Title = BookDetails.Title,
                        Description = BookDetails.Description,
                        CoverUrl = CoverUrl,
                        Author = Authors.Any() ? string.Join(", ", Authors) : null
                    };

                    _context.Books.Add(Book);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book details for work ID: {WorkId}", workId);

                // If we have a cached book, show it even if API fails
                if (Book != null)
                {
                    return Page();
                }

                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetExportAsync(string workId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.OpenLibraryId == workId);
            if (book == null) return NotFound();

            var bookInfo = $"Title: {book.Title}\n" +
                           $"Author: {book.Author}\n" +
                           $"Description: {book.Description}\n" +
                           $"First Published: {book.PublishYear}\n";

            return File(System.Text.Encoding.UTF8.GetBytes(bookInfo), "text/plain", $"{book.Title}.txt");
        }
    }

    // Simplified DTOs
    public class BookDetails
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<int> Covers { get; set; }
        public List<string> Subjects { get; set; }
    }
}