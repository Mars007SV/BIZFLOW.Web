# 🔧 Технічні деталі системи авторизації

## Архітектура

```
┌─────────────────┐
│   Користувач    │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐
│  AuthenticationMiddleware│ ◄── Перевіряє сесію
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│   AccountController     │ ◄── Login/Register/Logout
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│     AuthService         │ ◄── Бізнес-логіка авторизації
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│   BizFlowDbContext      │ ◄── База даних (SQLite)
└─────────────────────────┘
```

## Компоненти системи

### 1. Models
- **User.cs** - модель користувача
- **AuthViewModels.cs** - ViewModels для форм входу та реєстрації

### 2. Services
- **IAuthService / AuthService** - сервіс авторизації
  - `LoginAsync()` - вхід користувача
  - `RegisterAsync()` - реєстрація нового користувача
  - `GetCurrentUserAsync()` - отримання поточного користувача з сесії
  - `LogoutAsync()` - вихід з системи
  - `HashPassword()` - хешування пароля (SHA256)
  - `VerifyPassword()` - перевірка пароля

### 3. Middleware
- **AuthenticationMiddleware** - перехоплює всі запити та перевіряє авторизацію
  - Публічні шляхи: `/account/*`, статичні файли
  - Захищені шляхи: всі інші

### 4. Controllers
- **AccountController** - управління обліковими записами
  - `GET /Account/Login` - форма входу
  - `POST /Account/Login` - обробка входу
  - `GET /Account/Register` - форма реєстрації
  - `POST /Account/Register` - обробка реєстрації
  - `GET /Account/Logout` - вихід

- **UserController** - управління профілем
  - `GET /User/Profile` - профіль користувача
  - `POST /User/UpdateProfile` - оновлення профілю
  - `GET /User/Index` - список користувачів
  - `GET /User/GetCurrentUser` - API для отримання даних

### 5. Views
- **Views/Account/Login.cshtml** - сторінка входу
- **Views/Account/Register.cshtml** - сторінка реєстрації
- **Views/User/Profile.cshtml** - сторінка профілю
- **Views/User/Index.cshtml** - список користувачів

## База даних

### Таблиця Users

| Поле | Тип | Опис |
|------|-----|------|
| Id | INTEGER | Primary Key, Auto Increment |
| UserName | TEXT(50) | Унікальне ім'я користувача |
| PasswordHash | TEXT(255) | Хеш паролю (SHA256) |
| FullName | TEXT(100) | Повне ім'я (nullable) |
| CreatedAt | TEXT | Дата створення |
| LastAccessAt | TEXT | Останній вхід |
| IsActive | INTEGER | Активний/неактивний (boolean) |
| Preferences | TEXT | JSON з налаштуваннями (nullable) |

**Індекси:**
- UNIQUE INDEX на `UserName`

## Потік авторизації

### Реєстрація
```
1. Користувач заповнює форму реєстрації
2. POST /Account/Register
3. Валідація даних (ModelState)
4. Перевірка унікальності UserName
5. Хешування пароля (SHA256)
6. Збереження в БД
7. Автоматичний вхід
8. Створення сесії (UserId в Session)
9. Redirect на Home/Index
```

### Вхід
```
1. Користувач вводить UserName та Password
2. POST /Account/Login
3. Пошук користувача в БД за UserName
4. Перевірка пароля (VerifyPassword)
5. Оновлення LastAccessAt
6. Створення сесії (UserId в Session)
7. Redirect на Home/Index
```

### Перевірка авторизації (Middleware)
```
1. Кожен запит → AuthenticationMiddleware
2. Перевірка чи публічний шлях? 
   - Так → пропустити
   - Ні → перевірити сесію
3. GetCurrentUserAsync(HttpContext)
4. Користувач знайдений?
   - Так → продовжити запит
   - Ні → Redirect на /Account/Login
```

## Сесії

Налаштування в Program.cs:
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);  // 24 години
    options.Cookie.HttpOnly = true;                // Захист від XSS
    options.Cookie.IsEssential = true;             // Необхідна cookie
});
```

Дані в сесії:
- `UserId` - ID поточного користувача
- `UserName` - ім'я користувача

## Безпека

### Хешування паролів
```csharp
public string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
```

### Валідація
- **UserName**: 3-50 символів
- **Password**: мінімум 6 символів
- **ConfirmPassword**: має співпадати з Password

### Захист від атак
- ✅ SQL Injection - Entity Framework параметризація
- ✅ XSS - HttpOnly cookies
- ✅ CSRF - ValidateAntiForgeryToken на формах

## Розширення

### Додавання ролей
```csharp
public class User
{
    // ...
    public string Role { get; set; } = "User"; // "Admin", "User", "Manager"
}
```

### Додавання Email
```csharp
public class User
{
    // ...
    [EmailAddress]
    public string? Email { get; set; }
}
```

### Двофакторна автентифікація
```csharp
public class User
{
    // ...
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
}
```

## Міграції

```bash
# Створення міграції
dotnet ef migrations add UpdateUserTableForAuthentication

# Застосування до БД
dotnet ef database update

# Відкат останньої міграції
dotnet ef migrations remove
```

## Тестування

### Створення тестового користувача
```csharp
var authService = serviceProvider.GetService<IAuthService>();
await authService.RegisterAsync("admin", "admin123", "Administrator");
```

### Перевірка входу
```csharp
var user = await authService.LoginAsync("admin", "admin123");
Assert.NotNull(user);
```

## Performance

- Використовується in-memory cache для сесій
- Індекс на UserName для швидкого пошуку
- SHA256 - баланс між безпекою та продуктивністю

## TODO / Покращення

- [ ] Email verification
- [ ] Password reset functionality
- [ ] Remember me functionality (persistent cookies)
- [ ] Account lockout after failed attempts
- [ ] Password strength meter
- [ ] Social login (Google, Facebook)
- [ ] Role-based authorization
- [ ] Activity log
