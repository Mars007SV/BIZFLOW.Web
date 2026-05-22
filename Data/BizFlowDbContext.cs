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
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        // Configure database model and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create unique index on UserName to prevent duplicates
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // Configure Sale to SaleItem relationship (one-to-many)
            modelBuilder.Entity<Sale>()
                .HasMany(s => s.SaleItems)
                .WithOne(si => si.Sale)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SaleItem to Product relationship
            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}