# Technical Details of Authentication System

## Architecture

```
User
  |
  v
AuthenticationMiddleware  <-- Checks session
  |
  v
AccountController  <-- Login/Register/Logout
  |
  v
AuthService  <-- Authentication business logic
  |
  v
BizFlowDbContext  <-- Database (SQLite)
```

## System Components

### 1. Models
- User.cs - user model
- AuthViewModels.cs - ViewModels for login and registration forms

### 2. Services
- IAuthService / AuthService - authentication service
  - LoginAsync() - user login
  - RegisterAsync() - register new user
  - GetCurrentUserAsync() - get current user from session
  - LogoutAsync() - logout
  - HashPassword() - password hashing (BCrypt)
  - VerifyPassword() - password verification

### 3. Middleware
- AuthenticationMiddleware - intercepts all requests and checks authorization
  - Public paths: /account/*, static files
  - Protected paths: all others

### 4. Controllers
- AccountController - account management
  - GET /Account/Login - login form
  - POST /Account/Login - login processing
  - GET /Account/Register - registration form
  - POST /Account/Register - registration processing
  - GET /Account/Logout - logout

- UserController - profile management
  - GET /User/Profile - user profile
  - POST /User/UpdateProfile - profile update
  - GET /User/Index - user list
  - GET /User/GetCurrentUser - API for data retrieval

### 5. Views
- Views/Account/Login.cshtml - login page
- Views/Account/Register.cshtml - registration page
- Views/User/Profile.cshtml - profile page
- Views/User/Index.cshtml - user list

## Database

### Users Table

Field | Type | Description
------|------|------------
Id | INTEGER | Primary Key, Auto Increment
UserName | TEXT(50) | Unique username
PasswordHash | TEXT(255) | Password hash (SHA256)
FullName | TEXT(100) | Full name (nullable)
CreatedAt | TEXT | Creation date
LastAccessAt | TEXT | Last login
IsActive | INTEGER | Active/inactive (boolean)
Preferences | TEXT | JSON with settings (nullable)

Indexes:
- UNIQUE INDEX on UserName

## Authorization Flow

### Registration
```
1. User fills registration form
2. POST /Account/Register
3. Data validation (ModelState)
4. Check UserName uniqueness
5. Password hashing (SHA256)
6. Save to DB
7. Automatic login
8. Session creation (UserId in Session)
9. Redirect на Home/Index
```

### Вхід
```
1. Користувач вводить UserName та Password
2. POST /Account/Login
3. Пошук користувача в БД за UserName
4. Перевірка пароля (VerifyPassword)
9. Redirect to Home/Index
```

### Authorization Check (Middleware)
```
1. Each request -> AuthenticationMiddleware
2. Check if public path?
   - Yes -> pass through
   - No -> check session
3. GetCurrentUserAsync(HttpContext)
4. User found?
   - Yes -> continue request
   - No -> Redirect to /Account/Login
```

## Sessions

Configuration in Program.cs:
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);  // 24 hours
    options.Cookie.HttpOnly = true;                // XSS protection
    options.Cookie.IsEssential = true;             // Essential cookie
});
```

Session data:
- UserId - current user ID
- UserName - username

## Security

### Password Hashing
```csharp
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

public bool VerifyPassword(string password, string hash)
{
    try
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    catch
    {
        return false;
    }
}
```

BCrypt advantages:
- Adaptive hashing (configurable work factor)
- Built-in salt generation
- Resistant to rainbow table attacks
- Industry standard for password storage

### Validation
- UserName: 3-50 characters
- Password: minimum 6 characters
- ConfirmPassword: must match Password

### Attack Protection
- SQL Injection - Entity Framework parameterization
- XSS - HttpOnly cookies
- CSRF - ValidateAntiForgeryToken on forms

## Extensions

### Adding Roles
```csharp
public class User
{
    // ...
    public string Role { get; set; } = "User"; // "Admin", "User", "Manager"
}
```

### Adding Email
```csharp
public class User
{
    // ...
    [EmailAddress]
    public string? Email { get; set; }
}
```

### Two-Factor Authentication
```csharp
public class User
{
    // ...
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
}
```

## Migrations

```bash
# Create migration
dotnet ef migrations add UpdateUserTableForAuthentication

# Apply to DB
dotnet ef database update

# Rollback last migration
dotnet ef migrations remove
```

## Testing

### Create test user
```csharp
var authService = serviceProvider.GetService<IAuthService>();
await authService.RegisterAsync("admin", "admin123", "Administrator");
```

### Login check
```csharp
var user = await authService.LoginAsync("admin", "admin123");
Assert.NotNull(user);
```

## Performance

- Uses in-memory cache for sessions
- Index on UserName for fast lookup
- SHA256 - balance between security and performance

## TODO / Improvements

- Email verification
- Password reset functionality
- Remember me functionality (persistent cookies)
- Account lockout after failed attempts
- Password strength meter
- Social login (Google, Facebook)
- Role-based authorization
- Activity log
