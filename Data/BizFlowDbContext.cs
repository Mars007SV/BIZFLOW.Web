using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Models;

namespace BIZFLOW.Web.Data
{
    // Database context for Entity Framework Core
    // Manages connection to SQLite database and entity sets
    public class BizFlowDbContext : DbContext
    {
        public BizFlowDbContext(DbContextOptions<BizFlowDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<User> Users { get; set; }

        // Configure database model and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create unique index on UserName to prevent duplicates
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }
    }
}