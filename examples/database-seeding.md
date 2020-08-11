// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Database Seeding Guide

How to seed initial data into your database for development and testing.

## Approach 1: Using EF Core HasData() in Migrations

### Setup in DbContext

**File: `Data/AppDbContext.cs`**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Seed Products
    modelBuilder.Entity<Product>().HasData(
        new Product
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            Category = "Electronics",
            Stock = 5,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse",
            Price = 29.99m,
            Category = "Accessories",
            Stock = 50,
            CreatedAt = DateTime.UtcNow
        }
    );
    
    // Seed Users
    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440010"),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.UtcNow
        }
    );
}
```

### Create Migration

```bash
dotnet ef migrations add SeedInitialData
dotnet ef database update
```

**Benefits:**
- Data persists after migrations
- Reproducible across environments
- Good for reference data (categories, enums)

**Drawbacks:**
- Not ideal for large datasets
- Can clutter OnModelCreating
- Difficult to update existing seed data

## Approach 2: Dedicated Seeding Service

### Create Seeding Service

**File: `Data/SeedData.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Data
{
    /// Database seeding service for initial data
    public static class SeedData
    {
        public static async Task InitializeDatabaseAsync(
            AppDbContext context,
            ILogger<Program> logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Only seed if data doesn't exist
                if (context.Products.Any())
                {
                    logger.LogInformation("Database already seeded");
                    return;
                }

                logger.LogInformation("Seeding database with initial data...");

                // Create products
                var products = new List<Product>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Laptop",
                        Description = "High-performance laptop",
                        Price = 999.99m,
                        Category = "Electronics",
                        Stock = 5,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Monitor",
                        Description = "4K Monitor",
                        Price = 399.99m,
                        Category = "Electronics",
                        Stock = 10,
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Keyboard",
                        Description = "Mechanical keyboard",
                        Price = 149.99m,
                        Category = "Accessories",
                        Stock = 25,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(products);

                // Create users
                var users = new List<User>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john@example.com",
                        PhoneNumber = "+1234567890",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane@example.com",
                        PhoneNumber = "+0987654321",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(users);

                // Save changes
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding database");
                throw;
            }
        }
    }
}
```

### Call Seeding Service

**Update `Program.cs`:**

```csharp
// Build the application
var app = builder.Build();

// Seed database on startup
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        await SeedData.InitializeDatabaseAsync(context, logger);
    }
}

app.Run();
```

**Benefits:**
- Flexible and easy to update
- Good for development/testing data
- Can handle complex relationships

**Drawbacks:**
- Runs every startup (check for existing data)
- Not suitable for production
- Harder to track seed data versions

## Approach 3: Seed from JSON File

### Create Seed Data File

**File: `Data/seed-data.json`**

```json
{
  "products": [
    {
      "name": "Laptop",
      "description": "High-performance laptop",
      "price": 999.99,
      "category": "Electronics",
      "stock": 5
    },
    {
      "name": "Monitor",
      "description": "4K Monitor",
      "price": 399.99,
      "category": "Electronics",
      "stock": 10
    }
  ],
  "users": [
    {
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phone": "+1234567890"
    }
  ]
}
```

### Create JSON Seeding Service

```csharp
public class JsonSeeder
{
    public static async Task SeedFromJsonAsync(
        AppDbContext context,
        string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var seedData = JsonSerializer.Deserialize<SeedDataModel>(json);

        // Process products
        if (seedData?.Products != null)
        {
            foreach (var product in seedData.Products)
            {
                context.Products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Category = product.Category,
                    Stock = product.Stock
                });
            }
        }

        await context.SaveChangesAsync();
    }
}

public class SeedDataModel
{
    public List<ProductSeed> Products { get; set; }
    public List<UserSeed> Users { get; set; }
}

public class ProductSeed
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int Stock { get; set; }
}

public class UserSeed
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
```

**Benefits:**
- Easy to modify seed data
- Can be version controlled
- Reusable across projects

**Drawbacks:**
- Requires file management
- More complex implementation

## Approach 4: SQL Script Seeding

### Create SQL Seed Script

**File: `Data/seed.sql`**

```sql
-- Insert Products
INSERT INTO Products (Id, Name, Description, Price, Category, Stock, CreatedAt)
VALUES
(NEWID(), 'Laptop', 'High-performance laptop', 999.99, 'Electronics', 5, GETUTCDATE()),
(NEWID(), 'Monitor', '4K Monitor', 399.99, 'Electronics', 10, GETUTCDATE()),
(NEWID(), 'Keyboard', 'Mechanical keyboard', 149.99, 'Accessories', 25, GETUTCDATE());

-- Insert Users
INSERT INTO Users (Id, FirstName, LastName, Email, PhoneNumber, CreatedAt)
VALUES
(NEWID(), 'John', 'Doe', 'john@example.com', '+1234567890', GETUTCDATE()),
(NEWID(), 'Jane', 'Smith', 'jane@example.com', '+0987654321', GETUTCDATE());
```

### Execute Script

```bash
sqlcmd -S localhost -U sa -P password -d AspNetSpaTemplate -i seed.sql
```

Or in C#:

```csharp
var seedScript = await File.ReadAllTextAsync("Data/seed.sql");
await context.Database.ExecuteSqlRawAsync(seedScript);
```

## Best Practices

1. **Check for Existing Data**
   ```csharp
   if (context.Products.Any())
       return; // Already seeded
   ```

2. **Use GUIDs for IDs**
   ```csharp
   Id = Guid.NewGuid()
   ```

3. **Set UTC Timestamps**
   ```csharp
   CreatedAt = DateTime.UtcNow
   ```

4. **Test Seed Data**
   ```csharp
   Assert.Equal(expectedCount, context.Products.Count());
   ```

5. **Keep Seed Data Minimal**
   - Only essential reference data
   - Use for development/testing only
   - Don't include sensitive data

6. **Document Seed Data**
   ```csharp
   // Test user for development (remove in production)
   // Email: john@example.com, Password: Test123!
   ```

## Complete Example

See the full database initialization in the project's `Data/AppDbContext.cs` and `Program.cs` files.

## Common Issues

### "Duplicate key value violates unique constraint"
- Check if data already exists before seeding
- Clear database before seeding: `dotnet ef database drop`

### "Foreign key constraint fails"
- Seed parent entities before child entities
- Ensure referenced IDs exist

### Seed data not appearing
- Check if seeding code is called
- Verify database connection string
- Check for exceptions in logs

## See Also

- [Entity Framework Core Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [Testing with Seed Data](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/test-asp-net-core-mvc-apps)
