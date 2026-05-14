# 🖥️ BIZFLOW Desktop - Electron.NET Build Guide

## 📦 What You'll Get

**BIZFLOW** as a cross-platform desktop application:
- ✅ **Windows** (x64, x86, ARM64)
- ✅ **macOS** (Intel & Apple Silicon)
- ✅ **Linux** (AppImage, deb, rpm)
- ✅ **Offline** - No internet required
- ✅ **Single executable** or installer package

---

## 🛠️ Prerequisites

### 1. Install .NET SDK 10
Already installed ✅

### 2. Install Node.js
Download from: https://nodejs.org/ (LTS version recommended)

Verify installation:
```powershell
node --version
npm --version
```

### 3. Install Electron.NET CLI
```powershell
dotnet tool install ElectronNET.CLI -g
```

If already installed, update:
```powershell
dotnet tool update ElectronNET.CLI -g
```

---

## 🚀 Development Mode

### Run in Development (with DevTools)

```powershell
cd "D:\Diproject\BIZFLOW.Web"

# Start Electron.NET dev server
electronize start
```

This will:
- Build the .NET application
- Download Electron binaries (first time only ~150MB)
- Open desktop window with your app
- Enable hot reload
- Open DevTools (F12)

### Debug in Visual Studio

1. Set launch profile to **Electron.NET**
2. Press **F5** to debug

---

## 📦 Build for Production

### Windows x64 (Recommended)

```powershell
cd "D:\Diproject\BIZFLOW.Web"

# Build portable executable
electronize build /target win /electron-arch x64

# Output: bin\Desktop\BIZFLOW Setup 1.0.0.exe
```

### Windows Installer (NSIS)

```powershell
electronize build /target win /electron-arch x64 /package-json electron.manifest.json
```

Creates:
- `BIZFLOW Setup 1.0.0.exe` - Installer (~150MB)
- `BIZFLOW-1.0.0-win-unpacked` - Portable folder

### Other Platforms

```powershell
# Windows 32-bit
electronize build /target win /electron-arch ia32

# Windows ARM64
electronize build /target win /electron-arch arm64

# macOS (Intel)
electronize build /target osx /electron-arch x64

# macOS (Apple Silicon)
electronize build /target osx /electron-arch arm64

# Linux (AppImage)
electronize build /target linux /electron-arch x64
```

---

## 📁 Output Structure

After build:

```
BIZFLOW.Web/
├── bin/
│   └── Desktop/
│       ├── BIZFLOW Setup 1.0.0.exe        ← Installer (Windows)
│       └── win-unpacked/
│           ├── BIZFLOW.exe                ← Portable executable
│           ├── resources/
│           │   └── app/                   ← Your .NET app
│           └── ...                        ← Electron runtime
```

---

## 🎨 Customization

### 1. Application Icon

Replace default icon:
```
wwwroot/favicon.ico
```

Or add custom icon in `electron.manifest.json`:
```json
{
  "build": {
	"win": {
	  "icon": "path/to/icon.ico"
	}
  }
}
```

### 2. Window Settings

Edit `Program.cs` - `ConfigureElectronWindow()`:

```csharp
var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
{
	Width = 1400,              // Window width
	Height = 900,              // Window height
	Title = "BIZFLOW",         // Window title
	Fullscreen = false,        // Start fullscreen?
	Resizable = true,          // Allow resize?
	AutoHideMenuBar = true,    // Hide menu bar?
	MinWidth = 1024,           // Minimum width
	MinHeight = 768            // Minimum height
});
```

### 3. Splash Screen

Add loading screen in `electron.manifest.json`:
```json
{
  "splashscreen": {
	"imageFile": "wwwroot/splash.png"
  }
}
```

### 4. Single Instance

Prevent multiple app instances in `electron.manifest.json`:
```json
{
  "singleInstance": true
}
```

---

## 📝 Installer Customization

### NSIS Installer (Windows)

Edit `electron.manifest.json`:

```json
{
  "build": {
	"appId": "com.bizflow.desktop",
	"productName": "BIZFLOW",
	"copyright": "Copyright © 2025 Your Company",
	"win": {
	  "target": [
		{
		  "target": "nsis",
		  "arch": ["x64"]
		}
	  ],
	  "icon": "wwwroot/icon.ico",
	  "publisherName": "Your Company Name",
	  "verifyUpdateCodeSignature": false
	},
	"nsis": {
	  "oneClick": false,
	  "allowToChangeInstallationDirectory": true,
	  "createDesktopShortcut": true,
	  "createStartMenuShortcut": true,
	  "shortcutName": "BIZFLOW"
	}
  }
}
```

---

## 🔄 Auto-Update Feature

### Enable Auto-Updates

1. Install package:
```powershell
dotnet add package ElectronNET.API
```

2. Add update check in `Program.cs`:
```csharp
if (HybridSupport.IsElectronActive)
{
	// Check for updates
	Electron.AutoUpdater.OnUpdateAvailable += (info) =>
	{
		// Show notification to user
	};

	Electron.AutoUpdater.CheckForUpdates();
}
```

3. Host releases on GitHub Releases

---

## 📊 Build Sizes

Expected output sizes:

| Platform | Installer | Unpacked |
|----------|-----------|----------|
| Windows x64 | ~150MB | ~220MB |
| Windows x86 | ~140MB | ~210MB |
| macOS Intel | ~170MB | ~250MB |
| macOS ARM64 | ~160MB | ~240MB |
| Linux AppImage | ~160MB | ~230MB |

---

## 🐛 Troubleshooting

### Problem: `electronize` command not found

**Solution:**
```powershell
dotnet tool install ElectronNET.CLI -g
```

Add to PATH:
```
C:\Users\[YourUser]\.dotnet\tools
```

### Problem: Build fails with Node.js error

**Solution:**
```powershell
# Install Node.js LTS from https://nodejs.org/
# Restart terminal
node --version
```

### Problem: "Port already in use"

**Solution:**
```powershell
# Kill process using port 8080 (Electron default)
netstat -ano | findstr :8080
taskkill /PID [PID_NUMBER] /F
```

### Problem: Blank window on startup

**Solution:**
- Check console logs
- Verify database path exists
- Test in regular ASP.NET mode first

---

## 🚀 CI/CD Build Script

Create `.github/workflows/electron-build.yml`:

```yaml
name: Electron Build

on:
  push:
	tags:
	  - 'v*'

jobs:
  build-windows:
	runs-on: windows-latest

	steps:
	- uses: actions/checkout@v3

	- name: Setup .NET
	  uses: actions/setup-dotnet@v3
	  with:
		dotnet-version: '10.0.x'

	- name: Setup Node.js
	  uses: actions/setup-node@v3
	  with:
		node-version: '18'

	- name: Install Electron.NET CLI
	  run: dotnet tool install ElectronNET.CLI -g

	- name: Build Electron App
	  run: electronize build /target win /electron-arch x64

	- name: Upload Artifact
	  uses: actions/upload-artifact@v3
	  with:
		name: BIZFLOW-Windows-x64
		path: bin/Desktop/*.exe

	- name: Create Release
	  uses: softprops/action-gh-release@v1
	  with:
		files: bin/Desktop/*.exe
	  env:
		GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

---

## 📚 Additional Resources

- **Electron.NET Docs:** https://github.com/ElectronNET/Electron.NET
- **Electron Docs:** https://www.electronjs.org/docs
- **Builder Options:** https://www.electron.build/configuration/configuration

---

## ✅ Quick Checklist Before Release

- [ ] Test application in development mode
- [ ] Test database migrations
- [ ] Replace default icon
- [ ] Update version in `electron.manifest.json`
- [ ] Test installer on clean Windows machine
- [ ] Test offline mode (disconnect internet)
- [ ] Create user documentation
- [ ] Build for target platforms
- [ ] Test each build package

---

**Happy Building! 🎉**
