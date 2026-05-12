using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BIZFLOW.Web.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string userName, string password);
        Task<(bool Success, string Message)> RegisterAsync(string userName, string password, string? fullName);
        Task<User?> GetCurrentUserAsync(HttpContext context);
        Task LogoutAsync(HttpContext context);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly BizFlowDbContext _context;

        public AuthService(BizFlowDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string userName, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Оновлюємо час останнього входу
            user.LastAccessAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string userName, string password, string? fullName)
        {
            // Перевіряємо чи користувач вже існує
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (existingUser != null)
            {
                return (false, "Користувач з таким ім'ям вже існує");
            }

            // Створюємо нового користувача
            var user = new User
            {
                UserName = userName,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                CreatedAt = DateTime.Now,
                LastAccessAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Реєстрація успішна");
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext context)
        {
            var userIdString = context.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return null;

            if (int.TryParse(userIdString, out int userId))
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            }

            return null;
        }

        public async Task LogoutAsync(HttpContext context)
        {
            context.Session.Clear();
            await Task.CompletedTask;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
