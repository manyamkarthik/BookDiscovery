using System.ComponentModel.DataAnnotations;

namespace BookDiscovery.Models
{
    public class ReadingListBook
    {
        public int Id { get; set; }

        public int ReadingListId { get; set; }

        public int BookId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Want to Read", "Reading", "Completed"

        public string Notes { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public ReadingList ReadingList { get; set; }

        public Book Book { get; set; }
    }
}