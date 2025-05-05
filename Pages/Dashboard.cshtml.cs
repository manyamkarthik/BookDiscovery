using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookDiscovery.Data;

namespace BookDiscovery.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalBooksInDatabase { get; set; }
        public int TotalUniqueAuthors { get; set; }
        public int TotalSearches { get; set; }
        public List<string> TopSubjects { get; set; }
        public List<BooksByYear> BooksByYearData { get; set; }
        public List<PopularSearchItem> PopularSearches { get; set; }

        public async Task OnGetAsync()
        {
            TotalBooksInDatabase = await _context.Books.CountAsync();

            TotalUniqueAuthors = await _context.Books
                .Where(b => b.Author != null)
                .Select(b => b.Author)
                .Distinct()
                .CountAsync();

            TotalSearches = await _context.SearchHistories.CountAsync();

            // Get popular searches
            PopularSearches = await _context.SearchHistories
                .GroupBy(sh => sh.SearchQuery)
                .Select(g => new PopularSearchItem
                {
                    Query = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Get books by year for chart
            BooksByYearData = await _context.Books
                .Where(b => b.PublishYear.HasValue)
                .GroupBy(b => b.PublishYear.Value)
                .Select(g => new BooksByYear
                {
                    Year = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync();
        }

        public class BooksByYear
        {
            public int Year { get; set; }
            public int Count { get; set; }
        }

        public class PopularSearchItem
        {
            public string Query { get; set; }
            public int Count { get; set; }
        }
    }
}