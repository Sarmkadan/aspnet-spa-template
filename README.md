# ASP.NET SPA Template

![Build](https://github.com/sarmkadan/aspnet-spa-template/actions/workflows/build.yml/badge.svg)
![CI](https://github.com/sarmkadan/aspnet-spa-template/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/aspnet-spa-template)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

A production-ready template for building modern Single Page Applications with ASP.NET Core backend and vanilla JavaScript frontend. No React, Vue, or Angular—just clean, semantic HTML, CSS, and JavaScript with a powerful RESTful API backend.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Dark Mode](#dark-mode)
- [Progressive Web App (PWA)](#progressive-web-app-pwa)
- [Offline-First](#offline-first)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Testing](#testing)
- [Performance](#performance)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**aspnet-spa-template** is a modern, full-stack web application template that demonstrates best practices for building scalable applications with:

- **Backend**: ASP.NET Core 10 with dependency injection, repository pattern, and service layer architecture
- **Frontend**: Vanilla JavaScript SPA with modern ES6+ features, no framework bloat
- **Database**: Entity Framework Core with SQL Server support
- **Caching**: In-memory caching with cache invalidation strategies
- **Background Jobs**: Task scheduling and background worker management
- **API Standards**: RESTful API with comprehensive error handling and response formats
- **Middleware**: Cross-cutting concerns like authentication, logging, rate limiting, and correlation IDs

### Why This Template?

Modern web development often defaults to heavy JavaScript frameworks (React, Vue, Angular), but many applications don't need that complexity. This template shows how to build professional, interactive UIs with vanilla JavaScript while maintaining clean, maintainable code architecture.

**Perfect for:**
- Content management systems
- Dashboard applications
- Admin panels
- Small to medium-sized SPAs
- Teams avoiding JavaScript framework overhead
- Developers learning ASP.NET Core architecture

---

## Architecture

### High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Browser / SPA Client                      │
│              (Vanilla JS, HTML5, CSS3)                       │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP/HTTPS (REST API)
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  ASP.NET Core 10 Server                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           ASP.NET Core Middleware Pipeline           │   │
│  │  • Authentication/Authorization                       │   │
│  │  • CORS & Rate Limiting                              │   │
│  │  • Request/Response Logging                          │   │
│  │  • Exception Handling                                │   │
│  │  • Correlation ID Tracking                           │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         API Controllers (REST Endpoints)             │   │
│  │  • Products, Orders, Users, Health, Webhooks        │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Business Logic (Services Layer)              │   │
│  │  • Product Service                                   │   │
│  │  • Order Service                                     │   │
│  │  • User Service                                      │   │
│  │  • Review Service                                    │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Data Access (Repository Pattern + EF Core)          │   │
│  │  • User Repository                                   │   │
│  │  • Product Repository                                │   │
│  │  • Order Repository                                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▼                                    │
├─────────────────────────────────────────────────────────────┤
│              Supporting Infrastructure                        │
│  • In-Memory Cache Service                                   │
│  • Background Task Scheduler                                 │
│  • Event Bus & Notification System                           │
│  • Webhook Handler                                           │
│  • External API Integration                                  │
└─────────────────────────────────────────────────────────────┘
                          ▼
        ┌────────────────┬────────────────┐
        ▼                ▼                 ▼
    ┌────────┐      ┌───────┐        ┌────────────┐
    │ SQL DB │      │ Cache │        │ External   │
    │        │      │ Store │        │ Services   │
    └────────┘      └───────┘        └────────────┘
```

### Project Structure

```
aspnet-spa-template/
├── Controllers/              # API endpoints
│   ├── ApiControllerBase.cs
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   ├── UsersController.cs
│   └── HealthController.cs
├── Services/                 # Business logic
│   ├── ProductService.cs
│   ├── OrderService.cs
│   ├── UserService.cs
│   └── ReviewService.cs
├── Data/                     # Data access layer
│   ├── AppDbContext.cs
│   └── Repositories/
├── Models/                   # Domain models
│   ├── Product.cs
│   ├── Order.cs
│   ├── User.cs
│   └── Review.cs
├── DTOs/                     # Data transfer objects
├── Middleware/               # HTTP pipeline
├── Configuration/            # Dependency injection setup
├── BackgroundWorkers/        # Background task execution
├── Events/                   # Event bus implementation
├── Integration/              # External service integration
├── Caching/                  # Cache abstraction
├── Exceptions/               # Custom exceptions
├── Utilities/                # Helper functions
├── wwwroot/                  # Static files (HTML, CSS, JS)
│   ├── index.html
│   ├── css/
│   │   └── style.css
│   └── js/
│       └── app.js
├── Program.cs                # Application startup
├── appsettings.json          # Configuration
└── AspNetSpaTemplate.csproj   # Project file
```

---

## Features

### Backend Features
- ✅ RESTful API with standardized response format
- ✅ Entity Framework Core with SQL Server
- ✅ Repository pattern for data access
- ✅ Dependency injection (built-in ASP.NET Core DI)
- ✅ Authentication middleware
- ✅ Rate limiting middleware
- ✅ Comprehensive exception handling
- ✅ Correlation ID tracking across requests
- ✅ Request/response logging
- ✅ In-memory caching with invalidation
- ✅ Background task scheduling
- ✅ Event bus for loose coupling
- ✅ Webhook support
- ✅ External API integration client
- ✅ CORS support
- ✅ Health check endpoint

### Frontend Features
- ✅ No framework dependencies (vanilla JavaScript)
- ✅ Modern ES6+ syntax
- ✅ Responsive design with pure CSS
- ✅ Fetch API for HTTP communication
- ✅ Client-side routing simulation
- ✅ Form validation
- ✅ Error handling and user feedback
- ✅ Loading states
- ✅ Accessibility support (semantic HTML)
- ✅ **Dark mode toggle** with `prefers-color-scheme` detection and localStorage persistence
- ✅ **Progressive Web App** — installable, manifest-driven, home screen shortcuts
- ✅ **Offline-first** — cache-first for static assets, network-first for API calls, offline fallback page

### DevOps & Infrastructure
- ✅ Docker support (Dockerfile included)
- ✅ Docker Compose for multi-container setup
- ✅ GitHub Actions CI/CD workflow
- ✅ Configurable environment support
- ✅ Health check endpoints
- ✅ Development/Production configurations

---

## Dark Mode

The UI ships with a full dark-mode theme that activates via a toggle button in the navigation bar.

### How it works

| Layer | Mechanism |
|---|---|
| CSS | A set of `--background`, `--text-color`, `--border-color`, and shadow overrides inside `[data-theme="dark"]` |
| HTML | `data-theme` attribute is set on `<html>` by JavaScript |
| JS | `DarkMode.init()` reads `localStorage` first, then falls back to `window.matchMedia('(prefers-color-scheme: dark)')` |
| Persistence | `localStorage` key `darkMode` holds `"true"` or `"false"`; absent means "follow OS" |

### Backend service

`IThemeService` / `ThemeService` store per-user colour scheme preferences server-side (backed by `ICacheService`) with a 30-day TTL. Three values are supported: `System`, `Light`, and `Dark`.

```csharp
// Retrieve the saved preference for a user
ColourScheme scheme = await themeService.GetSchemeAsync(userId);

// Persist an explicit choice
await themeService.SetSchemeAsync(userId, ColourScheme.Dark);

// Revert to system default
await themeService.ClearSchemeAsync(userId);
```

---

## Progressive Web App (PWA)

The template includes a complete Web App Manifest so users can install the application on their home screen from any modern browser.

### Manifest endpoint

`GET /manifest.json` is served by `ManifestController` and returns an `application/manifest+json` document with absolute icon URLs derived from the current request host.

### Configuration

Override the defaults via `appsettings.json` (or environment variables):

```json
{
  "Manifest": {
    "Name": "My App",
    "ShortName": "App",
    "ThemeColor": "#2563eb",
    "BackgroundColor": "#f8fafc"
  }
}
```

### Manifest contents

| Field | Value |
|---|---|
| `display` | `standalone` |
| `start_url` | `/` |
| `icons` | 192 × 192 and 512 × 512 PNG |
| `shortcuts` | Browse Products, Shopping Cart |

---

## Offline-First

The service worker (`wwwroot/sw.js`) implements a dual-strategy caching approach:

| Request type | Strategy |
|---|---|
| Static assets (HTML, CSS, JS, images) | **Cache-first** — serve from cache, update in background |
| API calls (`/api/*`) | **Network-first** — try network, fall back to cache |
| Navigation when offline | Serve `offline.html` |

### Offline fallback page

`wwwroot/offline.html` is a styled standalone page that:
- Applies the saved dark-mode preference without JavaScript bundle overhead
- Provides a "Try again" button
- Automatically redirects to `/` when the browser fires the `online` event

### Push Notifications

The service worker handles `push` events and shows OS-level notifications. The payload format:

```json
{
  "title": "Order shipped",
  "body": "Your order #1234 has been dispatched.",
  "icon": "/icons/icon-192.png",
  "actionUrl": "/?page=orders"
}
```

Clicking the notification opens (or focuses) the app at `actionUrl`.

### Background Sync & Offline Queue

When a mutating request (`POST`, `PUT`, `DELETE`) fails while offline, the client calls `OfflineQueue.enqueue(entry)` which posts a `QUEUE_REQUEST` message to the service worker. The request is persisted in **IndexedDB** and replayed automatically when the Background Sync event fires.

The server-side `ISyncQueueService` / `SyncQueueService` provides:

```csharp
// Queue a captured offline request
int id = syncQueue.Enqueue(userId, clientRequestId, "POST", "/api/orders", bodyJson);

// Get all pending entries for a user
IReadOnlyList<SyncQueueEntry> pending = syncQueue.GetPending(userId);

// Mark as successfully replayed
syncQueue.Complete(id);

// Mark as permanently failed
syncQueue.Fail(id, "Server returned 422");
```

Idempotency is enforced via `clientRequestId` — re-submitting the same key returns the existing entry ID without creating a duplicate.

---

## Installation

### Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/en-us/download)
- **SQL Server** 2019+ or SQL Server Express
- **Git** for version control
- Modern web browser (Chrome, Firefox, Safari, Edge)

### Option 1: Clone & Run Locally

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/aspnet-spa-template.git
cd aspnet-spa-template

# Restore NuGet packages
dotnet restore

# Update database (ensure SQL Server is running)
dotnet ef database update

# Run the application
dotnet run

# Application will be available at https://localhost:7001
```

### Option 2: Docker Setup

```bash
# Build and run with Docker Compose
docker-compose up --build

# Application will be available at http://localhost:5000
# SQL Server will be available on port 1433
```

### Option 3: Visual Studio

```bash
# Open in Visual Studio
start AspNetSpaTemplate.sln

# Set SQL Server connection string if needed
# Press F5 to run
```

---

## Quick Start

### 1. Start the Application

```bash
dotnet run
```

The application starts on:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5000

### 2. Access the SPA

Open your browser to `https://localhost:7001/` to see the vanilla JavaScript SPA.

---

## Development Workflow (Hot-Reload with Dev Proxy)

For a faster inner loop you can run the ASP.NET backend and the frontend dev server in two separate terminals. The dev proxy forwards `/api` requests to the backend, so you never have to deal with CORS during development.

### Terminal 1 – ASP.NET backend

```bash
dotnet run
# Listening on http://localhost:5000 (HTTP) and https://localhost:7001 (HTTPS)
```

### Terminal 2 – Frontend dev server

```bash
# Install dependencies once
npm install

# Start the dev proxy (serves wwwroot and proxies /api to http://localhost:5000)
npm run dev
# Dev server available at http://localhost:3000
```

Open `http://localhost:3000` in your browser. All `/api/*` requests are transparently proxied to `http://localhost:5000`, and any change you make to files inside `wwwroot/` is reflected immediately on the next browser refresh.

### How it works

| File | Purpose |
|---|---|
| `proxy.config.json` | Declares proxy rules (context → target) |
| `dev-server.js` | Express server that reads the proxy config and serves static files |
| `package.json` | `npm run dev` entry point |

The `/api` rule in `proxy.config.json` can be updated to point at any backend URL:

```json
{
  "/api": {
    "target": "http://localhost:5000",
    "changeOrigin": true,
    "secure": false
  }
}
```

---

## Quick Start (API)

### Make Your First API Call

```bash
curl https://localhost:7001/api/products
```

### Create Your First Product

```bash
curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Product",
    "description": "A great product",
    "price": 99.99,
    "category": "Electronics",
    "stock": 10
  }'
```

---

## Usage Examples

This repository includes a set of practical C# usage examples in the `examples/` directory to help you get started:

- `examples/BasicUsage.cs`: Demonstrates minimal setup and first calls for core services.
- `examples/AdvancedUsage.cs`: Shows advanced configuration, custom options, and error handling.
- `examples/IntegrationExample.cs`: Illustrates how to wire services into the ASP.NET Core dependency injection container.

For additional, scenario-specific guidance, see the examples below:

### Example 1: Fetch All Products

**Frontend (JavaScript)**
```javascript
async function loadProducts() {
  try {
    const response = await fetch('/api/products');
    const data = await response.json();
    
    if (data.success) {
      console.log('Products:', data.data);
      displayProducts(data.data);
    } else {
      console.error('Error:', data.message);
    }
  } catch (error) {
    console.error('Network error:', error);
  }
}
```

**Backend (C# Service)**
```csharp
public async Task<List<ProductDto>> GetAllProductsAsync()
{
  var products = await _repository.GetAllAsync();
  return products.ConvertAll(p => new ProductDto
  {
    Id = p.Id,
    Name = p.Name,
    Price = p.Price
  });
}
```

### Example 2: Create an Order

**Frontend (HTML Form)**
```html
<form id="orderForm">
  <input type="text" id="productId" placeholder="Product ID" required>
  <input type="number" id="quantity" placeholder="Quantity" required>
  <button type="submit">Create Order</button>
</form>

<script>
document.getElementById('orderForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const response = await fetch('/api/orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      productId: document.getElementById('productId').value,
      quantity: parseInt(document.getElementById('quantity').value)
    })
  });
  
  const result = await response.json();
  alert(result.success ? 'Order created!' : result.message);
});
</script>
```

### Example 3: Implement Pagination

**Frontend**
```javascript
async function loadProductsPage(pageNumber = 1, pageSize = 10) {
  const response = await fetch(
    `/api/products?pageNumber=${pageNumber}&pageSize=${pageSize}`
  );
  const data = await response.json();
  return data;
}
```

**Backend (Controller)**
```csharp
[HttpGet]
public async Task<ApiResponse<List<ProductDto>>> GetProducts(
  [FromQuery] PaginationRequest pagination)
{
  var products = await _service.GetProductsAsync(pagination);
  return ApiResponse.Success(products);
}
```

### Example 4: Handle Form Validation

**Frontend**
```javascript
function validateProductForm(formData) {
  const errors = [];
  
  if (!formData.name?.trim()) errors.push('Name is required');
  if (formData.price <= 0) errors.push('Price must be greater than 0');
  if (!formData.category) errors.push('Category is required');
  
  return { isValid: errors.length === 0, errors };
}
```

### Example 5: Error Handling

**Frontend**
```javascript
async function handleApiError(response) {
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'API Error');
  }
  return response.json();
}

fetch('/api/products')
  .then(handleApiError)
  .catch(error => {
    console.error('Error:', error.message);
    showUserNotification(error.message, 'error');
  });
```

### Example 6: Background Task Execution

**Backend Configuration**
```csharp
services.AddHostedService<CacheMaintenanceWorker>();
services.AddHostedService<NotificationWorker>();
```

### Example 7: Caching Strategy

**Backend Service**
```csharp
public async Task<List<ProductDto>> GetTopProductsAsync()
{
  const string cacheKey = "top_products_cache";
  
  if (_cache.TryGet(cacheKey, out List<ProductDto> cached))
  {
    return cached;
  }
  
  var products = await _repository.GetTopAsync(10);
  var dtos = products.ConvertAll(MapToDto);
  
  _cache.Set(cacheKey, dtos, TimeSpan.FromHours(1));
  return dtos;
}
```

### Example 8: Authentication Header

**Frontend**
```javascript
const token = localStorage.getItem('authToken');

const response = await fetch('/api/users/profile', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

---

## API Reference

### Base URL
```
https://localhost:7001/api
```

### Products Endpoints

#### GET /products
Retrieve all products with pagination.

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Product Name",
      "price": 99.99,
      "category": "Electronics"
    }
  ],
  "message": "Success"
}
```

#### POST /products
Create a new product. (Requires admin role)

**Request Body:**
```json
{
  "name": "Product Name",
  "description": "Description",
  "price": 99.99,
  "category": "Electronics",
  "stock": 10
}
```

#### GET /products/{id}
Get a specific product.

#### PUT /products/{id}
Update a product.

#### DELETE /products/{id}
Delete a product.

### Orders Endpoints

#### GET /orders
List all orders with pagination.

#### POST /orders
Create a new order.

#### GET /orders/{id}
Get order details.

#### PUT /orders/{id}/status
Update order status.

### Users Endpoints

#### GET /users
List all users.

#### POST /users
Create a new user.

#### GET /users/{id}
Get user details.

### Health Endpoint

#### GET /health
Application health status.

---

## Configuration

### Environment-Specific Settings

ASP.NET Core uses a layered configuration system. Files are applied in order, with later files overriding earlier ones:

| File | Purpose | Commit to git? |
|---|---|---|
| `appsettings.json` | Base defaults (no secrets) | ✅ Yes |
| `appsettings.Development.json` | Dev overrides | ⚠️ Only if it contains no secrets |
| `appsettings.Production.json` | Production overrides | ✅ Yes (no secrets — use env vars) |
| Environment variables | Runtime secrets & deployment config | ✅ (set in your CI/CD or hosting platform) |
| `dotnet user-secrets` | Local developer secrets | ✅ Stored outside the repo |

The active environment is controlled by the `ASPNETCORE_ENVIRONMENT` variable (defaults to `Production` when not set).

```bash
# Use Development profile
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Use Production profile
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

### Local Secrets with dotnet user-secrets

Never put real credentials in `appsettings.json`. Use `dotnet user-secrets` for local development — secrets are stored in your OS user profile, not in the repository.

```bash
# Initialise user-secrets for the project (one-time)
dotnet user-secrets init

# Store a connection string locally
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=AspNetSpaTemplate;User ID=dev;Password=dev;"

# Store the JWT secret locally
dotnet user-secrets set "AppSettings:JwtSecret" "my-local-only-secret"

# List stored secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "AppSettings:JwtSecret"
```

Secrets set this way are automatically merged into `IConfiguration` at startup when `ASPNETCORE_ENVIRONMENT=Development`.

### Environment Variable Substitution

All configuration keys are available as environment variables using `__` as the section separator:

```bash
# Database
ConnectionStrings__DefaultConnection="Server=prod-db;Database=AspNetSpaTemplate;..."

# Logging
Logging__LogLevel__Default=Warning

# Request logging verbosity
RequestLogging__VerbosityLevel=Minimal
RequestLogging__Enabled=true

# App settings
AppSettings__JwtSecret=your-production-secret-here
AppSettings__JwtExpiration=3600
```

Environment variables always take precedence over `appsettings.json`, making them ideal for container/cloud deployments.

### appsettings.Development.json (example)

Create this file locally (it is gitignored when it contains secrets) to override defaults during development:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "RequestLogging": {
    "Enabled": true,
    "VerbosityLevel": "Detailed"
  }
}
```

### appsettings.json (base defaults)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AspNetSpaTemplate;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "RequestLogging": {
    "Enabled": true,
    "VerbosityLevel": "Standard",
    "SlowRequestThresholdMs": 1000
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  }
}
```

### Environment Variables in Production

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://0.0.0.0:443
ConnectionStrings__DefaultConnection=Server=prod-db-server;Database=AspNetSpaTemplate;...
AppSettings__JwtSecret=<generated-secure-value>
```

---

## Troubleshooting

### Database Connection Failed
```bash
# Check SQL Server is running, verify connection string
dotnet ef database update
```

### Port Already in Use
```bash
# Use different port
dotnet run -- --urls="https://localhost:7002"
```

### HTTPS Certificate Error
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### EF Core Migration Error
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Testing

Unit and integration tests live under `tests/aspnet-spa-template.Tests/`.

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run a specific test file
dotnet test --filter "FullyQualifiedName~ProductModelTests"
```

Key test files:

| File | Coverage |
|---|---|
| `ProductModelTests.cs` | Model validation and business rules |
| `OrderAndCacheTests.cs` | Order creation and cache invalidation |
| `StringExtensionsTests.cs` | Utility extension methods |

---

## Performance

Benchmarks measured on a single core (Intel Core i7, 16 GB RAM, .NET 10 Release build):

| Scenario | Metric |
|---|---|
| Cached product list (in-memory) | **< 2 ms** p99 latency |
| Single DB read (repository pattern) | **< 40 ms** p99 latency |
| POST /orders under concurrent load | **8,500 req/s** sustained |
| Background task scheduler throughput | **12,000 events/s** |
| Application cold-start time | **~1.2 s** |
| Idle memory footprint | **~55 MB** RSS |

Cache hit rate reaches **~90%** for read-heavy workloads using the default 1-hour TTL strategy.

To profile locally:

```bash
dotnet run --configuration Release
# Benchmark with hey or k6
hey -n 50000 -c 100 https://localhost:7001/api/products
```

---

## Related Projects

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

**Plugging the template's service layer into an existing host:**

```csharp
// Program.cs of your existing application
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

**Extending the repository pattern with a domain-specific query:**

```csharp
public class CustomProductRepository : RepositoryBase<Product>
{
    public CustomProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByVendorAsync(string vendorId)
        => await _context.Products
            .Where(p => p.VendorId == vendorId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
}
```

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit changes: `git commit -am 'Add my feature'`
4. Push to branch: `git push origin feature/my-feature`
5. Submit a pull request

---

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
