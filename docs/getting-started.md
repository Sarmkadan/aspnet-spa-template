// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with ASP.NET SPA Template

This guide will help you set up and run the aspnet-spa-template project on your local machine.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Installation Steps](#installation-steps)
- [Verify Installation](#verify-installation)
- [First Run](#first-run)
- [Next Steps](#next-steps)

## Prerequisites

Before starting, ensure you have the following installed:

### Required
- **.NET 10 SDK** (version 10.0 or higher)
  - [Download from Microsoft](https://dotnet.microsoft.com/en-us/download)
  - Verify: `dotnet --version`

- **SQL Server** (2019 or higher) or SQL Server Express
  - [Download SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-express)
  - For development, LocalDB works fine

- **Git** for version control
  - [Download Git](https://git-scm.com/)

### Optional
- **Visual Studio 2022** Community Edition (for easier development)
- **SQL Server Management Studio** (for database management)
- **Postman** or **Thunder Client** (for API testing)

## Installation Steps

### Step 1: Clone the Repository

```bash
# Clone from GitHub
git clone https://github.com/Sarmkadan/aspnet-spa-template.git

# Navigate to project directory
cd aspnet-spa-template
```

### Step 2: Restore NuGet Packages

```bash
# Restore all project dependencies
dotnet restore
```

This command reads the `.csproj` file and downloads all required NuGet packages.

### Step 3: Configure Database Connection

#### Option A: Using LocalDB (Recommended for Development)

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "(localdb)\\mssqllocaldb;Initial Catalog=AspNetSpaTemplate;Integrated Security=true;"
  }
}
```

#### Option B: Using Full SQL Server Instance

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AspNetSpaTemplate;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### Option C: Using SQL Server on Docker

If you have Docker, use the provided docker-compose.yml:

```bash
docker-compose up -d
```

Then update connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AspNetSpaTemplate;User ID=sa;Password=YourSecurePassword123!;TrustServerCertificate=true;"
  }
}
```

### Step 4: Create Database and Run Migrations

```bash
# Create initial migration if not exists
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update
```

You should see output like:
```
Done. To undo this action, use 'dotnet ef database update' with no parameters
```

### Step 5: Trust HTTPS Certificate

```bash
# Clean any existing certificates
dotnet dev-certs https --clean

# Create and trust new certificate
dotnet dev-certs https --trust
```

When prompted, accept the security warning to trust the certificate.

### Step 6: Build the Project

```bash
# Build in Release configuration
dotnet build --configuration Release
```

## Verify Installation

### Check .NET Installation

```bash
dotnet --version
# Should output: 10.x.x or higher
```

### Verify Database Connection

```bash
# Test if migrations can connect to database
dotnet ef migrations list

# Should show: InitialCreate
```

### Check Project Structure

```bash
# List main directories
ls -la

# Should contain:
# Controllers/
# Services/
# Data/
# Models/
# wwwroot/
# Program.cs
```

## First Run

### Using Command Line

```bash
# Run the application
dotnet run

# Output should show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7001
#       Now listening on: http://localhost:5000
```

### Using Visual Studio

1. Open `AspNetSpaTemplate.sln`
2. Press `F5` or click "Start Debugging"
3. Browser opens to `https://localhost:7001`

### Access the Application

1. Open browser: `https://localhost:7001`
2. You should see the SPA frontend
3. Open browser console (F12) to check for errors
4. Navigate to `https://localhost:7001/api/products` to test API

## Next Steps

### 1. Explore the Codebase

- Start with `Program.cs` to understand dependency injection
- Review `Controllers/ProductsController.cs` for API structure
- Check `Services/ProductService.cs` for business logic
- Look at `wwwroot/js/app.js` for frontend code

### 2. Create Your First Entity

1. Add a new model in `Models/`
2. Create repository in `Data/Repositories/`
3. Create service in `Services/`
4. Add controller in `Controllers/`
5. Use API to test

### 3. Modify the Database

```bash
# Make changes to entities in Models/
# Then create migration:
dotnet ef migrations add YourMigrationName

# Review the generated migration
dotnet ef database update
```

### 4. Debug the Application

```bash
# Run with detailed logging
ASPNETCORE_ENVIRONMENT=Development dotnet run --configuration Debug
```

### 5. Run Tests (if implemented)

```bash
dotnet test
```

## Troubleshooting

### Issue: "Cannot connect to database"

**Solution:**
1. Verify SQL Server is running
2. Check connection string in appsettings
3. Test connection with SSMS
4. Ensure database user has permissions

### Issue: "HTTPS certificate not trusted"

**Solution:**
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Issue: "Port 5000/7001 already in use"

**Solution:**
```bash
dotnet run -- --urls="https://localhost:7002;http://localhost:5001"
```

### Issue: "EF Core migration failed"

**Solution:**
```bash
# Reset migrations (WARNING: deletes database!)
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Development Workflow

### Daily Development

```bash
# 1. Start application
dotnet run

# 2. In another terminal, watch for file changes
dotnet watch run

# 3. Test APIs with curl or Postman
curl https://localhost:7001/api/products

# 4. Check logs in console
# 5. Open browser to https://localhost:7001
```

### Making Changes

1. Edit code in your IDE
2. Application auto-restarts if using `dotnet watch run`
3. Refresh browser to see frontend changes
4. Use browser DevTools (F12) to debug JavaScript

### Database Changes

1. Update entity in `Models/`
2. Create migration: `dotnet ef migrations add DescriptiveNameç
3. Review generated migration file
4. Update database: `dotnet ef database update`
5. Test with API

## Environment Configuration

### Development Environment

Used automatically when:
- `ASPNETCORE_ENVIRONMENT=Development` is set
- Running `dotnet run` without Release configuration
- Loads `appsettings.Development.json`

Features:
- Detailed logging
- HTTPS enforced
- In-memory database options available
- Debug information enabled

### Production Environment

Configure with:
```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run --configuration Release
```

Features:
- Minimal logging
- Error handling more strict
- Performance optimized
- Security hardened

## Next Learning Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Guide](https://learn.microsoft.com/en-us/ef/core/)
- [MDN JavaScript Guide](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide)
- Review the project's `docs/` directory for advanced topics

---

**For additional help:**
- Check `docs/faq.md` for common questions
- Review `docs/troubleshooting.md` for error solutions
- Open an issue on GitHub for bugs or missing documentation
