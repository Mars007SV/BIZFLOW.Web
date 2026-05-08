using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class Category
    {
        public int Id { get; set; } // Primary key

        [Required]
        public string Name { get; set; } // Category name

        public List<Product>? Products { get; set; } // Navigation property to related products
    }
}
