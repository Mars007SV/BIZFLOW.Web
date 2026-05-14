using BIZFLOW.Web.Data;
using BIZFLOW.Web.Middleware;
using BIZFLOW.Web.Services;
using Microsoft.EntityFrameworkCore;
using ElectronNET.API;
using ElectronNET.API.Entities;

var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку Electron.NET
builder.WebHost.UseElectron(args);

// Налаштовуємо порти за замовчуванням (якщо не запущено через Visual Studio)
if (!builder.Environment.IsDevelopment() || args.Contains("--launch-profile"))
{
    builder.WebHost.UseUrls("http://localhost:5555");
}

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

// Реєстрація ReportService
builder.Services.AddScoped<IReportService, ReportService>();

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

// Налаштування Electron Desktop вікна
await ConfigureElectronWindow();

app.Run();

// Налаштування Electron Desktop
async Task ConfigureElectronWindow()
{
    if (HybridSupport.IsElectronActive)
    {
        var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
        {
            Width = 1400,
            Height = 900,
            Title = "BIZFLOW - Бізнес Управління",
            Icon = "/favicon.ico",
            WebPreferences = new WebPreferences
            {
                NodeIntegration = false,
                ContextIsolation = true
            },
            AutoHideMenuBar = true, // Приховати меню
            Center = true
        });

        // Відкрити DevTools тільки в режимі розробки
        if (builder.Environment.IsDevelopment())
        {
            browserWindow.WebContents.OpenDevTools();
        }

        // Обробка закриття вікна
        browserWindow.OnClosed += () =>
        {
            Electron.App.Quit();
        };

        Console.WriteLine("✅ BIZFLOW Desktop запущено!");
    }
    else
    {
        // Якщо Electron не активний, перевіряємо чи запущено через Visual Studio
        var isVisualStudio = args.Any(a => a.Contains("--launch-profile")) || 
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (!isVisualStudio)
        {
            // Запущено через .bat або .exe - відкриваємо браузер автоматично
            var url = "http://localhost:5555";
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"🌐 Очікування запуску сервера на {url}...");

                    // Чекаємо поки сервер запуститься
                    await Task.Delay(3000);

                    Console.WriteLine($"🌐 Відкриття браузера: {url}");

                    // Відкриваємо браузер
                    if (OperatingSystem.IsWindows())
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        System.Diagnostics.Process.Start("xdg-open", url);
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        System.Diagnostics.Process.Start("open", url);
                    }

                    Console.WriteLine("✅ Браузер відкрито!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Не вдалося відкрити браузер: {ex.Message}");
                    Console.WriteLine($"Відкрийте браузер вручну: {url}");
                }
            });
        }
        else
        {
            // Запущено через Visual Studio - браузер відкриється через launchSettings.json
            Console.WriteLine("🌐 Запуск у режимі веб-застосунку (Visual Studio)");
        }
    }
}

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
