# 🌳 Git Workflow - BIZFLOW Project

## Branch Structure

| №  | Branch Name | Purpose | Integration Frequency |
|----|-------------|---------|----------------------|
| 1  | **main** | Stable production-ready code | Daily |
| 2  | **develop** | Integration branch for new features before merging to main | After task completion |
| 3  | **feature/*** | Temporary branches for developing specific features | Multiple times per day |

---

## 📋 Workflow Rules

### 1. Main Branch
- **Purpose:** Contains stable, production-ready code
- **Protection:** Direct commits are prohibited
- **Updates:** Only through Pull Requests from `develop`
- **Deployment:** Automatic CI/CD to production

### 2. Develop Branch
- **Purpose:** Integration branch for testing new features
- **Updates:** Merge from `feature/*` branches
- **Testing:** Full QA testing before merging to `main`

### 3. Feature Branches
- **Naming:** `feature/feature-name` (e.g., `feature/invoice-export`)
- **Purpose:** Isolated development of specific features
- **Lifespan:** Deleted after merging to `develop`

---

## 🔄 Standard Workflow

### Creating a New Feature

```powershell
# 1. Start from develop
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/new-feature-name

# 3. Make changes and commit
git add .
git commit -m "feat: Add new feature description"

# 4. Push to remote
git push -u origin feature/new-feature-name

# 5. Create Pull Request: feature/new-feature-name → develop
```

### Merging Feature to Develop

```powershell
# 1. Switch to develop
git checkout develop

# 2. Merge feature branch
git merge feature/new-feature-name

# 3. Push to remote
git push origin develop

# 4. Delete feature branch
git branch -d feature/new-feature-name
git push origin --delete feature/new-feature-name
```

### Releasing to Main

```powershell
# 1. Switch to main
git checkout main

# 2. Merge develop
git merge develop

# 3. Create version tag
git tag -a v1.0.0 -m "Release version 1.0.0"

# 4. Push to remote
git push origin main --tags
```

---

## 📝 Commit Message Convention

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>: <description>

[optional body]

[optional footer]
```

### Types:
- **feat:** New feature
- **fix:** Bug fix
- **docs:** Documentation changes
- **style:** Code style changes (formatting, no logic change)
- **refactor:** Code refactoring
- **test:** Adding or updating tests
- **chore:** Maintenance tasks (dependencies, build, etc.)
- **perf:** Performance improvements

### Examples:

```bash
git commit -m "feat: Add Electron.NET desktop support"
git commit -m "fix: Resolve database migration issue"
git commit -m "docs: Update installation guide"
git commit -m "refactor: Optimize invoice generation logic"
git commit -m "chore: Update NuGet packages"
```

---

## 🚀 Current Project Setup

### Existing Branches:
```bash
git branch -a
```

### Setup Complete Workflow:

```powershell
# 1. Ensure you're on develop
git checkout develop

# 2. Create feature branch for desktop app
git checkout -b feature/electron-desktop

# 3. Work on your feature...
# (changes already committed)

# 4. Push to remote
git push -u origin feature/electron-desktop

# 5. Create Pull Request on GitHub:
#    feature/electron-desktop → develop

# 6. After approval, merge to develop
git checkout develop
git merge feature/electron-desktop
git push origin develop

# 7. When ready for production
git checkout main
git merge develop
git tag -a v1.0.0 -m "Release: Desktop application with Electron.NET"
git push origin main --tags
```

---

## 🔐 Branch Protection Rules (GitHub Settings)

### For `main` branch:
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
- ✅ Require conversation resolution before merging
- ✅ Do not allow bypassing the above settings

### For `develop` branch:
- ✅ Require pull request reviews (optional)
- ✅ Require status checks to pass

---

## 📊 GitHub Actions Integration

Create `.github/workflows/ci.yml`:

```yaml
name: CI/CD Pipeline

on:
  push:
	branches: [ main, develop ]
  pull_request:
	branches: [ main, develop ]

jobs:
  build:
	runs-on: windows-latest

	steps:
	- uses: actions/checkout@v3

	- name: Setup .NET
	  uses: actions/setup-dotnet@v3
	  with:
		dotnet-version: '10.0.x'

	- name: Restore dependencies
	  run: dotnet restore

	- name: Build
	  run: dotnet build --no-restore

	- name: Test
	  run: dotnet test --no-build --verbosity normal

	- name: Publish (main branch only)
	  if: github.ref == 'refs/heads/main'
	  run: dotnet publish -c Release -o ./publish
```

---

## 📚 Quick Reference

```bash
# Check current branch
git branch

# View all branches (local + remote)
git branch -a

# Switch to existing branch
git checkout <branch-name>

# Create and switch to new branch
git checkout -b <branch-name>

# View commit history
git log --oneline --graph --all

# Sync with remote
git fetch --all
git pull origin <branch-name>

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Discard local changes
git restore <file>
git restore .
```

---

**Happy Coding! 🎉**
