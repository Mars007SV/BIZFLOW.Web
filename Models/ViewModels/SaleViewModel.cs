using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models.ViewModels
{
    // ViewModel for creating a new sale
    // Used to collect data from the user interface
    public class CreateSaleViewModel
    {
        // Optional customer name
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string? CustomerName { get; set; }

        // Optional notes about the sale
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // List of items to be added to the sale
        // This will be populated dynamically in the UI
        public List<SaleItemViewModel> Items { get; set; } = new List<SaleItemViewModel>();
    }

    // ViewModel for individual sale items
    // Represents one product in the shopping cart
    public class SaleItemViewModel
    {
        // ID of the product being sold
        [Required(ErrorMessage = "Please select a product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Name of the product (for display purposes)
        public string? ProductName { get; set; }

        // Quantity being sold
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        // Price per unit (will be populated from product)
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        // Calculated total for this item
        [Display(Name = "Total")]
        public decimal Total => Quantity * UnitPrice;
    }
}
