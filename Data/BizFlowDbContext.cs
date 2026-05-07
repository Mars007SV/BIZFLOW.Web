using Microsoft.EntityFrameworkCore;
using BIZFLOW.Web.Models;

namespace BIZFLOW.Web.Data
{
    public class BizFlowDbContext : DbContext
    {
        public BizFlowDbContext(DbContextOptions<BizFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Operation> Operations { get; set; }
    }
}