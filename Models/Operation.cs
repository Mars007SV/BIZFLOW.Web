using System;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class Operation
    {
        public int Id { get; set; } // Primary key

        [Required]
        [Display(Name = "Продукт")]
        public int ProductId { get; set; } // Foreign key to Product

        public Product? Product { get; set; } // Navigation property to related Product

        [Required]
        [Display(Name = "Кількість")]
        [Range(1, int.MaxValue, ErrorMessage = "Кількість повинна бути більше 0")]
        public int Quantity { get; set; } // Quantity of the operation

        [Required]
        [Display(Name = "Тип операції")]
        public string Type { get; set; } = string.Empty; // Incoming / Outgoing

        [Required]
        [Display(Name = "Дата операції")]
        public DateTime Date { get; set; } // Date of the operation

        [Display(Name = "Опис")]
        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        public string? Description { get; set; } // Description of the operation

        [Display(Name = "Користувач")]
        [StringLength(100)]
        public string? UserName { get; set; } // User who performed the operation

        [Display(Name = "Залишок після операції")]
        public int RemainingQuantity { get; set; } // Product quantity after operation
    }
}