using Microsoft.EntityFrameworkCore;
using BookDiscovery.Models;

namespace BookDiscovery.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ReadingList> ReadingLists { get; set; }
        public DbSet<ReadingListBook> ReadingListBooks { get; set; }

        public DbSet<SearchHistory> SearchHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<ReadingList>()
                .HasOne(rl => rl.User)
                .WithMany(u => u.ReadingLists)
                .HasForeignKey(rl => rl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReadingListBook>()
                .HasOne(rlb => rlb.ReadingList)
                .WithMany(rl => rl.ReadingListBooks)
                .HasForeignKey(rlb => rlb.ReadingListId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReadingListBook>()
                .HasOne(rlb => rlb.Book)
                .WithMany(b => b.ReadingListBooks)
                .HasForeignKey(rlb => rlb.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.OpenLibraryId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}