// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# System Architecture

This document describes the architecture of the aspnet-spa-template project, including design patterns, technology choices, and how components interact.

## Table of Contents
- [Architectural Overview](#architectural-overview)
- [Layered Architecture](#layered-architecture)
- [Design Patterns](#design-patterns)
- [Technology Stack](#technology-stack)
- [Data Flow](#data-flow)
- [Middleware Pipeline](#middleware-pipeline)
- [Dependency Injection](#dependency-injection)
- [Error Handling Strategy](#error-handling-strategy)

## Architectural Overview

The project follows a **Layered N-Tier Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────┐
│  Presentation Layer             │
│  (Controllers, API Responses)   │
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│  Business Logic Layer            │
│  (Services, Validation)          │
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│  Data Access Layer               │
│  (Repositories, Entity Framework)│
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│  Persistence Layer               │
│  (SQL Server Database)           │
└─────────────────────────────────┘
```

### Key Principles

1. **Single Responsibility**: Each layer has one reason to change
2. **Dependency Inversion**: Depend on abstractions, not implementations
3. **Don't Repeat Yourself**: Code reuse through inheritance and composition
4. **Fail Fast**: Validate early, error handling is explicit

## Layered Architecture

### 1. Presentation Layer (Controllers)

**Location:** `Controllers/`

Handles HTTP requests and returns responses.

**Responsibilities:**
- Accept HTTP requests
- Parse request data
- Call appropriate services
- Format responses
- Return HTTP status codes

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ApiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
  {
    var products = await _service.GetAllAsync();
    return Ok(ApiResponse.Success(products));
  }
}
```

### 2. Business Logic Layer (Services)

**Location:** `Services/`

Contains business rules and application logic.

**Responsibilities:**
- Implement business rules
- Coordinate between controllers and repositories
- Handle validation
- Manage transactions
- Cache data when appropriate

**Example:**
```csharp
public class ProductService : IProductService
{
  public async Task<ProductDto> CreateAsync(CreateProductRequest request)
  {
    // Validate request
    if (request.Price <= 0)
      throw new ValidationException("Price must be positive");
    
    // Create entity
    var product = new Product { /* ... */ };
    
    // Persist
    await _repository.AddAsync(product);
    await _repository.SaveAsync();
    
    return MapToDto(product);
  }
}
```

### 3. Data Access Layer (Repositories)

**Location:** `Data/Repositories/`

Abstracts database operations.

**Responsibilities:**
- Query database
- Save entities
- Handle EF Core operations
- Encapsulate data access logic

**Pattern:**
```csharp
public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
  public async Task<Product> GetByNameAsync(string name)
  {
    return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
  }
}
```

### 4. Persistence Layer (Database)

**Location:** `Data/AppDbContext.cs`

Entity Framework Core context manages database connections and migrations.

## Design Patterns

### Repository Pattern

Abstracts data access layer to make it testable and replaceable.

```csharp
// Interface (abstraction)
public interface IProductRepository
{
  Task<List<Product>> GetAllAsync();
  Task<Product> GetByIdAsync(Guid id);
  Task AddAsync(Product product);
}

// Implementation
public class ProductRepository : IProductRepository
{
  public async Task<List<Product>> GetAllAsync()
  {
    return await _dbSet.ToListAsync();
  }
}
```

### Service Layer Pattern

Contains business logic separate from data access.

```csharp
public class ProductService
{
  private readonly IProductRepository _repository;
  
  public ProductService(IProductRepository repository)
  {
    _repository = repository;
  }
  
  public async Task<ProductDto> GetAsync(Guid id)
  {
    var product = await _repository.GetByIdAsync(id);
    if (product == null)
      throw new NotFoundException("Product not found");
    return MapToDto(product);
  }
}
```

### Dependency Injection Pattern

Built into ASP.NET Core, used throughout the application.

```csharp
// Registration in Program.cs
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IProductService, ProductService>();

// Usage in controller
public ProductsController(IProductService service)
{
  _service = service;
}
```

### Data Transfer Object (DTO) Pattern

Transfer data between layers without exposing domain models.

```csharp
// Domain Model
public class Product
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

// DTO (what API returns)
public class ProductDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
  // Note: CreatedAt and UpdatedAt hidden from API
}
```

### Middleware Pattern

Processes requests and responses through a pipeline.

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

// Each middleware has similar structure:
public class ExceptionHandlingMiddleware
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      await HandleExceptionAsync(context, ex);
    }
  }
}
```

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 10
- **Language**: C# 13
- **Database**: SQL Server 2019+ with Entity Framework Core
- **Dependency Injection**: Built-in ASP.NET Core DI container
- **API Format**: RESTful JSON

### Frontend
- **Language**: JavaScript (ES6+)
- **UI Framework**: None (Vanilla JavaScript)
- **DOM Manipulation**: Native DOM APIs
- **HTTP Client**: Fetch API
- **Styling**: Pure CSS3

### Supporting Technologies
- **Caching**: In-Memory Cache (IMemoryCache)
- **Background Jobs**: Hosted Services
- **Logging**: ILogger (built-in)
- **Validation**: Data Annotations + Custom Validators

## Data Flow

### Request/Response Flow

```
1. HTTP Request arrives
   ↓
2. Middleware pipeline processes (auth, logging, etc.)
   ↓
3. Route matched to Controller action
   ↓
4. Controller receives request and calls Service
   ↓
5. Service validates and executes business logic
   ↓
6. Service calls Repository for data access
   ↓
7. Repository queries Database via EF Core
   ↓
8. Data returned through layers: Repository → Service → Controller
   ↓
9. Controller creates ApiResponse<T> with data
   ↓
10. Response serialized to JSON
   ↓
11. Middleware pipeline post-processes response
   ↓
12. HTTP Response sent to client
```

### Example: Get Product by ID

```csharp
// 1. Frontend makes request
fetch('/api/products/123')
  .then(response => response.json())
  .then(data => console.log(data));

// 2. Controller receives request
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
{
  // 3. Calls service
  var product = await _service.GetByIdAsync(id);
  
  // 4. Returns response
  return Ok(ApiResponse.Success(product));
}

// 5. Service executes logic
public async Task<ProductDto> GetByIdAsync(Guid id)
{
  // 6. Calls repository
  var product = await _repository.GetByIdAsync(id);
  
  // 7. Throws if not found
  if (product == null)
    throw new NotFoundException("Product not found");
  
  // 8. Maps to DTO
  return MapToDto(product);
}

// 9. Repository queries database
public async Task<Product> GetByIdAsync(Guid id)
{
  return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
}
```

## Middleware Pipeline

Configured in `Program.cs`, middleware processes requests sequentially:

```
Request
  ↓
┌─────────────────────────────────┐
│ ExceptionHandlingMiddleware     │ - Catches all exceptions
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ CorrelationIdMiddleware         │ - Adds request ID
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ LoggingMiddleware               │ - Logs requests/responses
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ AuthenticationMiddleware        │ - Validates user
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ RateLimitingMiddleware          │ - Prevents abuse
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ CoreMiddleware (Routing, etc)   │ - Routes to controller
└──────────────┬──────────────────┘
               ↓
         Response
```

## Dependency Injection

### Registration Pattern

```csharp
// Program.cs - ConfigureServices
var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// Event Bus
builder.Services.AddSingleton<IEventBus, EventBusImplementation>();

// Background Workers
builder.Services.AddHostedService<CacheMaintenanceWorker>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();
```

### Lifetimes

- **Transient**: New instance per request (use for stateless services)
- **Scoped**: One instance per HTTP request (typical for repositories)
- **Singleton**: One instance for application lifetime (caching, logging)

## Error Handling Strategy

### Custom Exception Hierarchy

```csharp
public abstract class ApplicationException : Exception { }

public class NotFoundException : ApplicationException { }
public class ValidationException : ApplicationException { }
public class BusinessException : ApplicationException { }
public class UnauthorizedException : ApplicationException { }
```

### Global Exception Handler

```csharp
// Middleware catches all exceptions
public class ExceptionHandlingMiddleware
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (NotFoundException ex)
    {
      context.Response.StatusCode = StatusCodes.Status404NotFound;
      await context.Response.WriteAsJsonAsync(
        ErrorResponse.NotFound(ex.Message));
    }
    catch (ValidationException ex)
    {
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      await context.Response.WriteAsJsonAsync(
        ErrorResponse.BadRequest(ex.Message));
    }
    catch (Exception ex)
    {
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      await context.Response.WriteAsJsonAsync(
        ErrorResponse.InternalError("An error occurred"));
    }
  }
}
```

### Frontend Error Handling

```javascript
async function handleApiError(response) {
  if (!response.ok) {
    const error = await response.json();
    console.error('API Error:', error);
    throw error;
  }
  return response.json();
}

fetch('/api/products')
  .then(handleApiError)
  .catch(error => {
    if (error.statusCode === 404) {
      showNotification('Product not found');
    } else if (error.statusCode === 400) {
      showNotification('Invalid request');
    } else {
      showNotification('Server error');
    }
  });
```

---

For more details on specific areas, see related documentation files.
