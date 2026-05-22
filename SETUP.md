# BIZFLOW - Інструкція з встановлення та розгортання

## Зміст
- Швидкий старт для користувачів
- Встановлення
- Налаштування для розробки
- Збірка для продакшн
- Розгортання

---

## Швидкий старт для користувачів

### ВАЖЛИВО: Створення збірки
**Папка `BIZFLOW-Windows` НЕ включена в Git репозиторій!**

Перед запуском програми потрібно створити збірку:
```bash
PUBLISH-DESKTOP.bat
```

Це створить папку `publish\BIZFLOW-Windows\` з готовим додатком.

### Запуск програми
1. Відкрийте папку `publish\BIZFLOW-Windows\`
2. Запустіть `BIZFLOW.exe`
3. Браузер відкриється автоматично

**Нічого встановлювати не потрібно!** Всі необхідні компоненти (.NET, база даних) вже включені в EXE файл.

### Перший запуск
1. Натисніть "Зареєструватися"
2. Введіть ім'я користувача (3-50 символів)
3. Створіть пароль (мінімум 6 символів)
4. Готово!

### Зберігання даних
База даних автоматично створюється:
```
C:\Users\[ВашеІм'я]\AppData\Local\BIZFLOW\bizflow.db
```

---

## Встановлення

### Для розробників

```bash
# 1. Клонуйте репозиторій
git clone https://github.com/Mars007SV/BIZFLOW.Web.git
cd BIZFLOW.Web

# 2. Створіть збірку
PUBLISH-DESKTOP.bat

# 3. Готовий додаток буде у:
# publish\BIZFLOW-Windows\BIZFLOW.exe
```

### Для кінцевих користувачів

**Системні вимоги:**
- Windows 10/11 (64-bit)
- Нічого додатково встановлювати не потрібно

**Інструкція:**
1. Отримайте папку `BIZFLOW-Windows` від розробника
2. Скопіюйте її в будь-яке місце на комп'ютері
3. Запустіть `BIZFLOW.exe`
4. Браузер відкриється автоматично

---

## Налаштування для розробки

### Необхідні компоненти
- .NET 10 SDK
- Visual Studio 2026 або VS Code
- Git

### 1. Клонування репозиторію
```bash
git clone https://github.com/Mars007SV/BIZFLOW.Web.git
cd BIZFLOW.Web
```

### 2. Відновлення залежностей
```bash
dotnet restore
```

### 3. Запуск сервера розробки

#### Варіант A: Простий запуск (Windows)
```bash
START-BIZFLOW.bat
```
- Запускає сервер на http://localhost:5555
- Відкриває браузер автоматично
- Натисніть будь-яку клавішу для зупинки

#### Варіант B: З Electron Desktop (розробка)
```bash
START-ELECTRON.bat
```
- Запускає Electron desktop додаток
- Перший запуск: Завантажує Electron (близько 150МБ, 2-5 хвилин)
- Наступні запуски: 20-30 секунд

#### Варіант C: Вручну
```bash
dotnet run --launch-profile BIZFLOW
```

### 4. Налаштування за замовчуванням
- URL для розробки: http://localhost:5555
- URL для продакшн: http://localhost:5000
- База даних: SQLite (автоматично створюється при першому запуску)

---

## Збірка для продакшн

### Десктопний додаток Windows

#### Повний скрипт збірки (рекомендується)
```bash
PUBLISH-DESKTOP.bat
```

Це створює:
- Самодостатній виконуваний файл
- Розгортання одного файлу
- Усі залежності включені
- Вихід: publish\BIZFLOW-Windows\BIZFLOW.exe

#### Збірка вручну
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/BIZFLOW-Windows
```

### Пояснення параметрів збірки
- -c Release - Оптимізована продакшн збірка
- -r win-x64 - Цільова платформа Windows 64-біт
- --self-contained true - Включає .NET runtime
- -p:PublishSingleFile=true - Один EXE файл
- -o publish/BIZFLOW-Windows - Папка для виходу

### Розмір файлів
- Зібраний додаток: 80-100 МБ
- Включає .NET 10 runtime та всі залежності

---

## Розгортання

### Локальне розгортання (Один ПК)

#### Розгортання зібраної версії
1. Запустіть PUBLISH-DESKTOP.bat
2. Скопіюйте всю папку publish\BIZFLOW-Windows
3. Помістіть на цільовий ПК
4. Запустіть BIZFLOW.exe
5. Браузер відкривається автоматично

#### Що розгортається
```
BIZFLOW-Windows/
├── BIZFLOW.exe          # Main executable
├── wwwroot/             # Static files (CSS, JS, images)
├── appsettings.json     # Configuration
└── [runtime files]      # .NET dependencies
```

### Multi-User Deployment (Shared PC)

Each Windows user automatically gets:
- Separate login credentials
- Own database in their AppData folder
- Independent inventory data

No additional setup required.

### Network Deployment (Multiple PCs)

#### Option 1: Individual Installations
- Deploy to each PC separately
- Each PC has independent database
- Users maintain their own data

#### Option 2: Shared Database (Advanced)
For shared database across network:
1. Set up SQL Server or PostgreSQL
2. Update appsettings.json connection string
3. Run migrations on shared database
4. Deploy application to all PCs

Note: Default SQLite setup doesn't support network sharing.

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=bizflow.db"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information"
	}
  }
}
```

### Environment Variables
- ASPNETCORE_ENVIRONMENT - Set to Production or Development
- ASPNETCORE_URLS - Override listening ports

### Launch Profiles (Properties/launchSettings.json)
- http - Development (port 5129)
- https - Development with SSL (port 7293)
- BIZFLOW - Custom profile (port 5555, auto-launch browser)

---

## Testing Deployment

### Test Published Build Locally
```bash
cd publish\BIZFLOW-Windows
BIZFLOW.exe
```

### Verify
1. Application starts
2. Browser opens automatically
3. Login page appears
4. Can create user account
5. Dashboard loads after login

---

## Database Management

### Location
```
C:\Users\[Username]\AppData\Local\BIZFLOW\bizflow.db
```

### Backup
Copy the entire BIZFLOW folder:
```bash
xcopy /E /I "C:\Users\%USERNAME%\AppData\Local\BIZFLOW" "D:\Backups\BIZFLOW"
```

### Restore
Copy the backed-up folder back:
```bash
xcopy /E /I "D:\Backups\BIZFLOW" "C:\Users\%USERNAME%\AppData\Local\BIZFLOW"
```

### Reset (Fresh Start)
Delete the BIZFLOW folder - new database will be created on next launch:
```bash
rmdir /S /Q "C:\Users\%USERNAME%\AppData\Local\BIZFLOW"
```

---

## Troubleshooting

### Application Won't Start
1. Check if port 5000 is available:
   ```bash
   netstat -ano | findstr :5000
   ```
2. Try running as Administrator
3. Check Windows Firewall settings

### Browser Doesn't Open
- Application still starts successfully
- Manually open: http://localhost:5000
- Check console output for actual port

### Database Errors
1. Check database path exists
2. Verify write permissions to AppData folder
3. Delete database file to recreate

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## Support and Documentation

### Project Links
- Repository: https://github.com/Mars007SV/BIZFLOW.Web
- Issues: https://github.com/Mars007SV/BIZFLOW.Web/issues

### Additional Documentation
- User authentication system: Built-in login/signup
- Multi-user support: Automatic per-Windows-user databases
- Technology stack: ASP.NET Core 10, Razor Pages, SQLite

---

## Git Workflow

### Branches
- main - Stable production releases
- develop - Integration branch for features
- feature/* - Feature development branches

### For Developers
```bash
# Switch to develop
git checkout develop

# Create feature branch
git checkout -b feature/your-feature-name

# After development
git add .
git commit -m "feat: your feature description"
git checkout develop
git merge feature/your-feature-name
```

---

## Features

- Inventory management (add, edit, delete products)
- Category organization
- Operations history with full audit trail
- User authentication and authorization
- Multi-user support (separate databases per Windows user)
- Offline-first design
- Auto-open browser on launch
- Cross-platform ready (Windows, macOS, Linux)

---

Last Updated: 2025
Version: 1.0
License: MIT
