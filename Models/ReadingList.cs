using System.ComponentModel.DataAnnotations;

namespace BookDiscovery.Models
{
    public class ReadingList
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }

        public ICollection<ReadingListBook> ReadingListBooks { get; set; }
    }
}