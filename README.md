# BIZFLOW - Inventory Management System

## 🎯 About

BIZFLOW is a professional inventory management system that allows you to:
- ✅ Track product stock levels
- ✅ Organize products by categories
- ✅ Maintain detailed operation history with full audit trail
- ✅ Work locally on your computer
- 🔐 **Secure user authentication and authorization**
- 👥 **Separate database for each Windows user**

## 🚀 Quick Start

### For Users
```bash
# Download and run
BIZFLOW.exe
```
Browser opens automatically at `http://localhost:5555`

### For Developers
```bash
# Clone repository
git clone https://github.com/Mars007SV/BIZFLOW.Web.git
cd BIZFLOW.Web

# Run development server
START-BIZFLOW.bat
```

**📖 Full Installation & Deployment Guide:** [SETUP.md](SETUP.md)

---

## 👥 Multi-User System

**Each Windows user has their own data!**

```
🖥️ One Computer
   ├── 👤 User 1 → Own database
   ├── 👤 User 2 → Own database
   └── 👤 User 3 → Own database
```

**Database location:**
```
C:\Users\[YourName]\AppData\Local\BIZFLOW\bizflow.db
```

---

## 🔐 Authentication

First time setup:
1. Click **"Sign Up"**
2. Enter username (3-50 characters)
3. Create password (minimum 6 characters)
4. Done! Start using the system

---

## 🛠️ Technology Stack

- **Backend:** ASP.NET Core 10 (Razor Pages)
- **Database:** SQLite with Entity Framework Core 10
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript
- **Reports:** ClosedXML (Excel generation)
- **Security:** SHA256 password hashing, Session-based auth
- **Desktop:** Electron.NET support ready

---

## ✨ Features

### 🔐 User Management
- User registration and login
- Secure password storage (SHA256)
- User profile editing
- Multi-user support with isolated databases

### 📦 Product Management
- Add, edit, delete products
- Track stock quantities
- Category organization
- Search and filter capabilities

### 📊 Operations History
- Automatic audit trail for all changes
- Detailed operation information
- Track old and new values
- Full history of inventory movements

### 📈 Reports
- Excel export functionality
- Inventory reports
- Operation history reports

---

## 📋 System Requirements

- **OS:** Windows 10/11 (x64), macOS, Linux
- **RAM:** 512 MB minimum
- **Disk Space:** 150 MB free space
- **.NET:** .NET 10 Runtime (included in self-contained builds)

---

## 📦 Project Structure

```
BIZFLOW.Web/
├── Controllers/           # MVC controllers
├── Models/               # Data models
├── Views/                # Razor views
├── Data/                 # DbContext and database
├── Migrations/           # EF Core migrations
├── Services/             # Business logic services
├── Middleware/           # Custom middleware
├── wwwroot/              # Static files (CSS, JS, images)
├── Properties/           # Launch settings
├── PUBLISH-DESKTOP.bat   # Build script for production
├── START-BIZFLOW.bat     # Development quick start
└── SETUP.md              # Full installation guide
```

---

## 📚 Documentation

- **[SETUP.md](SETUP.md)** - Complete installation, development, and deployment guide
- **[Docs/MultiUserSystem.md](Docs/MultiUserSystem.md)** - Multi-user database isolation
- **[Docs/AuthenticationGuide.md](Docs/AuthenticationGuide.md)** - Authentication system details

---

## 🔄 Git Workflow

### Branches
- `main` - Stable production releases
- `develop` - Integration branch for new features
- `feature/*` - Feature development branches

### Contributing
```bash
git checkout develop
git checkout -b feature/your-feature
# ... make changes ...
git commit -m "feat: your feature description"
git checkout develop
git merge feature/your-feature
```

---

## 🤝 Support

If you have questions or issues, please create an issue on GitHub:
https://github.com/Mars007SV/BIZFLOW.Web/issues

---

## 📄 License

MIT License - See LICENSE file for details

---

**Made with ❤️ for efficient inventory management**
