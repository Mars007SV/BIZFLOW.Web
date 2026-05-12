# Система автоматичного створення користувачів

## Опис

Система автоматично створює унікального користувача для кожного ПК при першому відвідуванні додатку. Дані користувача зберігаються в базі даних SQLite.

## Як це працює

1. **При першому запуску** - middleware перевіряє наявність cookie "DeviceId"
2. **Генерація унікального ID** - якщо cookie немає, генерується унікальний DeviceId на основі:
   - User-Agent браузера
   - IP-адреси
   - Імені комп'ютера
   - Імені користувача Windows
3. **Створення користувача** - новий користувач автоматично додається до БД
4. **Збереження cookie** - DeviceId зберігається в cookie на 10 років
5. **Подальші візити** - при наступних відвідуваннях система розпізнає користувача та оновлює час останнього доступу

## Структура даних користувача

```csharp
public class User
{
    public int Id { get; set; }                    // Унікальний ID в БД
    public string DeviceId { get; set; }           // Унікальний ID пристрою
    public string? UserName { get; set; }          // Ім'я користувача (можна редагувати)
    public DateTime CreatedAt { get; set; }        // Дата створення
    public DateTime LastAccessAt { get; set; }     // Останній вхід
    public string? Preferences { get; set; }       // Додаткові налаштування (JSON)
}
```

## Доступні функції

### Для користувача:
- `/User/Profile` - перегляд та редагування профілю
- `/User/GetCurrentUser` - API для отримання даних поточного користувача

### Для адміністратора:
- `/User/Index` - список всіх користувачів системи

## Використання в коді

### Отримання поточного користувача в контролері:

```csharp
public class MyController : Controller
{
    private readonly IUserService _userService;

    public MyController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> MyAction()
    {
        // Отримати весь об'єкт користувача
        var user = await _userService.GetCurrentUserAsync(HttpContext);

        // Або тільки ID
        var userId = await _userService.GetCurrentUserIdAsync(HttpContext);

        return View();
    }
}
```

### Отримання через HttpContext напряму:

```csharp
var deviceId = HttpContext.Request.Cookies["DeviceId"];
if (!string.IsNullOrEmpty(deviceId))
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
}
```

## Налаштування

Middleware підключається в `Program.cs`:

```csharp
app.UseUserInitialization(); // Додати після UseRouting()
```

Сервіс реєструється:

```csharp
builder.Services.AddScoped<IUserService, UserService>();
```

## Безпека

- DeviceId хешується за допомогою SHA256
- Cookie має HttpOnly прапорець (захист від XSS)
- Унікальність DeviceId забезпечується індексом в БД

## Розширення

Поле `Preferences` можна використовувати для зберігання додаткових налаштувань користувача в форматі JSON:

```csharp
user.Preferences = JsonSerializer.Serialize(new 
{
    Theme = "dark",
    Language = "uk",
    PageSize = 50
});
```

## Міграції

Для створення таблиці користувачів виконано:

```bash
dotnet ef migrations add AddUserTable
dotnet ef database update
```
