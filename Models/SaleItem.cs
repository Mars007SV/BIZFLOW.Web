using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // SaleItem represents a single line item in a sale
    // Each item shows what product was sold, quantity, and price
    public class SaleItem
    {
        // Unique identifier for this sale item
        public int Id { get; set; }

        // Foreign key to the parent sale
        [Required]
        [Display(Name = "Sale")]
        public int SaleId { get; set; }

        // Navigation property to parent sale
        public Sale? Sale { get; set; }

        // Foreign key to the product being sold
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Navigation property to the product
        public Product? Product { get; set; }

        // Quantity of this product being sold
        [Required]
        [Display(Name = "Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        // Price per unit at the time of sale
        // Stored here to preserve historical pricing
        [Required]
        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal UnitPrice { get; set; }

        // Total price for this line item (Quantity * UnitPrice)
        // Calculated property, but stored for data integrity
        [Required]
        [Display(Name = "Line Total")]
        [Range(0, double.MaxValue, ErrorMessage = "Total must be greater than or equal to 0")]
        public decimal LineTotal { get; set; }
    }
}
