using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models.ViewModels
{
    // ViewModel for creating a sale with multiple products
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
        public List<SaleItemViewModel> Items { get; set; } = new List<SaleItemViewModel>();
    }

    // ViewModel for individual sale items
    public class SaleItemViewModel
    {
        // ID of the product being sold
        [Required(ErrorMessage = "Please select a product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Quantity being sold
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }
    }
}
