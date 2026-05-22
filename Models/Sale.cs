using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // Sale model represents a sales transaction (receipt/check)
    // This is created when a customer purchases products
    public class Sale
    {
        // Unique identifier for the sale
        public int Id { get; set; }

        // Unique receipt/check number for this sale
        [Required]
        [Display(Name = "Receipt Number")]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        // Date and time when the sale was made
        [Required]
        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; }

        // Optional customer name or identifier
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string? CustomerName { get; set; }

        // Total amount of the sale (sum of all items)
        [Required]
        [Display(Name = "Total Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Total must be greater than or equal to 0")]
        public decimal TotalAmount { get; set; }

        // User who created this sale
        [Display(Name = "Cashier")]
        [StringLength(100)]
        public string? CashierName { get; set; }

        // Optional notes about the sale
        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Collection of items included in this sale
        // Navigation property to related sale items
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
