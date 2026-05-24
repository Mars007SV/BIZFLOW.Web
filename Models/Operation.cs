using System;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // Operation model tracks all product movements (incoming/outgoing)
    public class Operation
    {
        // Unique identifier
        public int Id { get; set; }

        // Foreign key to product
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Navigation property to related product
        public Product? Product { get; set; }

        // Quantity involved in this operation
        [Required]
        [Display(Name = "Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        // Type of operation (Incoming/Outgoing)
        [Required]
        [Display(Name = "Operation Type")]
        public string Type { get; set; } = string.Empty;

        // Date when operation was performed
        [Required]
        [Display(Name = "Operation Date")]
        public DateTime Date { get; set; }

        // Optional description of the operation
        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // User who performed the operation (display name)
        [Display(Name = "User")]
        [StringLength(100)]
        public string? UserName { get; set; }

        // Product quantity remaining after this operation
        [Display(Name = "Remaining Quantity")]
        public decimal RemainingQuantity { get; set; }

        // Foreign key linking to user (owner of this operation)
        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        // Navigation property to access related user
        public User? User { get; set; }
    }
}