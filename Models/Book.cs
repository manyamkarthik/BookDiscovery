using System.ComponentModel.DataAnnotations;

namespace BookDiscovery.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string OpenLibraryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(200)]
        public string? Author { get; set; }  // Make it nullable

        public string? Description { get; set; }  // Make it nullable

        [StringLength(500)]
        public string? CoverUrl { get; set; }  // Make it nullable

        public int? PublishYear { get; set; }

        public int? PageCount { get; set; }

        [StringLength(50)]
        public string? ISBN { get; set; }  // Make it nullable

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ReadingListBook>? ReadingListBooks { get; set; }
    }
}