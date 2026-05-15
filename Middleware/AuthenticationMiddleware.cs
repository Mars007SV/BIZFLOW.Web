using BIZFLOW.Web.Services;

namespace BIZFLOW.Web.Middleware
{
    // Middleware to check user authentication on each request
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // List of public pages that don't require authentication
            var publicPaths = new[] 
            { 
                "/account/login", 
                "/account/register",
                "/account/logout",
                "/lib/",     // JavaScript libraries
                "/css/",     // Stylesheets
                "/js/",      // JavaScript files
                "/favicon.ico"
            };

            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Check if current path is public (no authentication needed)
            var isPublicPath = publicPaths.Any(p => path.StartsWith(p));

            if (!isPublicPath)
            {
                // Check if user is authenticated
                var currentUser = await authService.GetCurrentUserAsync(context);

                if (currentUser == null)
                {
                    // Redirect to login page if not authenticated
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            // Continue to next middleware
            await _next(context);
        }
    }

    // Extension method to easily add middleware to pipeline
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
