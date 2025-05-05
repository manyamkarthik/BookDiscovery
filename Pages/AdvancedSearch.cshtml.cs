using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookDiscovery.Services;

namespace BookDiscovery.Pages
{
    public class AdvancedSearchModel : PageModel
    {
        private readonly OpenLibraryService _openLibraryService;

        public AdvancedSearchModel(OpenLibraryService openLibraryService)
        {
            _openLibraryService = openLibraryService;
        }

        [BindProperty(SupportsGet = true)]
        public string Title { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Author { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Subject { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ISBN { get; set; }

        public BookSearchResult SearchResults { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Title) ||
                !string.IsNullOrWhiteSpace(Author) ||
                !string.IsNullOrWhiteSpace(Subject) ||
                !string.IsNullOrWhiteSpace(ISBN))
            {
                var searchParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(Title))
                    searchParams.Add($"title:{Title}");

                if (!string.IsNullOrWhiteSpace(Author))
                    searchParams.Add($"author:{Author}");

                if (!string.IsNullOrWhiteSpace(Subject))
                    searchParams.Add($"subject:{Subject}");

                if (!string.IsNullOrWhiteSpace(ISBN))
                    searchParams.Add($"isbn:{ISBN}");

                var query = string.Join(" ", searchParams);
                SearchResults = await _openLibraryService.SearchBooksAsync(query);
            }

            return Page();
        }
    }
}