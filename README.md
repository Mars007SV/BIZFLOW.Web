# BIZFLOW - Система управління товарними запасами

## 🎯 Про проєкт

BIZFLOW - це професійна система для управління товарними запасами, яка дозволяє:
- ✅ Відстежувати залишки товарів
- ✅ Організовувати товари за категоріями
- ✅ Вести історію всіх операцій з детальним аудитом
- ✅ Працювати локально на вашому комп'ютері
- 🔐 **Захищений вхід з авторизацією користувачів**
- 👥 **Кожен користувач Windows має свою окрему базу даних**

## 👥 Багатокористувацька система

**Кожен користувач Windows має свої дані!**

```
🖥️ Один комп'ютер
   ├── 👤 Користувач 1 → Своя база даних
   ├── 👤 Користувач 2 → Своя база даних
   └── 👤 Користувач 3 → Своя база даних
```

**База даних зберігається:**
```
C:\Users\[ВашеІм'я]\AppData\Local\BIZFLOW\bizflow.db
```

Детальніше: [MultiUserSystem.md](Docs/MultiUserSystem.md)

## 🔐 Авторизація

При першому запуску вам потрібно **створити обліковий запис**:
1. Натисніть "Зареєструватися"
2. Введіть ім'я користувача (3-50 символів)
3. Створіть пароль (мінімум 6 символів)
4. Готово! Можете користуватися системою

Детальніше: [AuthenticationGuide.md](Docs/AuthenticationGuide.md)

## 🖥️ Desktop версія (Electron.NET)

BIZFLOW працює як **кросплатформний desktop додаток** на базі Electron.NET!

### ✨ Переваги:
- 🌍 **Windows, macOS, Linux** - одна кодова база
- 📴 **Повністю офлайн** - не потрібен інтернет
- 💾 **Локальна база даних** - всі дані на вашому ПК
- 🚀 **Швидкий запуск** - як звичайний додаток
- 📦 **Інсталятор** - легка установка для користувачів

### Швидкий старт:

```powershell
# Режим розробки (з DevTools)
electronize start

# Збірка для Windows (x64)
electronize build /target win /electron-arch x64

# Збірка для macOS
electronize build /target osx /electron-arch x64

# Збірка для Linux
electronize build /target linux /electron-arch x64
```

### 📚 Детальні інструкції:
- **[Electron Build Guide](ELECTRON_BUILD_GUIDE.md)** - Повний посібник зі збірки
- **[User Guide (UA)](USER_GUIDE_UA.md)** - Інструкція для кінцевих користувачів
- **[Git Workflow](GIT_WORKFLOW.md)** - Робота з гілками Git

## 🚀 Технології

- **Backend**: ASP.NET Core 10 (Razor Pages/MVC)
- **Desktop**: Electron.NET 23.6.2 (Chromium + Node.js)
- **Database**: SQLite with Entity Framework Core 10
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Reports**: ClosedXML (Excel generation)
- **Security**: SHA256 password hashing, Session-based auth
- **Deployment**: Self-contained executables (Windows/macOS/Linux)

## 📋 Функціональність

### 🔐 Система авторизації
- Реєстрація та вхід користувачів
- Безпечне зберігання паролів (SHA256)
- Профіль користувача з можливістю редагування
- Список всіх користувачів системи

### Управління товарами
- Додавання, редагування, видалення товарів
- Відстеження кількості на складі
- Прив'язка до категорій

### Категорії
- Організація товарів за категоріями
- Гнучка структура каталогу

### Історія операцій
- Автоматичний аудит всіх змін
- Детальна інформація про кожну операцію
- Зберігання старих та нових значень

## 🛠️ Системні вимоги

- Windows 10/11 (x64)
- 512 MB RAM (мінімум)
- 150 MB вільного місця на диску

## 📦 Структура проєкту

```
BIZFLOW.Web/
├── Controllers/         # MVC контролери
├── Models/             # Моделі даних
├── Views/              # Razor views
├── Data/               # DbContext
├── Migrations/         # EF Core міграції
├── wwwroot/            # Статичні файли
├── publish-desktop.ps1 # Скрипт публікації Desktop версії
└── start-desktop.ps1   # Швидкий запуск
```

## 🎨 Особливості

- 🎯 Сучасний UI з Bootstrap 5
- 🔄 Автоматичне відкриття браузера при запуску
- 💾 SQLite база даних (не потребує сервера БД)
- 📱 Responsive дизайн
- 🔐 Локальне зберігання даних (безпека)

## 📚 Документація

### Для розробників:
- **[Git Workflow Guide](GIT_WORKFLOW.md)** - Стратегія роботи з гілками та коміти
- **[Electron Build Guide](ELECTRON_BUILD_GUIDE.md)** - Інструкції зі збірки desktop додатку
- **[Multi-User System](Docs/MultiUserSystem.md)** - Система ізоляції баз даних
- **[Authentication Guide](Docs/AuthenticationGuide.md)** - Система автентифікації

### Для користувачів:
- **[User Guide (UA)](USER_GUIDE_UA.md)** - Інструкція користувача українською

## 🤝 Підтримка

Якщо у вас виникли питання або проблеми, створіть Issue на GitHub.

## 📄 Ліцензія

Цей проєкт розроблено для навчальних та комерційних цілей.

---

Made with ❤️ for efficient inventory management
