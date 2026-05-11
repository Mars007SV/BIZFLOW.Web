# 🎯 ШВИДКИЙ СТАРТ - BIZFLOW Desktop

## 📋 Для початківців (найпростіший спосіб)

### Як запустити додаток?

**Крок 1**: Подвійний клік на файл `START-BIZFLOW.bat`

**Крок 2**: Додаток автоматично відкриє браузер

**Крок 3**: Працюйте! 🎉

### Як створити EXE для іншого комп'ютера?

**Крок 1**: Подвійний клік на файл `PUBLISH-DESKTOP.bat`

**Крок 2**: Зачекайте ~1-2 хвилини

**Крок 3**: Скопіюйте папку `publish\BIZFLOW-Windows` на інший комп'ютер

**Крок 4**: На іншому комп'ютері запустіть `BIZFLOW.exe`

## 💻 Для розробників

### Режим розробки

```bash
dotnet run
```

Або:
```powershell
.\start-desktop.ps1
```

Або просто `F5` в Visual Studio

### Публікація Desktop версії

**PowerShell:**
```powershell
.\publish-desktop.ps1
```

**Або вручну:**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/BIZFLOW-Windows
```

**Командний рядок (CMD):**
```cmd
PUBLISH-DESKTOP.bat
```

## 📁 Файли проєкту

| Файл | Призначення |
|------|-------------|
| `START-BIZFLOW.bat` | Швидкий запуск для початківців |
| `PUBLISH-DESKTOP.bat` | Створення EXE (CMD) |
| `start-desktop.ps1` | Швидкий запуск (PowerShell) |
| `publish-desktop.ps1` | Створення EXE (PowerShell) |
| `DESKTOP-INSTRUCTIONS.md` | Детальні інструкції |
| `README-DESKTOP.md` | Опис Desktop версії |

## ❓ FAQ

**Q: Чи потрібен інтернет?**  
A: Ні! Додаток працює 100% офлайн

**Q: Де зберігаються дані?**  
A: В файлі `bizflow.db` в папці з додатком

**Q: Як зробити backup?**  
A: Просто скопіюйте файл `bizflow.db`

**Q: Скільки важить EXE?**  
A: ~70-90 MB (включає .NET Runtime)

**Q: Чи можна запустити на декількох комп'ютерах?**  
A: Так! Просто скопіюйте папку `BIZFLOW-Windows`

**Q: Чи потрібно встановлювати .NET?**  
A: Ні! Все вже включено в EXE

## 🎨 Що отримуєте?

✅ Один EXE файл - працює на будь-якому Windows  
✅ Автоматичне відкриття браузера  
✅ Локальна база даних SQLite  
✅ Сучасний веб-інтерфейс  
✅ Не потребує інтернету  
✅ Безпечне зберігання даних  

## 🚀 Готово до використання!

Просто запустіть `START-BIZFLOW.bat` і починайте працювати! 🎉

---

Питання? Дивіться детальні інструкції: [DESKTOP-INSTRUCTIONS.md](DESKTOP-INSTRUCTIONS.md)
