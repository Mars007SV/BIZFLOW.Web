using BIZFLOW.Web.Services;

namespace BIZFLOW.Web.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Список публічних сторінок (без авторизації)
            var publicPaths = new[] 
            { 
                "/account/login", 
                "/account/register",
                "/account/logout",
                "/lib/",
                "/css/",
                "/js/",
                "/favicon.ico"
            };

            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Перевіряємо чи це публічна сторінка
            var isPublicPath = publicPaths.Any(p => path.StartsWith(p));

            if (!isPublicPath)
            {
                // Перевіряємо чи користувач авторизований
                var currentUser = await authService.GetCurrentUserAsync(context);

                if (currentUser == null)
                {
                    // Перенаправляємо на сторінку входу
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
