@echo off
chcp 65001 >nul
echo.
echo ═══════════════════════════════════════════════════════════
echo            🚀 BIZFLOW - Запуск Desktop версії
echo ═══════════════════════════════════════════════════════════
echo.
echo Запуск додатку...
echo Браузер відкриється автоматично на http://localhost:5555
echo.
echo Для завершення - закрийте це вікно або натисніть Ctrl+C
echo.
echo ═══════════════════════════════════════════════════════════
echo.

dotnet run --launch-profile BIZFLOW

echo.
echo ✅ BIZFLOW закрито
echo.
pause
