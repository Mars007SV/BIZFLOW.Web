using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastAccessAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Додаткові налаштування користувача
        public string? Preferences { get; set; }
    }
}
