using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookDiscovery.Data;
using BookDiscovery.Models;
using BookDiscovery.Services;

namespace BookDiscovery.Pages
{
    public class IndexModel : PageModel
    {
        private readonly OpenLibraryService _openLibraryService;
        private readonly ApplicationDbContext _context;

        public IndexModel(OpenLibraryService openLibraryService, ApplicationDbContext context)
        {
            _openLibraryService = openLibraryService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public BookSearchResult SearchResults { get; set; }
        public List<PopularSearch> PopularSearches { get; set; }
        public List<Book> RecentBooks { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get popular searches
            PopularSearches = await _context.SearchHistories
                .GroupBy(sh => sh.SearchQuery)
                .Select(g => new PopularSearch
                {
                    Query = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Get recent books
            RecentBooks = await _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults = await _openLibraryService.SearchBooksAsync(SearchQuery);

                // Track search history
                var searchHistory = new SearchHistory
                {
                    SearchQuery = SearchQuery,
                    ResultCount = SearchResults?.NumFound ?? 0
                };

                _context.SearchHistories.Add(searchHistory);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public class PopularSearch
        {
            public string Query { get; set; }
            public int Count { get; set; }
        }
    }
}