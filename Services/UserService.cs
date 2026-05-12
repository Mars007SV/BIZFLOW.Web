using BIZFLOW.Web.Data;
using BIZFLOW.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BIZFLOW.Web.Services
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync(HttpContext context);
        Task<int?> GetCurrentUserIdAsync(HttpContext context);
    }

    public class UserService : IUserService
    {
        private readonly BizFlowDbContext _context;

        public UserService(BizFlowDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext context)
        {
            var deviceId = context.Request.Cookies["DeviceId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
        }

        public async Task<int?> GetCurrentUserIdAsync(HttpContext context)
        {
            var user = await GetCurrentUserAsync(context);
            return user?.Id;
        }
    }
}
