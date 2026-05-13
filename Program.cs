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

// Створюємо окрему базу даних для кожного користувача Windows
var userDataPath = GetUserDataPath();
var dbPath = Path.Combine(userDataPath, "bizflow.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine($"📂 Директорія даних користувача: {userDataPath}");
Console.WriteLine($"🗄️ База даних: {dbPath}");

builder.Services.AddDbContext<BizFlowDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Автоматично застосовуємо міграції для бази даних користувача
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BizFlowDbContext>();

    try
    {
        Console.WriteLine("🔄 Перевірка та застосування міграцій...");
        dbContext.Database.Migrate();
        Console.WriteLine("✅ База даних готова!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Помилка при застосуванні міграцій: {ex.Message}");
    }
}

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

// Отримуємо шлях до даних користувача Windows
static string GetUserDataPath()
{
    // Отримуємо ім'я користувача Windows
    var userName = Environment.UserName;

    // Створюємо шлях: C:\Users\[UserName]\AppData\Local\BIZFLOW
    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var userDataPath = Path.Combine(appDataPath, "BIZFLOW");

    // Створюємо директорію якщо не існує
    if (!Directory.Exists(userDataPath))
    {
        Directory.CreateDirectory(userDataPath);
        Console.WriteLine($"✅ Створено директорію для користувача: {userName}");
    }

    return userDataPath;
}

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
