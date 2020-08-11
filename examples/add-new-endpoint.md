// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Adding a New API Endpoint - Step by Step Guide

This guide shows how to add a complete new API endpoint to the aspnet-spa-template.

## Scenario: Add a "Category" API

We'll add full CRUD operations for a Category entity.

## Step 1: Create the Domain Model

**File: `Models/Category.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace AspNetSpaTemplate.Models
{
    /// Represents a product category
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
```

## Step 2: Create Data Transfer Object (DTO)

**File: `DTOs/CategoryDto.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs
{
    /// DTO for transferring category data
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
    }
}
```

## Step 3: Create Repository Interface and Implementation

**File: `Data/Repositories/ICategoryRepository.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Data.Repositories
{
    /// Repository interface for Category data access
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> GetByNameAsync(string name);
        Task<List<Category>> GetWithProductCountAsync();
    }
}
```

**File: `Data/Repositories/CategoryRepository.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetSpaTemplate.Data.Repositories
{
    /// Repository for Category data access
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<List<Category>> GetWithProductCountAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .ToListAsync();
        }
    }
}
```

## Step 4: Create Service Interface and Implementation

**File: `Services/ICategoryService.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;

namespace AspNetSpaTemplate.Services
{
    /// Service interface for category operations
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(string name, string description);
        Task UpdateAsync(Guid id, string name, string description);
        Task DeleteAsync(Guid id);
    }
}
```

**File: `Services/CategoryService.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Services
{
    /// Service for category business logic
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.ConvertAll(MapToDto);
        }

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");
            return MapToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Category name is required");

            var existing = await _repository.GetByNameAsync(name);
            if (existing != null)
                throw new ValidationException("Category name already exists");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description
            };

            await _repository.AddAsync(category);
            await _repository.SaveAsync();
            return MapToDto(category);
        }

        public async Task UpdateAsync(Guid id, string name, string description)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            if (!string.IsNullOrWhiteSpace(name))
                category.Name = name;
            if (!string.IsNullOrWhiteSpace(description))
                category.Description = description;

            category.UpdatedAt = DateTime.UtcNow;
            _repository.Update(category);
            await _repository.SaveAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            _repository.Delete(category);
            await _repository.SaveAsync();
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count ?? 0
            };
        }
    }
}
```

## Step 5: Create Controller

**File: `Controllers/CategoriesController.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers
{
    /// Controller for category API endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        /// Get all categories
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(ApiResponse.Success(categories));
        }

        /// Get category by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success(category));
        }

        /// Create new category
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(
            [FromBody] CreateCategoryRequest request)
        {
            var category = await _service.CreateAsync(
                request.Name, request.Description);
            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                ApiResponse.Success(category, "Category created"));
        }

        /// Update category
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(
            Guid id,
            [FromBody] UpdateCategoryRequest request)
        {
            await _service.UpdateAsync(id, request.Name, request.Description);
            return Ok(ApiResponse.Success("Category updated"));
        }

        /// Delete category
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
```

## Step 6: Create Request DTOs

**File: `DTOs/CreateCategoryRequest.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs
{
    /// Request DTO for creating a category
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
```

**File: `DTOs/UpdateCategoryRequest.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs
{
    /// Request DTO for updating a category
    public class UpdateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
```

## Step 7: Register Dependencies

**Update `Program.cs`:**

```csharp
// Add after other service registrations
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
```

## Step 8: Create Database Migration

```bash
# Create migration
dotnet ef migrations add AddCategory

# Apply migration
dotnet ef database update
```

## Step 9: Test the Endpoint

```bash
# Get all categories
curl https://localhost:7001/api/categories

# Create category
curl -X POST https://localhost:7001/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"Electronics","description":"Electronic products"}'

# Get specific category
curl https://localhost:7001/api/categories/550e8400-e29b-41d4-a716-446655440000

# Update category
curl -X PUT https://localhost:7001/api/categories/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Electronics"}'

# Delete category
curl -X DELETE https://localhost:7001/api/categories/550e8400-e29b-41d4-a716-446655440000
```

## Checklist

- [x] Create domain model in `Models/`
- [x] Create DTOs in `DTOs/`
- [x] Create repository interface and implementation
- [x] Create service interface and implementation
- [x] Create controller with all CRUD endpoints
- [x] Register dependencies in `Program.cs`
- [x] Create database migration
- [x] Test all endpoints

## Best Practices Applied

1. **Separation of Concerns** - Each layer has specific responsibilities
2. **DI Pattern** - Dependencies injected via constructor
3. **DTOs** - Data not exposed directly from domain models
4. **Validation** - Input validation in service layer
5. **Error Handling** - Custom exceptions thrown and handled
6. **Async/Await** - All I/O operations are asynchronous
7. **Naming Conventions** - Clear, descriptive names
8. **Comments** - Brief, focused documentation
