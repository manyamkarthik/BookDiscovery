using Microsoft.EntityFrameworkCore;
using BookDiscovery.Models;

namespace BookDiscovery.Data
{
    public class PostgreSqlDbContext : ApplicationDbContext
    {
        public PostgreSqlDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=temp;Username=temp;Password=temp");
            }
        }
    }
}