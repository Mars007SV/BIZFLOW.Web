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
        public DbSet<Product> Products { get; set; } // Represents the Products table
        public DbSet<Category> Categories { get; set; } // Represents the Categories table
        public DbSet<Operation> Operations { get; set; } // Represents the Operations table
        public DbSet<User> Users { get; set; } // Represents the Users table

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