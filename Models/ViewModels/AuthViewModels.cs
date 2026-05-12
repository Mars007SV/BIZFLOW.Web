using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть ім'я користувача")]
        [Display(Name = "Ім'я користувача")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введіть ім'я користувача")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача має бути від 3 до 50 символів")]
        [Display(Name = "Ім'я користувача")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Повне ім'я (необов'язково)")]
        [StringLength(100)]
        public string? FullName { get; set; }
    }
}
