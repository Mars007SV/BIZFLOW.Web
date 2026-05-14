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

REM Запускаємо сервер у фоновому режимі та відкриваємо браузер
start /B dotnet run --launch-profile BIZFLOW

REM Чекаємо 5 секунд поки сервер запуститься
timeout /t 5 /nobreak >nul

REM Відкриваємо браузер
start http://localhost:5555

REM Чекаємо завершення процесу dotnet
echo.
echo Натисніть будь-яку клавішу для завершення...
pause >nul

REM Завершуємо всі процеси dotnet
taskkill /IM dotnet.exe /F >nul 2>&1

echo.
echo ✅ BIZFLOW закрито
echo.
