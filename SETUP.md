# 🚀 BIZFLOW - Installation & Deployment Guide

## 📋 Table of Contents
- [Quick Start for Users](#quick-start-for-users)
- [Installation](#installation)
- [Development Setup](#development-setup)
- [Building for Production](#building-for-production)
- [Deployment](#deployment)

---

## 🎯 Quick Start for Users

### Running Installed Application
After installation, simply run `BIZFLOW.exe` - the browser will open automatically at `http://localhost:5555`

### First Time Setup
1. Launch the application
2. Click **"Sign Up"** on the login page
3. Enter username (3-50 characters)
4. Create password (minimum 6 characters)
5. Done! Start managing your inventory

### Data Storage
Your database is stored at:
```
C:\Users\[YourName]\AppData\Local\BIZFLOW\bizflow.db
```

Each Windows user has their own separate database.

---

## 📦 Installation

### For End Users (Windows)

#### Option 1: Run from Published Build
1. Download the `BIZFLOW-Windows` folder
2. Run `BIZFLOW.exe`
3. Browser opens automatically

#### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/Mars007SV/BIZFLOW.Web.git
cd BIZFLOW.Web

# Run the publish script
PUBLISH-DESKTOP.bat

# Find your executable at:
# publish\BIZFLOW-Windows\BIZFLOW.exe
```

---

## 🛠️ Development Setup

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026 (or VS Code)
- Git

### 1. Clone Repository
```bash
git clone https://github.com/Mars007SV/BIZFLOW.Web.git
cd BIZFLOW.Web
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Run Development Server

#### Option A: Simple Run (Windows)
```bash
START-BIZFLOW.bat
```
- Starts server on `http://localhost:5555`
- Opens browser automatically
- Press any key to stop

#### Option B: With Electron Desktop (Development)
```bash
START-ELECTRON.bat
```
- Starts Electron desktop app
- First run: Downloads Electron (~150MB, 2-5 minutes)
- Next runs: 20-30 seconds

#### Option C: Manual
```bash
dotnet run --launch-profile BIZFLOW
```

### 4. Default Configuration
- **Development URL:** `http://localhost:5555`
- **Production URL:** `http://localhost:5000`
- **Database:** SQLite (auto-created on first run)

---

## 📦 Building for Production

### Windows Desktop Application

#### Full Build Script (Recommended)
```bash
PUBLISH-DESKTOP.bat
```

This creates:
- Self-contained executable
- Single file deployment
- All dependencies included
- Output: `publish\BIZFLOW-Windows\BIZFLOW.exe`

#### Manual Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/BIZFLOW-Windows
```

### Build Options Explained
- `-c Release` - Optimized production build
- `-r win-x64` - Windows 64-bit target
- `--self-contained true` - Includes .NET runtime
- `-p:PublishSingleFile=true` - Single EXE file
- `-o publish/BIZFLOW-Windows` - Output directory

### File Size
- Built application: ~80-100 MB
- Includes .NET 10 runtime and all dependencies

---

## 🌐 Deployment

### Local Deployment (Single PC)

#### Deploy Published Build
1. Run `PUBLISH-DESKTOP.bat`
2. Copy entire `publish\BIZFLOW-Windows` folder
3. Place on target PC
4. Run `BIZFLOW.exe`
5. Browser opens automatically

#### What Gets Deployed
```
BIZFLOW-Windows/
├── BIZFLOW.exe          # Main executable
├── wwwroot/             # Static files (CSS, JS, images)
├── appsettings.json     # Configuration
└── [runtime files]      # .NET dependencies
```

### Multi-User Deployment (Shared PC)

Each Windows user automatically gets:
- Separate login credentials
- Own database in their AppData folder
- Independent inventory data

**No additional setup required!**

### Network Deployment (Multiple PCs)

#### Option 1: Individual Installations
- Deploy to each PC separately
- Each PC has independent database
- Users maintain their own data

#### Option 2: Shared Database (Advanced)
For shared database across network:
1. Set up SQL Server or PostgreSQL
2. Update `appsettings.json` connection string
3. Run migrations on shared database
4. Deploy application to all PCs

**Note:** Default SQLite setup doesn't support network sharing.

---

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=bizflow.db"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information"
	}
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Set to `Production` or `Development`
- `ASPNETCORE_URLS` - Override listening ports

### Launch Profiles (Properties/launchSettings.json)
- `http` - Development (port 5129)
- `https` - Development with SSL (port 7293)
- `BIZFLOW` - Custom profile (port 5555, auto-launch browser)

---

## 🧪 Testing Deployment

### Test Published Build Locally
```bash
cd publish\BIZFLOW-Windows
BIZFLOW.exe
```

### Verify
1. ✅ Application starts
2. ✅ Browser opens automatically
3. ✅ Login page appears
4. ✅ Can create user account
5. ✅ Dashboard loads after login

---

## 📊 Database Management

### Location
```
C:\Users\[Username]\AppData\Local\BIZFLOW\bizflow.db
```

### Backup
Copy the entire `BIZFLOW` folder:
```bash
xcopy /E /I "C:\Users\%USERNAME%\AppData\Local\BIZFLOW" "D:\Backups\BIZFLOW"
```

### Restore
Copy the backed-up folder back:
```bash
xcopy /E /I "D:\Backups\BIZFLOW" "C:\Users\%USERNAME%\AppData\Local\BIZFLOW"
```

### Reset (Fresh Start)
Delete the BIZFLOW folder - new database will be created on next launch:
```bash
rmdir /S /Q "C:\Users\%USERNAME%\AppData\Local\BIZFLOW"
```

---

## 🐛 Troubleshooting

### Application Won't Start
1. Check if port 5000 is available:
   ```bash
   netstat -ano | findstr :5000
   ```
2. Try running as Administrator
3. Check Windows Firewall settings

### Browser Doesn't Open
- Application still starts successfully
- Manually open: `http://localhost:5000`
- Check console output for actual port

### Database Errors
1. Check database path exists
2. Verify write permissions to AppData folder
3. Delete database file to recreate

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 📞 Support & Documentation

### Project Links
- **Repository:** https://github.com/Mars007SV/BIZFLOW.Web
- **Issues:** https://github.com/Mars007SV/BIZFLOW.Web/issues

### Additional Documentation
- User authentication system: Built-in login/signup
- Multi-user support: Automatic per-Windows-user databases
- Technology stack: ASP.NET Core 10, Razor Pages, SQLite

---

## 🔄 Git Workflow

### Branches
- `main` - Stable production releases
- `develop` - Integration branch for features
- `feature/*` - Feature development branches

### For Developers
```bash
# Switch to develop
git checkout develop

# Create feature branch
git checkout -b feature/your-feature-name

# After development
git add .
git commit -m "feat: your feature description"
git checkout develop
git merge feature/your-feature-name
```

---

## ✨ Features

- ✅ Inventory management (add, edit, delete products)
- ✅ Category organization
- ✅ Operations history with full audit trail
- ✅ User authentication and authorization
- ✅ Multi-user support (separate databases per Windows user)
- ✅ Offline-first design
- ✅ Auto-open browser on launch
- ✅ Cross-platform ready (Windows, macOS, Linux)

---

**Last Updated:** 2025  
**Version:** 1.0  
**License:** MIT
