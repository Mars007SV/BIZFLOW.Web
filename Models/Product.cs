using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // Product model represents items in warehouse inventory
    public class Product
    {
        // Unique identifier for product
        public int Id { get; set; }

        // Product name (required field)
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        // Current quantity available in stock
        [Required]
        [Display(Name = "Quantity")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
        public decimal Quantity { get; set; }

        // Unit of measure (pieces, kg, liters, etc.)
        [Required]
        [Display(Name = "Unit of Measure")]
        public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Pieces;

        // Foreign key linking to category
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Navigation property to access related category
        public Category? Category { get; set; }

        // Foreign key linking to user (owner of this product)
        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        // Navigation property to access related user
        public User? User { get; set; }
    }
}