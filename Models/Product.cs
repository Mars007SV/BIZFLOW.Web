using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class Product
    {
        public int Id { get; set; } // Primary key

        [Required]
        public string Name { get; set; } = string.Empty; // Product name

        [Required]
        public int Quantity { get; set; } // Current quantity in stock

        [Required]
        public int CategoryId { get; set; } // Foreign key to Category

        public Category? Category { get; set; } // Navigation property to related Category
    }
}