using System;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class Operation
    {
        public int Id { get; set; } // Primary key

        [Required]
        public int ProductId { get; set; } // Foreign key to Product

        public Product? Product { get; set; } // Navigation property to related Product

        [Required]
        public int Quantity { get; set; } // Quantity of the operation

        [Required]
        public string Type { get; set; } // Incoming / Outgoing

        [Required]
        public DateTime Date { get; set; } // Date of the operation
    }
}