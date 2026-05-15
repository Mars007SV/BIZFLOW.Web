using BIZFLOW.Web.Data;
using BIZFLOW.Web.Middleware;
using BIZFLOW.Web.Services;
using Microsoft.EntityFrameworkCore;
using ElectronNET.API;
using ElectronNET.API.Entities;

// Create web application builder
var builder = WebApplication.CreateBuilder(args);

// Add Electron.NET support for desktop app
builder.WebHost.UseElectron(args);

// Configure default ports (if not launched through Visual Studio)
if (!builder.Environment.IsDevelopment() || args.Contains("--launch-profile"))
{
    builder.WebHost.UseUrls("http://localhost:5555");
}

// Add services to the container
builder.Services.AddControllersWithViews();

// Add session support for user authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Session timeout: 24 hours
    options.Cookie.HttpOnly = true; // Protect from XSS attacks
    options.Cookie.IsEssential = true; // Essential for app functionality
});

// Register AuthService for authentication
builder.Services.AddScoped<IAuthService, AuthService>();

// Register ReportService for report generation
builder.Services.AddScoped<IReportService, ReportService>();

// Create separate database for each Windows user
// This ensures data isolation between different Windows accounts
var userDataPath = GetUserDataPath();
var dbPath = Path.Combine(userDataPath, "bizflow.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine($"User data directory: {userDataPath}");
Console.WriteLine($"Database: {dbPath}");

// Configure Entity Framework with SQLite database
builder.Services.AddDbContext<BizFlowDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Automatically apply database migrations for user's database
// This runs when application starts to ensure database is up to date
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BizFlowDbContext>();

    try
    {
        Console.WriteLine("Checking and applying migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database ready!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseStaticFiles();
app.UseRouting();

// Enable sessions
app.UseSession();

// Custom middleware to check user authorization
app.UseAuthenticationMiddleware();

app.UseAuthorization();

app.MapStaticAssets();

// Configure default route pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Configure Electron Desktop window if running as desktop app
await ConfigureElectronWindow();

app.Run();

// Configure Electron Desktop window and browser launch
async Task ConfigureElectronWindow()
{
    // Check if running as Electron desktop app
    if (HybridSupport.IsElectronActive)
    {
        // Create desktop window with specific settings
        var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
        {
            Width = 1400,
            Height = 900,
            Title = "BIZFLOW - Business Management",
            Icon = "/favicon.ico",
            WebPreferences = new WebPreferences
            {
                NodeIntegration = false, // Disable Node.js integration for security
                ContextIsolation = true // Enable context isolation for security
            },
            AutoHideMenuBar = true, // Hide menu bar for cleaner UI
            Center = true // Center window on screen
        });

        // Open DevTools only in development mode for debugging
        if (builder.Environment.IsDevelopment())
        {
            browserWindow.WebContents.OpenDevTools();
        }

        // Handle window close event
        browserWindow.OnClosed += () =>
        {
            Electron.App.Quit();
        };

        Console.WriteLine("BIZFLOW Desktop launched!");
    }
    else
    {
        // Check if launched through Visual Studio
        var isVisualStudio = args.Any(a => a.Contains("--launch-profile")) || 
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        if (!isVisualStudio)
        {
            // Launched via .bat or .exe - open browser automatically
            var url = "http://localhost:5555";
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"Waiting for server to start on {url}...");

                    // Wait for server to fully start
                    await Task.Delay(3000);

                    Console.WriteLine($"Opening browser: {url}");

                    // Open browser based on operating system
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

                    Console.WriteLine("Browser opened!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open browser: {ex.Message}");
                    Console.WriteLine($"Open browser manually: {url}");
                }
            });
        }
        else
        {
            // Launched through Visual Studio - browser will open via launchSettings.json
            Console.WriteLine("Running in web application mode (Visual Studio)");
        }
    }
}

// Get Windows user data path for database storage
// This ensures each Windows user has their own database
static string GetUserDataPath()
{
    // Get Windows username
    var userName = Environment.UserName;

    // Create path: C:\Users\[UserName]\AppData\Local\BIZFLOW
    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var userDataPath = Path.Combine(appDataPath, "BIZFLOW");

    // Create directory if it doesn't exist
    if (!Directory.Exists(userDataPath))
    {
        Directory.CreateDirectory(userDataPath);
        Console.WriteLine($"Created directory for user: {userName}");
    }

    return userDataPath;
}
