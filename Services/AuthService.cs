using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BIZFLOW.Web.Services
{
    // Interface defining authentication service contract
    public interface IAuthService
    {
        Task<User?> LoginAsync(string userName, string password);
        Task<(bool Success, string Message)> RegisterAsync(string userName, string password, string? fullName);
        Task<User?> GetCurrentUserAsync(HttpContext context);
        Task LogoutAsync(HttpContext context);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    // Service handling user authentication logic
    public class AuthService : IAuthService
    {
        private readonly BizFlowDbContext _context;

        public AuthService(BizFlowDbContext context)
        {
            _context = context;
        }

        // Attempt to login user with username and password
        public async Task<User?> LoginAsync(string userName, string password)
        {
            // Find active user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);

            if (user == null)
                return null;

            // Verify password matches stored hash
            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Update last access time
            user.LastAccessAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        // Register new user account
        public async Task<(bool Success, string Message)> RegisterAsync(string userName, string password, string? fullName)
        {
            // Check if username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (existingUser != null)
            {
                return (false, "User with this name already exists");
            }

            // Create new user with hashed password
            var user = new User
            {
                UserName = userName,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                CreatedAt = DateTime.Now,
                LastAccessAt = DateTime.Now,
                IsActive = true
            };

            // Save new user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful");
        }

        // Get currently logged in user from session
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

        // Clear user session (logout)
        public async Task LogoutAsync(HttpContext context)
        {
            context.Session.Clear();
            await Task.CompletedTask;
        }

        // Hash password using SHA256 for secure storage
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Verify password matches stored hash
        public bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
