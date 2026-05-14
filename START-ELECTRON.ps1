# BIZFLOW Desktop - Quick Start PowerShell Script
# For Windows PowerShell

Write-Host ""
Write-Host "╔═══════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      BIZFLOW Desktop - Quick Start       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if electronize is installed
$electronizeInstalled = Get-Command electronize -ErrorAction SilentlyContinue

if (-not $electronizeInstalled) {
	Write-Host "⚠️  Electron.NET CLI not found!" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "Installing ElectronNET.CLI globally..." -ForegroundColor Yellow
	dotnet tool install ElectronNET.CLI -g
	Write-Host ""
	Write-Host "✅ Installation complete!" -ForegroundColor Green
	Write-Host ""
}

Write-Host "🚀 Starting Electron.NET development mode..." -ForegroundColor Green
Write-Host ""
Write-Host "This will:" -ForegroundColor Gray
Write-Host "  • Build the .NET application" -ForegroundColor Gray
Write-Host "  • Download Electron binaries (first time only)" -ForegroundColor Gray
Write-Host "  • Launch desktop window" -ForegroundColor Gray
Write-Host "  • Enable hot reload" -ForegroundColor Gray
Write-Host ""

electronize start
