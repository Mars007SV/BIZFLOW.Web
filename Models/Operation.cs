using System;

namespace BIZFLOW.Web.Models
{
    public class Operation
    {
        public int Id { get; set; } // Primary key

        public int ProductId { get; set; } // Foreign key to Product
        public Product Product { get; set; } // Navigation property to related Product

        public int Quantity { get; set; } // Quantity of the operation

        public string Type { get; set; } // Incoming / Outgoing

        public DateTime Date { get; set; } // Date of the operation
    }
}