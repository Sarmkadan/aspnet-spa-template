// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions (FAQ)

Common questions about the aspnet-spa-template project.

## Table of Contents
- [General Questions](#general-questions)
- [Installation & Setup](#installation--setup)
- [Development](#development)
- [Database](#database)
- [Frontend](#frontend)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## General Questions

### Q: Why vanilla JavaScript instead of React/Vue/Angular?

**A:** This template demonstrates that modern, professional web applications don't always need heavy frameworks. Vanilla JavaScript with proper architecture provides:
- Smaller bundle size
- No framework lock-in
- Better for learning fundamentals
- Perfect for smaller applications
- Works great for admin panels and dashboards

For complex applications requiring advanced features, React is still a good choice.

### Q: Is this production-ready?

**A:** Yes, this template follows production best practices:
- Proper error handling
- Security considerations (CORS, validation)
- Performance optimizations (caching)
- Scalable architecture
- Comprehensive logging
- Health checks
- Docker support

You should still conduct security audits and performance testing before deploying to production.

### Q: Can I use this as a project template?

**A:** Absolutely! This is the purpose of the template. Clone or fork it to start your own project. Update:
- Project name in `.csproj`
- Namespace in C# files
- Application names
- Configuration files

### Q: What .NET version should I use?

**A:** Use .NET 10 or later. This template targets `net10.0` and uses latest C# features. If you need older .NET versions, update the `TargetFramework` in `.csproj` and remove unsupported features.

### Q: Is there API versioning?

**A:** Not in the base template, but it's easy to add:
- Use route prefixes: `/api/v1/products`
- Use header-based versioning
- Use query parameter versioning

See examples in the `examples/` directory for implementation.

## Installation & Setup

### Q: I'm getting "Cannot connect to database" error

**A:** Check:
1. SQL Server is running
2. Connection string is correct
3. Database exists (or let EF Core create it)
4. User has permissions
5. Firewall allows connection

Try:
```bash
dotnet ef database create
```

### Q: How do I use SQL Server Express instead of LocalDB?

**A:** Update connection string in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AspNetSpaTemplate;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Q: Can I use PostgreSQL or MySQL instead of SQL Server?

**A:** Yes! EF Core supports multiple databases:

**For PostgreSQL:**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Update `Program.cs`:
```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("ConnectionString"));
```

**For MySQL:**
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### Q: How do I run migrations on a remote database?

**A:** Set connection string and run:
```bash
ASPNETCORE_ENVIRONMENT=Production \
dotnet ef database update
```

Or use the Azure SQL command:
```bash
dotnet ef database update --project . \
  --connection "Server=server.database.windows.net;..."
```

### Q: Do I need Visual Studio or can I use VS Code?

**A:** VS Code works great! You only need:
- .NET 10 SDK
- VS Code with C# extension
- Integrated terminal

Visual Studio has more features but is optional.

## Development

### Q: How do I add a new API endpoint?

**A:** Follow this pattern:

1. **Create Model** (`Models/MyEntity.cs`)
2. **Create Repository** (`Data/Repositories/IMyRepository.cs` + implementation)
3. **Create Service** (`Services/MyService.cs`)
4. **Create DTO** (`DTOs/MyEntityDto.cs`)
5. **Create Controller** (`Controllers/MyController.cs`)
6. **Register in DI** (`Program.cs`)

See `examples/add-new-endpoint.md` for detailed example.

### Q: How do I write tests?

**A:** The template doesn't include tests by default. Add them:

```bash
# Create test project
dotnet new xunit -n AspNetSpaTemplate.Tests

# Add to solution
dotnet sln add AspNetSpaTemplate.Tests/AspNetSpaTemplate.Tests.csproj

# Add reference to main project
cd AspNetSpaTemplate.Tests
dotnet add reference ../AspNetSpaTemplate.csproj
```

Example test:
```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var service = new ProductService(mockRepo.Object);
        
        // Act
        var result = await service.GetAsync(Guid.NewGuid());
        
        // Assert
        Assert.NotNull(result);
    }
}
```

### Q: How do I add authentication/authorization?

**A:** The template has auth middleware hooks. To implement:

1. Install authentication package:
   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

2. Configure in `Program.cs`:
   ```csharp
   services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => { /* config */ });
   ```

3. Add `[Authorize]` attribute to controllers
4. Implement login endpoint to issue tokens

See `examples/authentication-jwt.md` for full example.

### Q: How do I debug the application?

**A:** Multiple ways:

**VS Code:**
1. F5 or Debug > Start Debugging
2. Set breakpoints
3. Step through code

**Command line:**
```bash
dotnet run
# Or with debugging symbols:
dotnet run --configuration Debug
```

**Browser DevTools:**
- F12 to open
- Console tab for JavaScript logs
- Network tab to inspect API calls

### Q: How do I profile performance?

**A:** Use built-in tools:

```csharp
// Add timing to service
var sw = Stopwatch.StartNew();
var result = await _repository.GetAllAsync();
sw.Stop();
_logger.LogInformation($"Query took {sw.ElapsedMilliseconds}ms");
```

Or use:
- Application Insights
- dotTrace (JetBrains)
- Visual Studio Profiler

### Q: Can I use async/await for all operations?

**A:** Yes, and you should! The template is designed for async:

```csharp
// Good - async all the way
public async Task<List<Product>> GetAsync()
{
    return await _repository.GetAllAsync();
}

// Avoid - blocking
public List<Product> Get()
{
    return _repository.GetAllAsync().Result; // DON'T DO THIS
}
```

## Database

### Q: How do I add a new database migration?

**A:** Create migration after changing entities:
```bash
dotnet ef migrations add DescriptiveName
dotnet ef database update
```

### Q: How do I revert a migration?

**A:** Remove migration and revert database:
```bash
# Revert to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Q: Can I use stored procedures?

**A:** Yes! EF Core supports stored procedures:

```csharp
var result = await _context.Products
    .FromSqlRaw("EXECUTE GetTopProducts @count", 
        new SqlParameter("@count", 10))
    .ToListAsync();
```

### Q: How do I handle database timeouts?

**A:** Configure in `Program.cs`:

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        b => b.CommandTimeout(60))); // 60 seconds
```

### Q: What about data seeding?

**A:** Add to `AppDbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<Product>().HasData(
        new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 999.99 }
    );
}
```

Then run migrations.

## Frontend

### Q: How do I make the frontend reactive (update without refresh)?

**A:** Use Fetch API with event listeners:

```javascript
document.getElementById('addBtn').addEventListener('click', async () => {
    const response = await fetch('/api/products', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: 'New Product' })
    });
    
    const result = await response.json();
    if (result.success) {
        // Update UI without refresh
        addProductToDOM(result.data);
    }
});
```

### Q: Can I use a frontend framework like React?

**A:** Absolutely! The backend is completely decoupled from frontend. You can:
- Keep vanilla JS
- Add React
- Use Vue, Angular, Svelte, etc.
- Build mobile app with same API

Just update the frontend in `wwwroot/` directory.

### Q: How do I handle forms properly?

**A:** Use HTML `<form>` elements and prevent default:

```javascript
document.getElementById('productForm').addEventListener('submit', async (e) => {
    e.preventDefault(); // Prevent page reload
    
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData);
    
    const response = await fetch('/api/products', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
    
    if (!response.ok) {
        // Show error
        const error = await response.json();
        showError(error.message);
    } else {
        // Success handling
        showSuccess('Product created!');
    }
});
```

### Q: How do I handle file uploads?

**A:** Use FormData and multipart request:

```javascript
const fileInput = document.getElementById('fileInput');
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch('/api/products/upload', {
    method: 'POST',
    body: formData
    // Don't set Content-Type, browser will set it
});
```

Backend:
```csharp
[HttpPost("upload")]
public async Task<IActionResult> UploadImage(IFormFile file)
{
    if (file.Length == 0)
        return BadRequest("File is empty");
    
    // Process file
    return Ok();
}
```

## Deployment

### Q: How do I deploy to production?

**A:** Multiple options documented in `docs/deployment.md`:
- Docker
- Azure
- AWS
- IIS
- Linux/VM

### Q: Should I use Docker?

**A:** Yes, recommended for:
- Consistency across environments
- Easy scaling
- CI/CD integration
- Containerized deployments

No Docker needed for:
- Small single-server deployments
- Existing infrastructure
- Learning purposes

### Q: How do I set up CI/CD?

**A:** GitHub Actions workflow included (`.github/workflows/build.yml`):
- Builds on push
- Runs tests
- Creates Docker image
- Pushes to registry

Customize for your needs.

### Q: What about SSL certificates?

**A:** Use:
- **Let's Encrypt** (free, auto-renewal)
- **Azure App Service** (auto-managed)
- **Self-signed** (development only)
- **Commercial certificates**

For nginx:
```bash
certbot certonly --nginx -d yourdomain.com
```

## Troubleshooting

### Q: API returns 500 error but no details

**A:** Check logs:
```bash
# Console output
dotnet run

# Log files
tail -f logs/app-*.txt

# Application Insights (if configured)
```

Add detailed logging:
```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Q: Frontend can't connect to backend

**A:** Check:
1. CORS configuration in `Program.cs`
2. API URL is correct
3. Backend is running
4. Network requests in browser DevTools (F12 > Network)

### Q: Migrations fail with "An error occurred"

**A:** Get more details:
```bash
dotnet ef database update --verbose
```

Common causes:
- Connection string invalid
- Database permissions
- Missing migration files
- Conflicting migrations

### Q: High memory usage

**A:** Check:
1. Memory leaks in services
2. Large queries without pagination
3. Cache not being cleared
4. Background workers

Monitor with:
```bash
dotnet-counters monitor -n AspNetSpaTemplate
```

---

**Can't find the answer? Open an issue on GitHub or check the documentation files.**
