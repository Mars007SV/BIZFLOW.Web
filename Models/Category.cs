using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // Category model for organizing products into groups
    public class Category
    {
        // Unique identifier
        public int Id { get; set; }

        // Category name (required)
        [Required]
        public string Name { get; set; } = string.Empty;

        // Collection of products in this category
        public List<Product>? Products { get; set; }
    }
}
