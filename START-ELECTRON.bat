@echo off
chcp 65001 >nul
echo.
echo ╔═══════════════════════════════════════════╗
echo ║      BIZFLOW Desktop - Quick Start       ║
echo ╚═══════════════════════════════════════════╝
echo.
echo 🚀 Starting Electron.NET development mode...
echo.

REM Check if electronize is installed
where electronize >nul 2>nul
if %errorlevel% neq 0 (
	echo ⚠️  Electron.NET CLI not found!
	echo.
	echo Installing ElectronNET.CLI globally...
	dotnet tool install ElectronNET.CLI -g
	echo.
	echo ✅ Installation complete!
	echo.
)

echo 📦 Building and launching desktop app...
echo.

electronize start

pause
