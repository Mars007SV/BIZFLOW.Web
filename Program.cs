using BIZFLOW.Web.Data;
using BIZFLOW.Web.Middleware;
using BIZFLOW.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Налаштування для Desktop режиму
builder.WebHost.UseUrls("http://localhost:5555");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Додаємо підтримку сесій
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Сесія на 24 години
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Реєстрація AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddDbContext<BizFlowDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseStaticFiles();
app.UseRouting();

// Додаємо сесії
app.UseSession();

// Middleware для перевірки авторизації
app.UseAuthenticationMiddleware();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Автоматичне відкриття браузера
var url = "http://localhost:5555";
OpenBrowser(url);

Console.WriteLine($"BIZFLOW запущено на {url}");
Console.WriteLine("Натисніть Ctrl+C для завершення...");

app.Run();

static void OpenBrowser(string url)
{
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
    }
    catch
    {
        // Якщо не вдалося відкрити, користувач зможе відкрити вручну
    }
}
