# Скрипт для створення Desktop версії BIZFLOW для Windows

Write-Host "🚀 Публікація BIZFLOW Desktop для Windows..." -ForegroundColor Green

# Очищення старих публікацій
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

# Публікація додатку
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/BIZFLOW-Windows

Write-Host "✅ Готово! Виконуваний файл знаходиться в папці: publish/BIZFLOW-Windows/BIZFLOW.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Для розповсюдження скопіюйте папку publish/BIZFLOW-Windows на інший комп'ютер" -ForegroundColor Yellow
Write-Host "   Користувач зможе просто запустити BIZFLOW.exe" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 База даних SQLite автоматично створюється при першому запуску" -ForegroundColor Magenta

# Відкрити папку з результатом
Start-Process "publish/BIZFLOW-Windows"
