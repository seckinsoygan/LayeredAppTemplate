using LayeredAppTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredAppTemplate.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        // Yeni entity eklemek: buraya ekle
        // public DbSet<Product> Products => Set<Product>();
    }
}
