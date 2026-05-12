using BIZFLOW.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BIZFLOW.Web.Middleware
{
    public class UserInitializationMiddleware
    {
        private readonly RequestDelegate _next;

        public UserInitializationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, BizFlowDbContext dbContext)
        {
            // Перевіряємо чи є cookie з DeviceId
            if (!context.Request.Cookies.ContainsKey("DeviceId"))
            {
                // Генеруємо унікальний DeviceId на основі інформації про ПК
                var deviceId = GenerateDeviceId(context);

                // Зберігаємо DeviceId в cookie (термін дії 10 років)
                context.Response.Cookies.Append("DeviceId", deviceId, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(10),
                    HttpOnly = true,
                    Secure = false, // Для localhost можна false
                    SameSite = SameSiteMode.Lax
                });

                // Створюємо нового користувача в базі даних
                await CreateOrUpdateUser(dbContext, deviceId);
            }
            else
            {
                // Оновлюємо час останнього доступу
                var deviceId = context.Request.Cookies["DeviceId"];
                if (!string.IsNullOrEmpty(deviceId))
                {
                    await CreateOrUpdateUser(dbContext, deviceId);
                }
            }

            await _next(context);
        }

        private string GenerateDeviceId(HttpContext context)
        {
            // Комбінуємо різні параметри для створення унікального ID
            var sb = new StringBuilder();
            sb.Append(context.Request.Headers["User-Agent"].ToString());
            sb.Append(context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            sb.Append(Environment.MachineName);
            sb.Append(Environment.UserName);
            sb.Append(DateTime.UtcNow.Ticks); // Додаємо час для унікальності

            // Хешуємо для отримання короткого унікального ID
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Substring(0, 32);
        }

        private async Task CreateOrUpdateUser(BizFlowDbContext dbContext, string deviceId)
        {
            try
            {
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.DeviceId == deviceId);

                if (user == null)
                {
                    // Створюємо нового користувача
                    user = new Models.User
                    {
                        DeviceId = deviceId,
                        UserName = $"User_{DateTime.Now:yyyyMMddHHmmss}",
                        CreatedAt = DateTime.Now,
                        LastAccessAt = DateTime.Now
                    };

                    dbContext.Users.Add(user);
                }
                else
                {
                    // Оновлюємо час останнього доступу
                    user.LastAccessAt = DateTime.Now;
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Логуємо помилку, але не блокуємо роботу додатку
                Console.WriteLine($"Error in UserInitializationMiddleware: {ex.Message}");
            }
        }
    }

    // Extension method для зручного підключення middleware
    public static class UserInitializationMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserInitialization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserInitializationMiddleware>();
        }
    }
}
