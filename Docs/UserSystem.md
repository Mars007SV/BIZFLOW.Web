# Система автентифікації користувачів

## Опис

Система вимагає від користувача входу або реєстрації при першому запуску додатку. Дані користувачів зберігаються в базі даних SQLite з безпечним хешуванням паролів.

## Як це працює

1. **При запуску додатку** - middleware перевіряє наявність активної сесії користувача
2. **Без сесії** - користувач перенаправляється на сторінку входу
3. **Вхід або реєстрація** - користувач може увійти або створити новий обліковий запис
4. **Збереження сесії** - після успішного входу створюється сесія на 24 години
5. **Доступ до системи** - користувач може користуватися всіма функціями BIZFLOW

## Структура даних користувача

```csharp
public class User
{
    public int Id { get; set; }                    // Унікальний ID в БД
    public string UserName { get; set; }           // Ім'я користувача (унікальне)
    public string PasswordHash { get; set; }       // Хеш паролю (SHA256)
    public string? FullName { get; set; }          // Повне ім'я (необов'язково)
    public DateTime CreatedAt { get; set; }        // Дата створення
    public DateTime LastAccessAt { get; set; }     // Останній вхід
    public bool IsActive { get; set; }             // Активний чи ні
    public string? Preferences { get; set; }       // Додаткові налаштування (JSON)
}
```

## Доступні сторінки

### Публічні (без авторизації):
- `/Account/Login` - вхід в систему
- `/Account/Register` - реєстрація нового користувача
- `/Account/Logout` - вихід з системи

### Захищені (потрібна авторизація):
- Всі інші сторінки системи
- `/User/Profile` - перегляд та редагування профілю
- `/User/Index` - список всіх користувачів (адміністрування)

## Використання в коді

### Отримання поточного користувача в контролері:

```csharp
public class MyController : Controller
{
    private readonly IAuthService _authService;

    public MyController(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> MyAction()
    {
        // Отримати поточного користувача
        var user = await _authService.GetCurrentUserAsync(HttpContext);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Використовуємо дані користувача
        ViewBag.UserName = user.UserName;

        return View();
    }
}
```

### Реєстрація нового користувача:

```csharp
var result = await _authService.RegisterAsync("username", "password", "Full Name");
if (result.Success)
{
    // Реєстрація успішна
}
else
{
    // Помилка: result.Message
}
```

### Вхід користувача:

```csharp
var user = await _authService.LoginAsync("username", "password");
if (user != null)
{
    // Вхід успішний, зберігаємо в сесії
    HttpContext.Session.SetString("UserId", user.Id.ToString());
}
```

## Налаштування

Middleware підключається в `Program.cs`:

```csharp
// Додаємо підтримку сесій
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Реєстрація AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// В pipeline додаємо
app.UseSession();
app.UseAuthenticationMiddleware();
```

## Безпека

- **Хешування паролів** - використовується SHA256 для зберігання паролів
- **Перевірка унікальності** - ім'я користувача має бути унікальним (індекс в БД)
- **Захист сесій** - HttpOnly cookies для запобігання XSS атакам
- **Валідація даних** - перевірка всіх вхідних даних через Data Annotations
- **Мінімальна довжина паролю** - не менше 6 символів

## Вимоги до реєстрації

- **Ім'я користувача**: 3-50 символів, унікальне
- **Пароль**: мінімум 6 символів
- **Підтвердження паролю**: має співпадати з паролем
- **Повне ім'я**: необов'язково, до 100 символів

## Розширення

Поле `Preferences` можна використовувати для зберігання додаткових налаштувань користувача в форматі JSON:

```csharp
user.Preferences = JsonSerializer.Serialize(new 
{
    Theme = "dark",
    Language = "uk",
    PageSize = 50,
    EmailNotifications = true
});
```

## Міграції

Для створення системи авторизації виконано:

```bash
dotnet ef migrations add UpdateUserTableForAuthentication
dotnet ef database update
```

## Функції AuthService

- `LoginAsync(userName, password)` - вхід користувача
- `RegisterAsync(userName, password, fullName)` - реєстрація
- `GetCurrentUserAsync(HttpContext)` - отримання поточного користувача
- `LogoutAsync(HttpContext)` - вихід з системи
- `HashPassword(password)` - хешування паролю
- `VerifyPassword(password, hash)` - перевірка паролю

