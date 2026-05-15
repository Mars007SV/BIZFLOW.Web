using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    // User model for authentication and user management
    public class User
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // Unique username for login (required, max 50 characters)
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        // Hashed password (SHA256) for security
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // User's full name (optional, max 100 characters)
        [MaxLength(100)]
        public string? FullName { get; set; }

        // Account creation timestamp
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Last login timestamp
        public DateTime LastAccessAt { get; set; } = DateTime.Now;

        // Account status (active/inactive)
        public bool IsActive { get; set; } = true;

        // Additional user preferences stored as JSON
        public string? Preferences { get; set; }
    }
}
