namespace BIZFLOW.Web.Models
{
    public class Product
    {
        public int Id { get; set; } // Primary key
        public string Name { get; set; } // Product name
        public int Quantity { get; set; } // Current quantity in stock

        public int CategoryId { get; set; } // Foreign key to Category
        public Category Category { get; set; } // Navigation property to related Category
    }
}