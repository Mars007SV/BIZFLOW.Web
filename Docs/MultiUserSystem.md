# BIZFLOW Multi-User System

## How It Works

Each Windows user on one computer has their own separate database and user accounts.

---

## Where Data Is Stored

### Database path:
```
C:\Users\[USERNAME]\AppData\Local\BIZFLOW\bizflow.db
```

### Examples:
- User "Oleh": C:\Users\Oleh\AppData\Local\BIZFLOW\bizflow.db
- User "Maria": C:\Users\Maria\AppData\Local\BIZFLOW\bizflow.db
- User "Admin": C:\Users\Admin\AppData\Local\BIZFLOW\bizflow.db

---

## Data Isolation

### What this means:

1. Oleh logs in to Windows as "Oleh"
   - Runs BIZFLOW
   - Sees only his products, categories, operations
   - Has his own user accounts

2. Maria logs in to Windows as "Maria"
   - Runs BIZFLOW on the same computer
   - Sees only her data
   - Cannot see Oleh's data
   - Has separate database

3. Administrator logs in to Windows as "Admin"
   - Has his own separate database
   - Cannot see Oleh's or Maria's data

---

## First Launch

### For each Windows user:

1. Log in to Windows with your account
2. Run BIZFLOW: dotnet run
3. On first launch:
   ```
   User data directory: C:\Users\YourName\AppData\Local\BIZFLOW
   Database: C:\Users\YourName\AppData\Local\BIZFLOW\bizflow.db
   Checking and applying migrations...
   Created directory for user: YourName
   Database ready!
   ```
4. Register your user in BIZFLOW system
5. Start working!

---

## Usage Scenarios

### Scenario 1: Family Computer
```
One computer at home

Dad (Windows user: Dad)
   -> Own database
   -> Own store / warehouse
   -> Own products

Mom (Windows user: Mom)  
   -> Own database
   -> Own business
   -> Own products

Son (Windows user: Son)
   -> Own database
   -> Learning project
   -> Own data
```

### Scenario 2: Office Computer
```
One computer in office

Manager (Windows user: Manager)
   -> Database with store products
   -> Users: self + cashier

Accountant (Windows user: Accountant)
   -> Own database
   -> Only financial data

Warehouse (Windows user: Warehouse)
   -> Own database
   -> Warehouse inventory
```

---

## Data Verification

### How to see where your database is stored:

```powershell
# Open current user data folder
explorer "$env:LOCALAPPDATA\BIZFLOW"

# Or manually
# 1. Press Win+R
# 2. Enter: %LOCALAPPDATA%\BIZFLOW
# 3. Press Enter
```

### What you will see:
```
BIZFLOW\
   bizflow.db (your database)
```

---

## Backup

### Save your data:

```powershell
# Create backup
Copy-Item "$env:LOCALAPPDATA\BIZFLOW\bizflow.db" `
          "$env:USERPROFILE\Desktop\BIZFLOW_Backup_$(Get-Date -Format 'yyyy-MM-dd').db"
```

### Restore from backup:

```powershell
# Restore data
Copy-Item "$env:USERPROFILE\Desktop\BIZFLOW_Backup_2026-05-12.db" `
          "$env:LOCALAPPDATA\BIZFLOW\bizflow.db" -Force
```

---

## Data Migration

### When updating the program:

Migrations applied automatically on startup for each user separately.

```
User 1 starts -> migrations applied to his database
User 2 starts -> migrations applied to his database
```

---

## Important to Know

### Advantages:
- Complete data isolation between Windows users
- Each can have own accounts
- No need to logout from BIZFLOW when changing Windows user
- Simple backup (just a file)

### Limitations:
- Windows users cannot see each other's data
- For shared work need network database (SQL Server, PostgreSQL)
- Backups must be made for each user separately

---

## Shared Work (future)

If need multiple people working with same data:

### Option 1: One Windows Account
```
All login under one Windows user -> see same data
```

### Option 2: Network Database (requires development)
```
SQL Server / PostgreSQL -> all connect to one database
```

---

## Technical Details

### Path determination code:
```csharp
static string GetUserDataPath()
{
    var userName = Environment.UserName;
    var appDataPath = Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData
    );
    var userDataPath = Path.Combine(appDataPath, "BIZFLOW");

    if (!Directory.Exists(userDataPath))
    {
        Directory.CreateDirectory(userDataPath);
    }

    return userDataPath;
}
```

### Connection String:
```csharp
var dbPath = Path.Combine(userDataPath, "bizflow.db");
var connectionString = $"Data Source={dbPath}";
```

---

## Done!

Now each Windows user has:
- Own separate database
- Own BIZFLOW accounts
- Own products and operations
- Complete isolation from other users

Run BIZFLOW under your Windows account and create your user!
