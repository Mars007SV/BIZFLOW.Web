# ⚡ BIZFLOW - Quick Start

## 🎯 Що зроблено?

✅ **Electron.NET інтегровано!** Ваш додаток тепер працює як:
- 🖥️ Desktop додаток (Windows, macOS, Linux)
- 📴 Повністю офлайн
- 🚀 Швидкий запуск
- 📦 З інсталятором

✅ **Git гілки налаштовано:**
```
main              - Стабільна версія (production)
develop           - Інтеграція нових функцій
feature/electron-desktop - Поточна розробка
```

---

## 🚀 Швидкий запуск (для розробки)

### Варіант 1: Подвійний клік (Windows)
```
START-ELECTRON.bat
```

### Варіант 2: PowerShell
```powershell
.\START-ELECTRON.ps1
```

### Варіант 3: Командний рядок
```bash
electronize start
```

**Перший запуск:** Завантажить Electron (~150MB) - може зайняти 2-5 хвилин  
**Наступні запуски:** 20-30 секунд

---

## 📦 Збірка для розповсюдження

### Windows (64-bit інсталятор)
```bash
electronize build /target win /electron-arch x64
```
**Результат:** `bin\Desktop\BIZFLOW Setup 1.0.0.exe` (~150MB)

### Всі платформи
```bash
# Windows 32-bit
electronize build /target win /electron-arch ia32

# macOS Intel
electronize build /target osx /electron-arch x64

# macOS Apple Silicon
electronize build /target osx /electron-arch arm64

# Linux AppImage
electronize build /target linux /electron-arch x64
```

---

## 🌳 Робота з Git

### Поточна гілка: `feature/electron-desktop`

```bash
# Перегляд всіх гілок
git branch -a

# Перемикання між гілками
git checkout main      # Стабільна версія
git checkout develop   # Версія в розробці
git checkout feature/electron-desktop  # Electron функція

# Коміт змін
git add .
git commit -m "feat: Your feature description"
git push origin feature/electron-desktop
```

### Merge feature → develop
```bash
git checkout develop
git merge feature/electron-desktop
git push origin develop
```

### Release develop → main
```bash
git checkout main
git merge develop
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin main --tags
```

📖 **Детально:** [GIT_WORKFLOW.md](GIT_WORKFLOW.md)

---

## 📁 Файли проекту

### Нові файли (Electron):
- ✅ `electron.manifest.json` - Конфігурація Electron
- ✅ `START-ELECTRON.bat` - Швидкий запуск (Windows)
- ✅ `START-ELECTRON.ps1` - Швидкий запуск (PowerShell)

### Змінені файли:
- ✅ `Program.cs` - Додано підтримку Electron.NET
- ✅ `BIZFLOW.Web.csproj` - Додано пакет ElectronNET.API
- ✅ `README.md` - Оновлена документація

### Документація:
- 📖 `ELECTRON_BUILD_GUIDE.md` - Повна інструкція зі збірки
- 📖 `GIT_WORKFLOW.md` - Робота з Git гілками
- 📖 `USER_GUIDE_UA.md` - Інструкція для користувачів

---

## 🔧 Налаштування

### Вікно додатку
Редагуйте `Program.cs` → `ConfigureElectronWindow()`:
```csharp
Width = 1400,          // Ширина вікна
Height = 900,          // Висота
Title = "BIZFLOW",     // Назва
Fullscreen = false,    // Повний екран?
Resizable = true       // Зміна розміру?
```

### Іконка додатку
Замініть файл:
```
wwwroot/favicon.ico
```

### Версія і назва
Редагуйте `electron.manifest.json`:
```json
{
  "build": {
	"appId": "com.bizflow.desktop",
	"productName": "BIZFLOW",
	"buildVersion": "1.0.0"
  }
}
```

---

## ❓ Проблеми?

### `electronize` не знайдено
```bash
dotnet tool install ElectronNET.CLI -g
```

### Порт зайнятий
```bash
# Windows
netstat -ano | findstr :8080
taskkill /PID [PID] /F
```

### Пусте вікно
1. Перевірте console logs
2. Спробуйте спочатку `dotnet run`
3. Видаліть `bin\` і `obj\` папки

---

## 📚 Корисні посилання

- **Electron.NET Docs:** https://github.com/ElectronNET/Electron.NET
- **Electron Docs:** https://www.electronjs.org/docs
- **ASP.NET Core Docs:** https://docs.microsoft.com/aspnet/core

---

## ✅ Наступні кроки

1. **Запустіть у режимі розробки:**
   ```bash
   START-ELECTRON.bat
   ```

2. **Протестуйте функціонал:**
   - Вхід/реєстрація
   - Додавання товарів
   - Генерація звітів
   - Експорт в Excel

3. **Зберіть для тестування:**
   ```bash
   electronize build /target win /electron-arch x64
   ```

4. **Протестуйте installer на чистій машині**

5. **Створіть Pull Request:**
   ```bash
   # На GitHub: feature/electron-desktop → develop
   ```

---

**Успіхів! 🎉**

Якщо потрібна допомога - пишіть у Issues на GitHub!
