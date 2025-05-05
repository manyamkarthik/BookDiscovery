namespace BookDiscovery.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public string SearchQuery { get; set; }
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
        public int ResultCount { get; set; }
    }
}