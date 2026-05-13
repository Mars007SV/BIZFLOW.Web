using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class Product
    {
        public int Id { get; set; } // Primary key

        [Required]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty; // Product name

        [Required]
        [Display(Name = "Кількість")]
        [Range(0, double.MaxValue, ErrorMessage = "Кількість повинна бути більше або дорівнювати 0")]
        public decimal Quantity { get; set; } // Current quantity in stock

        [Required]
        [Display(Name = "Одиниця виміру")]
        public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Pieces; // Unit of measure (pieces, kg, liters)

        [Required]
        [Display(Name = "Категорія")]
        public int CategoryId { get; set; } // Foreign key to Category

        public Category? Category { get; set; } // Navigation property to related Category
    }
}