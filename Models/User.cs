using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastAccessAt { get; set; } = DateTime.Now;

        // Додаткові налаштування користувача
        public string? Preferences { get; set; }
    }
}
