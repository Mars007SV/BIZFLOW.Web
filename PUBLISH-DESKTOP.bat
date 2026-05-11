@echo off
chcp 65001 >nul
echo.
echo ═══════════════════════════════════════════════════════════
echo         📦 BIZFLOW - Створення Desktop версії
echo ═══════════════════════════════════════════════════════════
echo.
echo Створення EXE файлу для розповсюдження...
echo Це може зайняти кілька хвилин...
echo.

if exist publish (
    echo Видалення старої версії...
    rmdir /s /q publish
)

echo Публікація додатку...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/BIZFLOW-Windows

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ═══════════════════════════════════════════════════════════
    echo ✅ Готово! Файл створено успішно!
    echo ═══════════════════════════════════════════════════════════
    echo.
    echo 📂 Виконуваний файл: publish\BIZFLOW-Windows\BIZFLOW.exe
    echo.
    echo 📋 Що далі:
    echo    1. Скопіюйте папку publish\BIZFLOW-Windows
    echo    2. Користувач запускає BIZFLOW.exe
    echo    3. Браузер відкриється автоматично
    echo.
    echo 💡 База даних SQLite створюється автоматично
    echo.
    echo Відкриваю папку з результатом...
    start "" "publish\BIZFLOW-Windows"
) else (
    echo.
    echo ❌ Помилка при створенні Desktop версії
    echo.
)

echo.
pause
