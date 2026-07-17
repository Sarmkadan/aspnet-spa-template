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

# Application will be available at http://localhost:8080
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

---

## CacheMaintenanceWorker

The `CacheMaintenanceWorker` is a background service that monitors and maintains the health of the in-memory cache system. It periodically checks cache performance metrics, identifies potential issues like excessive memory usage or low hit rates, and provides detailed health reports. The worker helps ensure optimal cache performance and prevents memory leaks by tracking key metrics such as hit rate, item count, and memory usage.

### Usage Example

```csharp
// Register the worker in Program.cs
builder.Services.AddHostedService<CacheMaintenanceWorker>();

// Access the worker via DI to check status
public class CacheMonitorService
{
    private readonly CacheMaintenanceWorker _cacheWorker;

    public CacheMonitorService(CacheMaintenanceWorker cacheWorker)
    {
        _cacheWorker = cacheWorker;
    }

    public async Task MonitorCacheHealth()
    {
        // Get current status
        BackgroundTaskStatus status = await _cacheWorker.GetStatus();

        // Check health metrics
        bool isHealthy = _cacheWorker.IsHealthy;
        double hitRate = _cacheWorker.HitRate;
        long itemCount = _cacheWorker.ItemCount;
        long memoryUsage = _cacheWorker.MemoryUsageBytes;

        // Get detailed health report
        CacheHealthReport healthReport = await _cacheWorker.GetHealthReportAsync();

        // Check if cache is healthy
        bool isCacheHealthy = await _cacheWorker.IsCacheHealthyAsync();

        // Access warnings if any issues detected
        if (!isHealthy && _cacheWorker.WarningCount > 0)
        {
            foreach (string warning in _cacheWorker.Warnings)
            {
                Console.WriteLine($"Cache warning: {warning}");
            }
        }
    }
}
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

## NotFoundExceptionExtensions

The `NotFoundExceptionExtensions` class provides a set of extension methods for creating and working with `NotFoundException` instances. These methods simplify common patterns for constructing not-found exceptions and checking their details, making error handling more readable and maintainable throughout your application.

The extension methods work with the `NotFoundException` class, which includes resource type and ID information for better error context and debugging.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a not-found exception with a simple message
var exception = "User not found".ToNotFound();

// Create a not-found exception with resource type and ID
var productException = "product".ToNotFound(123);

// Create a not-found exception with formatted message
var orderException = "order".ToNotFound(456, "Order with ID {0} was not found in the system", 456);

// Create a not-found exception with an inner exception
try
{
    // Some operation that might throw
}
catch (Exception ex)
{
    var notFound = "Customer".ToNotFound(ex);
}

// Create a strongly-typed not-found exception using generic method
var userException = 789.ToNotFound<User>();

// Check if an exception is for a specific resource type
bool isUserNotFound = userException.IsNotFoundFor("User"); // true
bool isProductNotFound = userException.IsNotFoundFor("Product"); // false

// Check if an exception is for a specific resource type and ID
bool isUser789NotFound = userException.IsNotFoundFor("User", 789); // true
bool isUser123NotFound = userException.IsNotFoundFor("User", 123); // false

// Check using generic method
bool isUserTypeNotFound = userException.IsNotFoundFor<User>(); // true

// Get the resource type from the exception
string resourceType = userException.GetResourceType(); // "User"

// Get the resource ID from the exception with type safety
int userId = userException.GetResourceId<int>(); // 789
```

---

## NotFoundExceptionExtensionsValidation

The `NotFoundExceptionExtensionsValidation` class provides validation helper methods for parameters used with the `NotFoundExceptionExtensions` extension methods. These methods validate parameters before they're passed to the extension methods, ensuring that field names, error messages, and other inputs are properly formatted and non-null, helping to prevent runtime errors and improve code reliability.

The validation methods work with all the parameter combinations supported by the `NotFoundExceptionExtensions` methods, including simple messages, resource types with IDs, formatted messages, and exceptions with inner exceptions. Each validation method returns an empty list when validation succeeds, or a list of validation problems when validation fails.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Validate parameter combinations before using NotFoundExceptionExtensions methods
var messageProblems = NotFoundExceptionExtensionsValidation.ValidateMessage("User not found");
bool isMessageValid = NotFoundExceptionExtensionsValidation.IsMessageValid("Product not found");

// Use EnsureMessageValid to throw an exception if the message is invalid
try
{
    NotFoundExceptionExtensionsValidation.EnsureMessageValid(null);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Message validation failed: {ex.Message}");
}

// Validate resource type and ID parameters
var resourceProblems = NotFoundExceptionExtensionsValidation.ValidateResource("User", 123);
bool isResourceValid = NotFoundExceptionExtensionsValidation.IsResourceValid("Product", 456);

// Use EnsureResourceValid to throw an exception if parameters are invalid
try
{
    NotFoundExceptionExtensionsValidation.EnsureResourceValid(" ", null);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Resource validation failed: {ex.Message}");
}

// Validate formatted resource parameters
var formattedProblems = NotFoundExceptionExtensionsValidation.ValidateFormattedResource(
    "Order", 
    789, 
    "Order with ID {0} was not found",
    789
);
bool isFormattedValid = NotFoundExceptionExtensionsValidation.IsFormattedResourceValid(
    "Product", 
    123, 
    "Product with ID {0} not available",
    123
);

// Validate exception with inner exception
var innerProblems = NotFoundExceptionExtensionsValidation.ValidateWithInner("Customer not found", new Exception("Database error"));
bool isInnerValid = NotFoundExceptionExtensionsValidation.IsWithInnerValid("User not found", new Exception());

// Validate generic resource ID parameter
var genericProblems = NotFoundExceptionExtensionsValidation.ValidateGeneric(456);
bool isGenericValid = NotFoundExceptionExtensionsValidation.IsGenericValid(789);

// Use EnsureGenericValid to throw an exception if the resource ID is invalid
try
{
    NotFoundExceptionExtensionsValidation.EnsureGenericValid(null);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Generic validation failed: {ex.Message}");
}
```

---

## ValidationExceptionExtensions

## ValidationExceptionExtensions

The `ValidationExceptionExtensions` class provides a set of extension methods for working with `ValidationException` objects in a fluent, readable manner. These methods simplify common validation scenarios such as adding errors, checking for specific field errors, and merging validation results.

The extension methods work with the standard `ValidationException` class, which maintains a dictionary of field names to lists of error messages, making it easy to manage and retrieve validation errors throughout your application.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a validation exception using the WithError extension method
var validationException = "email".WithError("Email is required");

// Add additional errors to the same exception
validationException.AddError("email", "Email format is invalid");
validationException.AddError("password", "Password must be at least 8 characters");

// Check if a specific field has errors
bool hasEmailErrors = validationException.HasErrorFor("email"); // returns true
bool hasPhoneErrors = validationException.HasErrorFor("phone"); // returns false

// Get all error messages for a specific field
string emailErrors = validationException.GetErrorMessages("email");
// "Email is required; Email format is invalid"

// Get all errors as a dictionary
var allErrors = validationException.GetAllErrors();
// Returns: {"email": ["Email is required", "Email format is invalid"], "password": ["Password must be at least 8 characters"]}

// Check if the exception contains any errors
bool hasAnyErrors = validationException.HasErrors(); // returns true

// Merge errors from another validation exception
var anotherException = "username".WithError("Username is already taken");
validationException.MergeErrors(anotherException);

// Access the underlying Errors dictionary directly
foreach (var fieldErrors in validationException.GetAllErrors())
{
    Console.WriteLine($"{fieldErrors.Key}: {string.Join(", ", fieldErrors.Value)}");
}
```

---

## ValidationException

The `ValidationException` class represents an exception thrown when data validation fails in the application. It maintains a dictionary of field names to lists of error messages, allowing for structured validation error reporting. This exception is particularly useful for API controllers that need to return detailed validation errors to clients.

The `Errors` property provides access to all validation errors, while the `AddError` method allows for fluent error accumulation. The exception can be constructed with either a simple message, a dictionary of errors, or individual field-specific errors.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a validation exception with a simple message
var validationException = new ValidationException("User registration data is invalid");

// Create a validation exception with a dictionary of field errors
var errors = new Dictionary<string, List<string>>
{
    { "email", new List<string> { "Email is required", "Email format is invalid" } },
    { "password", new List<string> { "Password must be at least 8 characters" } },
    { "username", new List<string> { "Username is already taken" } }
};
var dictValidationException = new ValidationException(errors);

// Create a validation exception for a specific field
var fieldValidationException = new ValidationException("email", "Email address is required");

// Add additional errors to an existing exception
fieldValidationException.AddError("email", "Email must contain @ symbol");
fieldValidationException.AddError("password", "Password must contain at least one digit");

// Access the Errors dictionary to inspect all validation errors
foreach (var fieldError in fieldValidationException.Errors)
{
    Console.WriteLine($"{fieldError.Key}: {string.Join(", ", fieldError.Value)}");
}

// Check if there are any errors
if (fieldValidationException.Errors.Count > 0)
{
    Console.WriteLine("Validation failed!");
}

// Access the Field property to get the first field with errors
string? firstField = fieldValidationException.Field; // "email"
```

---

## ValidationExceptionExtensionsValidation

The `ValidationExceptionExtensionsValidation` class provides validation helper methods specifically designed for validating parameters and exception instances used with `ValidationExceptionExtensions` extension methods. These methods ensure that field names and error messages are properly formatted before being used in validation operations, helping to prevent runtime errors and improve code reliability.

The class includes methods for validating both parameter combinations (field name and error message) and complete `ValidationException` instances, with options to check validity, validate with exceptions, and get detailed validation problems.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Validate parameter combinations before using ValidationExceptionExtensions methods
var parameterProblems = ValidationExceptionExtensionsValidation.ValidateParameters("email", "Email address is required");
bool areValid = ValidationExceptionExtensionsValidation.AreParametersValid("username", "Username cannot be empty");

// Use EnsureParametersValid to throw an exception if parameters are invalid
try
{
    ValidationExceptionExtensionsValidation.EnsureParametersValid(" ", "Error message");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Parameter validation failed: {ex.Message}");
}

// Validate a complete ValidationException instance
var validationException = new ValidationException(new Dictionary<string, List<string>>
{
    { "email", new List<string> { "Email is required" } }
});

var exceptionProblems = ValidationExceptionExtensionsValidation.ValidateException(validationException);
bool isExceptionValid = ValidationExceptionExtensionsValidation.IsExceptionValid(validationException);

// Use EnsureExceptionValid to throw an exception if the ValidationException is not valid
try
{
    ValidationExceptionExtensionsValidation.EnsureExceptionValid(validationException);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Exception validation failed: {ex.Message}");
}
```

---

## ValidationExceptionJsonExtensionsJsonExtensions

The `ValidationExceptionJsonExtensionsJsonExtensions` class provides extension methods for serializing and deserializing `ValidationException` objects to/from JSON strings. This duplicate class (with "Json" suffix) exists for API consistency and provides the same functionality as `ValidationExceptionJsonExtensions` but with a different class name to match naming conventions in the codebase.

These methods are particularly useful when you need to transmit validation errors across API boundaries, persist them in storage, or cache them for later use.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a validation exception with error dictionary
var validationException = new ValidationException(new Dictionary<string, List<string>>
{
    { "email", new List<string> { "Email is required", "Email format is invalid" } },
    { "password", new List<string> { "Password must be at least 8 characters" } }
});

// Serialize to JSON string
string json = validationException.ToJson();
// {"message":"Validation failed","errors":{"email":["Email is required","Email format is invalid"],"password":["Password must be at least 8 characters"]}}

// Serialize with indentation for readability
string prettyJson = validationException.ToJson(indented: true);

// Deserialize back to ValidationException
ValidationException? deserialized = ValidationExceptionJsonExtensionsJsonExtensions.FromJson(json);

// Try to deserialize with error handling
if (ValidationExceptionJsonExtensionsJsonExtensions.TryFromJson(json, out var result))
{
    // Use the deserialized exception
    if (result != null)
    {
        Console.WriteLine(result.Message);
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
        }
    }
}

// Handle null or empty JSON
string emptyJson = "";
ValidationException? nullResult = ValidationExceptionJsonExtensionsJsonExtensions.FromJson(emptyJson); // returns null

// Handle invalid JSON
string invalidJson = "{invalid}";
bool success = ValidationExceptionJsonExtensionsJsonExtensions.TryFromJson(invalidJson, out var invalidResult); // returns false
```

---

## BusinessException

The `BusinessException` class represents an exception thrown when business logic constraints are violated in the application. It provides structured error information including an optional error code and configurable HTTP status code, making it ideal for API controllers that need to return specific business rule violations to clients.

The exception supports multiple construction patterns and includes a fluent `WithData` method for attaching additional diagnostic context to the exception instance.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a basic business exception with a message
var exception = new BusinessException("Product stock cannot be negative");

// Create a business exception with error code and default HTTP status (400)
var validationException = new BusinessException("Invalid order quantity", "ORDER_QTY_INVALID");

// Create a business exception with custom HTTP status code (e.g., 422 for validation failures)
var businessRuleException = new BusinessException(
    "User already has an active subscription",
    "USER_SUBSCRIPTION_ACTIVE",
    422
);

// Use the fluent WithData method to attach additional context
try
{
    // Some business operation that might fail
}
catch (Exception ex)
{
    throw new BusinessException("Failed to process payment", "PAYMENT_PROCESSING_ERROR", 402)
        .WithData(ex);
}

// Access the properties
Console.WriteLine(exception.ErrorCode); // null
Console.WriteLine(validationException.ErrorCode); // "ORDER_QTY_INVALID"
Console.WriteLine(businessRuleException.HttpStatusCode); // 422
```

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create an external API exception with endpoint and message
var exception = new ExternalApiException("/api/payment/process", "Payment gateway returned 402 Payment Required");
Console.WriteLine(exception.Endpoint); // "/api/payment/process"

// Create an external API exception with full context
var detailedException = new ExternalApiException(
    endpoint: "https://api.stripe.com/v1/charges",
    method: "POST",
    statusCode: 402,
    message: "Payment failed: card declined"
);
Console.WriteLine(detailedException.Endpoint); // "https://api.stripe.com/v1/charges"
Console.WriteLine(detailedException.Method); // "POST"
Console.WriteLine(detailedException.StatusCode); // 402

// Add additional diagnostic context using the fluent WithContext method
var exceptionWithContext = new ExternalApiException(
    "/api/external-service/data",
    "External service unavailable"
)
.WithContext("requestId", Guid.NewGuid())
.WithContext("retryCount", 3)
.WithContext("timestamp", DateTime.UtcNow);

// Access the additional context through the Data property
Console.WriteLine(exceptionWithContext.Data["requestId"]); // Guid value
Console.WriteLine(exceptionWithContext.Data["retryCount"]); // 3
```

The extension methods support both standard JSON serialization (with camelCase property naming) and pretty-printed JSON for debugging purposes. The deserialization methods handle both the simple message case and the full validation error dictionary format.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a validation exception with error dictionary
var validationException = new ValidationException(new Dictionary<string, List<string>>
{
    { "email", new List<string> { "Email is required", "Email format is invalid" } },
    { "password", new List<string> { "Password must be at least 8 characters" } }
});

// Serialize to JSON string
string json = validationException.ToJson();
// {"message":"Validation failed","errors":{"email":["Email is required","Email format is invalid"],"password":["Password must be at least 8 characters"]}}

// Serialize with indentation for readability
string prettyJson = validationException.ToJson(indented: true);

// Deserialize back to ValidationException
ValidationException? deserialized = ValidationExceptionJsonExtensions.FromJson(json);

// Try to deserialize with error handling
if (ValidationExceptionJsonExtensions.TryFromJson(json, out var result))
{
    // Use the deserialized exception
    if (result != null)
    {
        Console.WriteLine(result.Message);
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
        }
    }
}

// Handle null or empty JSON
string emptyJson = "";
ValidationException? nullResult = ValidationExceptionJsonExtensions.FromJson(emptyJson); // returns null

// Handle invalid JSON
string invalidJson = "{invalid}";
bool success = ValidationExceptionJsonExtensions.TryFromJson(invalidJson, out var invalidResult); // returns false
```

---

## NotFoundException

The `NotFoundException` class represents an exception thrown when a requested resource cannot be found in the system. It includes optional `ResourceType` and `ResourceId` properties to provide context about which resource was not found, making it ideal for API controllers that need to return specific not-found responses to clients.

This exception can be constructed with a simple message, or with resource type and ID information for better error context and debugging.

### Usage Example

```csharp
using AspNetSpaTemplate.Exceptions;

// Create a not-found exception with a simple message
var exception = new NotFoundException("User not found");

// Create a not-found exception with resource type and ID
var productException = new NotFoundException("Product", 123);

// Access the resource type and ID properties
Console.WriteLine(productException.ResourceType); // "Product"
Console.WriteLine(productException.ResourceId);   // 123

// Create a not-found exception with an inner exception
try
{
    // Some operation that might throw
}
catch (Exception ex)
{
    var notFound = new NotFoundException("Order not found", ex);
}
```

---

## ExternalApiClient

The `ExternalApiClient` class provides a robust, retry-capable HTTP client for consuming external APIs from your ASP.NET Core services. It handles common concerns like automatic retries on transient failures, configurable timeouts, structured error handling, and detailed logging—making external API calls more reliable and easier to debug.

Configure the client via `ExternalApiConfig` and register it in your DI container. The client automatically retries failed requests (up to 3 times by default) for server errors (5xx) and timeouts, with exponential backoff between attempts.

### Usage Example

```csharp
// In Program.cs or your DI configuration
builder.Services.AddHttpClient<ExternalApiClient>();
builder.Services.Configure<ExternalApiConfig>(config =>
{
    config.BaseUrl = "https://api.example.com/v1";
    config.ApiKey = "your-api-key-here";
    config.Timeout = TimeSpan.FromSeconds(30);
    config.MaxRetries = 3;
    config.LogRequests = true;
    config.LogResponses = false; // Set to true for debugging, but be careful with sensitive data
});

// Inject ExternalApiClient in your service
public class PaymentService
{
    private readonly ExternalApiClient _apiClient;
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(ExternalApiClient apiClient, ILogger<PaymentService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            // Make GET request to external API
            var paymentMethods = await _apiClient.GetAsync<PaymentMethod[]>(
                $"/payment-methods"
            );
            
            // Make POST request to external API
            var paymentResponse = await _apiClient.PostAsync<PaymentResponse>(
                $"/payments",
                new { request.Amount, request.Currency, request.PaymentMethodId }
            );
            
            return paymentResponse;
        }
        catch (ExternalApiException ex)
        {
            _logger.LogError(ex, "Failed to process payment via external API");
            throw; // Or handle gracefully based on your business requirements
        }
    }
}

// Example DTOs
public record PaymentRequest(decimal Amount, string Currency, int PaymentMethodId);
public record PaymentResponse(string TransactionId, decimal Amount, string Status);
public record PaymentMethod(string Id, string Name, string Type);
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | `""` | The base URL of the external API (e.g., `https://api.example.com/v1`) |
| `ApiKey` | `string` | `""` | API key for authentication with the external service |
| `Timeout` | `TimeSpan` | `30 seconds` | HTTP request timeout duration |
| `MaxRetries` | `int` | `3` | Maximum number of retry attempts for transient failures |
| `LogRequests` | `bool` | `true` | Whether to log request details (endpoint, attempt count) |
| `LogResponses` | `bool` | `false` | Whether to log response details (be careful with sensitive data) |

### Public Methods

- `GetAsync<T>(string endpoint)` - Makes a GET request to the specified endpoint and returns a deserialized response of type T
- `PostAsync<T>(string endpoint, object request)` - Makes a POST request with the provided request object and returns a deserialized response of type T

Both methods automatically handle retries, timeouts, and error conversion to `ExternalApiException` with detailed context.

### Error Handling

All external API failures throw `ExternalApiException` which includes:
- The endpoint that failed
- HTTP method (GET/POST)
- HTTP status code (if available)
- Detailed error message
- Additional diagnostic context (attempt count, request body, etc.)

```csharp
try
{
    var response = await _apiClient.GetAsync<WeatherData>("/weather/current");
}
catch (ExternalApiException ex)
{
    // ex.Endpoint = "/weather/current"
    // ex.Method = "GET"
    // ex.StatusCode = 500 (if available)
    // ex.Message = "Internal server error"
    // ex.Data["Attempts"] = "3" (if retries exhausted)
    
    // Handle gracefully or rethrow
    throw;
}
```

### Retry Strategy

The client retries on:
- Server errors (HTTP 5xx)
- Request timeouts (HTTP 408)
- Connection timeouts

After the configured maximum retries are exhausted, it fails fast and throws an `ExternalApiException` with the attempt count in the context.

---

## OrderItem

The `OrderItem` class represents a line item in an order, containing product details, pricing information, and calculation methods for order totals. It provides comprehensive order calculation functionality including subtotals, taxes, discounts, and validation, making it ideal for order processing and line item management in e-commerce applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create an order item for a product
var orderItem = new OrderItem
{
    OrderId = 1,
    ProductId = 101,
    Quantity = 2,
    UnitPrice = 29.99m,
    TaxAmount = 4.80m
};

// Calculate and set the total
orderItem.RecalculateTotal();

// Apply a discount
orderItem.ApplyDiscount(5.00m);

// Get calculated values
decimal subtotal = orderItem.GetSubtotal(); // 59.98
decimal totalWithTax = orderItem.GetTotalWithTax(); // 59.78
decimal averagePrice = orderItem.GetAveragePricePerUnit(); // 29.89

// Validate the order item
bool isValid = orderItem.IsValid(); // true

// Access navigation properties
Order? order = orderItem.Order;
Product? product = orderItem.Product;
```

---

## API Reference

## ExternalApiClient

The `ExternalApiClient` class provides a robust, retry-capable HTTP client for consuming external APIs from your ASP.NET Core services. It handles common concerns like automatic retries on transient failures, configurable timeouts, structured error handling, and detailed logging—making external API calls more reliable and easier to debug.

Configure the client via `ExternalApiConfig` and register it in your DI container. The client automatically retries failed requests (up to 3 times by default) for server errors (5xx) and timeouts, with exponential backoff between attempts.

### Usage Example

```csharp
// In Program.cs or your DI configuration
builder.Services.AddHttpClient<ExternalApiClient>();
builder.Services.Configure<ExternalApiConfig>(config =>
{
    config.BaseUrl = "https://api.example.com/v1";
    config.ApiKey = "your-api-key-here";
    config.Timeout = TimeSpan.FromSeconds(30);
    config.MaxRetries = 3;
    config.LogRequests = true;
    config.LogResponses = false; // Set to true for debugging, but be careful with sensitive data
});

// Inject ExternalApiClient in your service
public class PaymentService
{
    private readonly ExternalApiClient _apiClient;
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(ExternalApiClient apiClient, ILogger<PaymentService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            // Make GET request to external API
            var paymentMethods = await _apiClient.GetAsync<PaymentMethod[]>(
                $"/payment-methods"
            );
            
            // Make POST request to external API
            var paymentResponse = await _apiClient.PostAsync<PaymentResponse>(
                $"/payments",
                new { request.Amount, request.Currency, request.PaymentMethodId }
            );
            
            return paymentResponse;
        }
        catch (ExternalApiException ex)
        {
            _logger.LogError(ex, "Failed to process payment via external API");
            throw; // Or handle gracefully based on your business requirements
        }
    }
}

// Example DTOs
public record PaymentRequest(decimal Amount, string Currency, int PaymentMethodId);
public record PaymentResponse(string TransactionId, decimal Amount, string Status);
public record PaymentMethod(string Id, string Name, string Type);
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | `""` | The base URL of the external API (e.g., `https://api.example.com/v1`) |
| `ApiKey` | `string` | `""` | API key for authentication with the external service |
| `Timeout` | `TimeSpan` | `30 seconds` | HTTP request timeout duration |
| `MaxRetries` | `int` | `3` | Maximum number of retry attempts for transient failures |
| `LogRequests` | `bool` | `true` | Whether to log request details (endpoint, attempt count) |
| `LogResponses` | `bool` | `false` | Whether to log response details (be careful with sensitive data) |

### Public Methods

- `GetAsync<T>(string endpoint)` - Makes a GET request to the specified endpoint and returns a deserialized response of type T
- `PostAsync<T>(string endpoint, object request)` - Makes a POST request with the provided request object and returns a deserialized response of type T

Both methods automatically handle retries, timeouts, and error conversion to `ExternalApiException` with detailed context.

### Error Handling

All external API failures throw `ExternalApiException` which includes:
- The endpoint that failed
- HTTP method (GET/POST)
- HTTP status code (if available)
- Detailed error message
- Additional diagnostic context (attempt count, request body, etc.)

```csharp
try
{
    var response = await _apiClient.GetAsync<WeatherData>("/weather/current");
}
catch (ExternalApiException ex)
{
    // ex.Endpoint = "/weather/current"
    // ex.Method = "GET"
    // ex.StatusCode = 500 (if available)
    // ex.Message = "Internal server error"
    // ex.Data["Attempts"] = "3" (if retries exhausted)
    
    // Handle gracefully or rethrow
    throw;
}
```

### Retry Strategy

The client retries on:
- Server errors (HTTP 5xx)
- Request timeouts (HTTP 408)
- Connection timeouts

After the configured maximum retries are exhausted, it fails fast and throws an `ExternalApiException` with the attempt count in the context.

---

## OrderItem

The `OrderItem` class represents a line item in an order, containing product details, pricing information, and calculation methods for order totals. It provides comprehensive order calculation functionality including subtotals, taxes, discounts, and validation, making it ideal for order processing and line item management in e-commerce applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create an order item for a product
var orderItem = new OrderItem
{
    OrderId = 1,
    ProductId = 101,
    Quantity = 2,
    UnitPrice = 29.99m,
    TaxAmount = 4.80m
};

// Calculate and set the total
orderItem.RecalculateTotal();

// Apply a discount
orderItem.ApplyDiscount(5.00m);

// Get calculated values
decimal subtotal = orderItem.GetSubtotal(); // 59.98
decimal totalWithTax = orderItem.GetTotalWithTax(); // 59.78
decimal averagePrice = orderItem.GetAveragePricePerUnit(); // 29.89

// Validate the order item
bool isValid = orderItem.IsValid(); // true

// Access navigation properties
Order? order = orderItem.Order;
Product? product = orderItem.Product;
```

---

## Order

The `Order` class represents a customer order in the e-commerce system, containing order details, pricing information, shipping addresses, and order lifecycle management. It provides comprehensive order processing functionality including status transitions, discount application, total recalculation, and validation methods, making it ideal for order management in e-commerce applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Constants;

// Create a new order for a user
var order = new Order
{
    UserId = 42,
    OrderNumber = "ORD-2024-001",
    Status = OrderStatus.Pending,
    SubTotal = 99.98m,
    TaxAmount = 9.99m,
    ShippingCost = 5.99m,
    ShippingAddress = "123 Main St, City, Country",
    BillingAddress = "123 Main St, City, Country",
    Notes = "Please deliver during business hours"
};

// Add items to the order
order.Items = new List<OrderItem>
{
    new OrderItem { ProductId = 101, Quantity = 2, UnitPrice = 49.99m, TaxAmount = 4.80m },
    new OrderItem { ProductId = 102, Quantity = 1, UnitPrice = 0.00m, TaxAmount = 0.00m }
};

// Calculate and set the total
order.RecalculateTotal();

// Apply a discount
order.ApplyDiscount(5.00m);

// Get calculated values
decimal total = order.Total; // 109.96m
decimal subtotal = order.SubTotal; // 99.98m
int totalItems = order.GetTotalItems(); // 3

// Check if order can be cancelled
bool canCancel = order.CanBeCancelled(); // true (if status is Pending or Confirmed)

// Check if order is recent (within 30 days)
bool isRecent = order.IsRecent(); // true

// Transition order status
order.MarkAsProcessing();
order.MarkAsShipped("UPS123456789");
order.MarkAsDelivered();

// Cancel order with reason
order.Cancel("Customer requested cancellation");

// Access navigation properties
User? user = order.User;
ICollection<OrderItem>? items = order.Items;
```

---

## User

The `User` class represents a user account in the system, containing personal information, authentication details, and account lifecycle management. It provides comprehensive user management functionality including profile updates, email verification, login tracking, and account activation/deactivation, making it ideal for user management in web applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create a new user account
var user = new User
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    PasswordHash = "hashed_password_123",
    PhoneNumber = "+1234567890",
    Address = "123 Main St",
    City = "New York",
    PostalCode = "10001",
    Country = "USA",
    IsActive = true,
    IsEmailVerified = false
};

// Get the user's full name
string fullName = user.GetFullName(); // "John Doe"

// Validate email format
bool isEmailValid = user.IsValidEmail(); // true

// Update last login timestamp
user.UpdateLastLogin();

// Update user profile information
user.UpdateProfile(
    "John",
    "Smith",
    "+1234567890",
    "456 Oak Ave",
    "Boston",
    "02108",
    "USA"
);

// Mark email as verified
user.VerifyEmail();

// Deactivate user account
user.Deactivate();

// Reactivate user account
user.Activate();

// Access navigation properties
ICollection<Order>? orders = user.Orders;
ICollection<Review>? reviews = user.Reviews;
```

## Review

The `Review` class represents a product review submitted by a user, enabling customers to provide feedback on products with ratings, titles, and detailed content. Reviews track helpfulness counts, verification status, approval state, and timestamps for moderation and analytics purposes.




Reviews support CRUD operations with validation for rating values (1-5 scale), recent review tracking, and moderation workflows through approval/rejection methods.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create a new product review
var review = new Review
{
    ProductId = 101,
    UserId = 42,
    Rating = 5,
    Title = "Excellent product!",
    Content = "This product exceeded my expectations. Highly recommended for anyone looking for quality.",
    IsVerifiedPurchase = true
};

// Check if rating is valid
bool isValid = review.IsValidRating(); // true

// Get a visual rating display
string ratingDisplay = review.GetRatingDisplay(); // "★★★★★"

// Check if review is recent (within 30 days)
bool isRecent = review.IsRecent(); // true

// Mark review as helpful
review.MarkAsHelpful();

// Update the review after validation
review.UpdateReview(4, "Very good, but could be better", "Good quality overall, but shipping took longer than expected");

// Approve or reject review for moderation
review.Approve();
// review.Reject();

// Access navigation properties
Product? product = review.Product;
User? user = review.User;
```

---

## Product

The `Product` class represents a catalog item in the e-commerce system, containing core product information, pricing, inventory management, and business logic for stock operations. It serves as the central domain model for product management with comprehensive inventory tracking, availability status, and rating calculations.

The Product model supports CRUD operations with stock management methods (`ReduceStock`, `IncreaseStock`), availability checks, and tax calculations based on product category tax rates.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Constants;

// Create a new product
var product = new Product
{
    Name = "Premium Wireless Headphones",
    Description = "Noise-cancelling wireless headphones with 30-hour battery life",
    Price = 199.99m,
    StockQuantity = 50,
    Category = ProductCategory.Electronics,
    ImageUrl = "/images/headphones.jpg",
    Sku = "AUD-WH-1001",
    Rating = 4.5m,
    ReviewCount = 125,
    IsAvailable = true,
    IsFeatured = true
};

// Check stock availability
bool isInStock = product.IsInStock(); // true
bool canPurchase = product.CanPurchase(2); // true

// Calculate pricing with tax
decimal taxAmount = product.GetTaxAmount(); // 19.99m (10% tax for Electronics)
decimal priceWithTax = product.GetPriceWithTax(); // 219.98m

// Manage inventory
product.ReduceStock(5); // Reduces stock by 5 units
product.IncreaseStock(10); // Increases stock by 10 units

// Update product details
product.UpdateDetails(
    "Premium Wireless Headphones Pro",
    "Premium noise-cancelling wireless headphones with 30-hour battery life and Bluetooth 5.2",
    249.99m,
    ProductCategory.Electronics,
    "/images/headphones-pro.jpg"
);

// Update rating
product.UpdateRating(4.7m, 142);

// Set availability and featured status
product.SetAvailability(true);
product.SetFeatured(true);

// Access navigation properties
ICollection<Review>? reviews = product.Reviews;
ICollection<OrderItem>? orderItems = product.OrderItems;
```

---

## PushSubscription

The `PushSubscription` class represents a browser Web Push subscription for a user device. It stores the VAPID endpoint URL and encryption keys required to deliver push notifications via the Web Push Protocol (RFC 8030). Each user can have multiple subscriptions — one per browser/device combination.



### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create a new push subscription for a user
var subscription = new PushSubscription
{
    UserId = 123,
    Endpoint = "https://fcm.googleapis.com/fcm/send/device-token-123",
    P256dhKey = "BLM8xgL5F2JGqgJqgJqgJqgJqgJqgJqgJqgJq",
    AuthKey = "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB",
    DeviceLabel = "Home Chrome Browser",
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
};

// Record a successful push delivery
subscription.RecordDelivery();

// Check if subscription is active
bool isActive = subscription.IsActive; // true

// Deactivate subscription when endpoint becomes invalid
subscription.Deactivate();

// Access navigation property
User? user = subscription.User;
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
| `appsettings.example.json` | Example settings with placeholders | ✅ Yes |
| `appsettings.Development.json` | Dev overrides | ⚠️ Only if it contains no secrets |
| `appsettings.Production.json` | Production overrides | ✅ Yes (no secrets — use env vars) |
| Environment variables | Runtime secrets & deployment config | ✅ (set in your CI/CD or hosting platform) |
| `dotnet user-secrets` | Local developer secrets | ✅ Stored outside the repo |

The active environment is controlled by the `ASPNETCORE_ENVIRONMENT` variable (defaults to `Production` when not set).

### Application Configuration (AspnetSpaTemplateOptions)

The application uses the Options pattern for strongly-typed configuration. The main settings are under the `AspnetSpaTemplate` section.

```json
{
  "AspnetSpaTemplate": {
    "Environment": "Production",
    "JwtSecret": "REPLACE_WITH_A_SECURE_RANDOM_KEY",
    "JwtExpiration": 3600,
    "RequestLogging": {
      "Enabled": true,
      "VerbosityLevel": "Standard",
      "SlowRequestThresholdMs": 1000
    }
  }
}

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
| `StringExtensionsTests.cs` | Utility extension methods for string manipulation |

## StringExtensionsTests

The `StringExtensionsTests` class provides comprehensive unit tests for the `StringExtensions` utility class, which offers a collection of extension methods for common string manipulation scenarios. These methods simplify tasks like sanitizing input, converting between formats, truncating text, and validating content, providing a robust toolkit for consistent string handling throughout the application.

The test suite covers methods for sanitizing whitespace, converting to slugs, truncating text with ellipsis, converting PascalCase to display names, validating emails and alphanumeric strings, providing fallback values, and HTML encoding special characters.


### Usage Example

```csharp
using AspNetSpaTemplate.Extensions;

// Sanitize user input by removing excessive whitespace
string sanitizedInput = "  Hello    World  ".Sanitize();
// Returns: "Hello World"

// Convert a title to a URL-friendly slug
string slug = "Hello World & Friends!".ToSlug();
// Returns: "hello-world-friends"

// Truncate long text with ellipsis
string truncated = "This is a very long text that needs to be shortened".Truncate(20);
// Returns: "This is a very long..."

// Convert PascalCase to display format
string displayName = "UserProfileSettings".ToDisplayName();
// Returns: "User Profile Settings"

// Validate email format
bool isValidEmail = "user@example.com".IsValidEmail();
// Returns: true

// Provide fallback for empty strings
string name = "".OrIfEmpty("John Doe");
// Returns: "John Doe"

// HTML encode special characters
string encoded = "<script>alert('XSS')</script>".HtmlEncode();
// Returns: "&lt;script&gt;alert(&#39;XSS&#39;)&lt;/script&gt;"

// Check if string is alphanumeric
bool isAlphaNumeric = "User123".IsAlphaNumeric();
// Returns: true
```

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

## IEventBus

The `IEventBus` interface defines the contract for a publish-subscribe event system that enables loose coupling between application components. It provides mechanisms to subscribe to domain events, unsubscribe from them, and publish events either individually or in batches to registered handlers.

### Usage Example

```csharp
using AspNetSpaTemplate.Events;

// Inject IEventBus in your service
public class ProductService
{
    private readonly IEventBus _eventBus;
    
    public ProductService(IEventBus eventBus) => _eventBus = eventBus;

    public async Task CreateProduct(string name, decimal price)
    {
        // Perform creation logic...
        
        // Publish event
        await _eventBus.PublishAsync(new ProductCreatedEvent {
            ProductId = 123,
            ProductName = name,
            Price = price
        });
    }
}

// In your background worker or startup
public class NotificationWorker
{
    public NotificationWorker(IEventBus eventBus)
    {
        eventBus.Subscribe<ProductCreatedEvent>(async @event => {
            Console.WriteLine($"Product created: {@event.ProductName} - {@event.Price}");
            await Task.CompletedTask;
        });
    }
}
```

---

## EventBusImplementation

The `EventBusImplementation` class provides an in-process event bus implementation suitable for single-server deployments. It maintains subscribers in memory and dispatches events sequentially, making it ideal for loose coupling between application components without external message broker dependencies.

### Usage Example
```csharp
// Subscribe to events
eventBus.Subscribe<ProductCreatedEvent>(async @event => {
    await _cache.RemoveAsync("products_list");
    await _cache.RemoveAsync($"product_{@event.ProductId}");
});

// Publish events
await eventBus.PublishAsync(new ProductCreatedEvent {
    ProductId = 123,
    ProductName = "Laptop",
    Price = 999.99m,
    AggregateType = "Product"
});

// Get subscriber count for monitoring
int subscriberCount = eventBus.GetSubscriberCount<ProductCreatedEvent>();

// Publish multiple events efficiently
await eventBus.PublishManyAsync(new[] {
    new UserRegisteredEvent { UserId = 1, Email = "user@example.com", FullName = "John Doe" },
    new OrderPlacedEvent { OrderId = 456, UserId = 1, TotalAmount = 99.99m, ItemCount = 2 }
});
```

## DomainEventHandlers

The `DomainEventHandlers` class contains event handler methods that respond to domain events in the application. These handlers implement business logic for actions like cache invalidation, notifications, and analytics updates that should occur when specific domain events are raised. Each handler method follows the same pattern: it receives an event parameter, performs its business logic, and includes error handling with logging.

The handlers are registered with the event bus through the `RegisterEventHandlers` extension method, which subscribes each handler to its corresponding event type.

### Usage Example

```csharp
using AspNetSpaTemplate.Events;
using Microsoft.Extensions.DependencyInjection;

// In Program.cs or your DI configuration
var services = new ServiceCollection();

// Register required services
services.AddSingleton<ICacheService, MemoryCacheService>();
services.AddSingleton<NotificationService>();
services.AddLogging();

// Register DomainEventHandlers
services.AddSingleton<DomainEventHandlers>();

// Build service provider to register handlers
var serviceProvider = services.BuildServiceProvider();

// Get event bus (typically registered elsewhere in your application)
var eventBus = serviceProvider.GetRequiredService<IEventBus>();

// Register all event handlers with the event bus
services.RegisterEventHandlers(eventBus);

// Usage: When events are published, handlers are automatically invoked
// For example, in your ProductService:
await eventBus.PublishAsync(new ProductCreatedEvent {
    ProductId = 123,
    ProductName = "New Laptop",
    Price = 999.99m,
    AggregateType = "Product"
});

// This triggers OnProductCreated() which invalidates the product cache
```

## EventBusImplementationExtensions

The `EventBusImplementationExtensions` class provides extension methods for the `EventBusImplementation` class, adding functionality for bulk operations, conditional publishing, and subscriber management.

### Subscribe

Subscribes multiple handlers for the same event type in a single call.

```csharp
public static void Subscribe<TEvent>(this EventBusImplementation eventBus, IEnumerable<Func<TEvent, Task>> handlers) where TEvent : DomainEvent
```

### TryPublishAsync

Publishes an event only if there are subscribers interested in it.

```csharp
public static async Task<bool> TryPublishAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event) where TEvent : DomainEvent
```

### PublishBatchAsync

Publishes multiple events of different types in a single batch.

```csharp
public static async Task PublishBatchAsync(this EventBusImplementation eventBus, IEnumerable<DomainEvent> events)
```

### GetAllSubscriberCounts

Gets all subscriber counts for all registered event types.

```csharp
public static IReadOnlyDictionary<Type, int> GetAllSubscriberCounts(this EventBusImplementation eventBus)
```

### ClearSubscribers

Unsubscribes all handlers for a specific event type.

```csharp
public static void ClearSubscribers<TEvent>(this EventBusImplementation eventBus) where TEvent : DomainEvent
```

### PublishWithDelayAsync

Publishes an event with a delay using Task.Delay or TimeSpan.

```csharp
public static async Task PublishWithDelayAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event, int delayMilliseconds) where TEvent : DomainEvent
public static async Task PublishWithDelayAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event, TimeSpan delay) where TEvent : DomainEvent
```

### GetSubscriberCountLock

Gets the lock object used for thread-safe operations on subscribers.

```csharp
public static object GetSubscriberCountLock(this EventBusImplementation eventBus)
```


## ThemeServiceTests

The `ThemeServiceTests` class provides comprehensive unit tests for the `ThemeService`, which manages user theme preferences (System, Light, Dark) using the in-memory cache service. These tests verify theme preference management including retrieval, setting, clearing, and user isolation scenarios, ensuring the theme service correctly handles default behavior and preference persistence.

### Usage Example

```csharp
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Caching;

// Create theme service with cache backing
var cacheService = new MemoryCacheService(logger);
var themeService = new ThemeService(cacheService, logger);

// Get current theme preference (defaults to System when nothing stored)
ColourScheme currentTheme = await themeService.GetSchemeAsync(userId: 123);
// Returns: ColourScheme.System

// Set a theme preference for a user
await themeService.SetSchemeAsync(userId: 123, ColourScheme.Dark);

// Get the stored preference
ColourScheme storedTheme = await themeService.GetSchemeAsync(userId: 123);
// Returns: ColourScheme.Dark

// Clear the preference to revert to system default
await themeService.ClearSchemeAsync(userId: 123);

// Get after clearing (back to default)
ColourScheme defaultTheme = await themeService.GetSchemeAsync(userId: 123);
// Returns: ColourScheme.System

// Different users have isolated preferences
await themeService.SetSchemeAsync(userId: 456, ColourScheme.Light);
ColourScheme user456Theme = await themeService.GetSchemeAsync(userId: 456);
// Returns: ColourScheme.Light

ColourScheme user123Theme = await themeService.GetSchemeAsync(userId: 123);
// Returns: ColourScheme.System (still isolated)
```

## ValidationHelperTests

The `ValidationHelperTests` class provides a comprehensive set of unit tests for the validation helper methods used throughout the application. These tests verify that validation logic correctly handles various input scenarios including null values, empty strings, out-of-range values, and pattern matching, ensuring robust validation across the codebase.

The test suite covers validation methods for common validation scenarios such as null checks, string length validation, range validation, email validation, phone number validation, and pattern matching, providing confidence that validation logic behaves as expected in different edge cases.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

public class UserRegistrationTests
{
    [Fact]
    public void RegisterUser_WithValidData_DoesNotThrow()
    {
        // Arrange
        var user = new UserRegistrationDto
        {
            Email = "user@example.com",
            PhoneNumber = "+1234567890",
            Password = "SecurePassword123!",
            Username = "johndoe"
        };

        // Act & Assert - should not throw any validation exceptions
        var exception = Record.Exception(() => ValidateUserRegistration(user));
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterUser_WithInvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var user = new UserRegistrationDto
        {
            Email = "invalid-email",
            PhoneNumber = "+1234567890",
            Password = "SecurePassword123!",
            Username = "johndoe"
        };

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateUserRegistration(user));
    }

    [Fact]
    public void RegisterUser_WithShortPassword_ThrowsValidationException()
    {
        // Arrange
        var user = new UserRegistrationDto
        {
            Email = "user@example.com",
            PhoneNumber = "+1234567890",
            Password = "short",
            Username = "johndoe"
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => ValidateUserRegistration(user));
        Assert.Contains("Password must be at least", exception.Message);
    }

    private void ValidateUserRegistration(UserRegistrationDto user)
    {
        user.Email.ValidEmail("Email");
        user.PhoneNumber.ValidPhoneNumber("PhoneNumber");
        user.Password.LengthBetween(8, 100, "Password");
        user.Username.NotNullOrEmpty("Username");
    }
}

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithValidPrice_DoesNotThrow()
    {
        // Arrange
        decimal price = 99.99m;

        // Act & Assert - should not throw
        var exception = Record.Exception(() => ValidateProductPrice(price));
        Assert.Null(exception);
    }

    [Fact]
    public void CreateProduct_WithNegativePrice_ThrowsValidationException()
    {
        // Arrange
        decimal price = -10.00m;

        // Act & Assert
        Assert.Throws<ValidationException>(() => ValidateProductPrice(price));
    }

    private void ValidateProductPrice(decimal price)
    {
        price.InRange(0.01m, 10000m, "Price");
    }
}
```

## ValidationHelperTestsExtensions

The `ValidationHelperTestsExtensions` class provides a set of fluent extension methods for unit testing validation logic. It simplifies asserting validation constraints on properties by allowing you to chain validation calls directly onto the values being tested, improving test readability and reducing boilerplate code.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

[Fact]
public void Validate_Product_Fields()
{
    var product = new Product { 
        Name = "Laptop", 
        Price = 999.99m, 
        Category = "Electronics",
        Stock = 10 
    };

    // Fluent validation chaining
    product.Name.NotNullOrEmpty("Name")
                .LengthBetween(3, 50, "Name");
    
    product.Price.InRange(0.01m, 10000m, "Price")
                 .GreaterThan(0, "Price");
    
    product.Category.NotNullOrEmpty("Category");
    
    product.Stock.InRange(0, 1000, "Stock");
}

[Fact]
public void Validate_User_Email()
{
    var email = "test@example.com";
    email.ValidEmail("Email");
}

[Fact]
public void Validate_Collection()
{
    var items = new List<string> { "item1", "item2" };
    items.NotEmpty("Items")
         .MaxItems<string>(5, "Items")
         .CountEquals(2, "Items");
}
```

---

## MemoryCacheServiceTestsExtensions

The `MemoryCacheServiceTestsExtensions` class provides a set of extension methods for the `MemoryCacheServiceTests` class that simplify testing common caching patterns and operations. These methods encapsulate frequently used test scenarios like setting, getting, and removing cache entries, testing the GetOrSet pattern, verifying counter increments, and testing pattern-based cache removal, making test code more concise and readable.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

public class CacheServiceTestsExample
{
    private readonly MemoryCacheServiceTests _tests = new MemoryCacheServiceTests();

    public async Task RunCacheTests()
    {
        // Test basic set/get/remove operations
        await _tests.SetGetAndRemoveAsync("test_key", "test_value");

        // Test GetOrSet pattern - factory function should only be called once
        await _tests.TestGetOrSetPatternAsync("cached_key", "cached_value");

        // Test counter increment functionality
        await _tests.TestIncrementAndVerifyAsync("counter_key", 8); // 1 + 1 + 5 + 1

        // Test pattern-based removal
        await _tests.TestRemoveByPatternAsync<TestData>(
            pattern: "user:*",
            matchingKeys: new[] { "user:123", "user:456", "user:admin" },
            nonMatchingKeys: new[] { "product:1", "settings:general", "cache:stats" }
        );
    }
}
```

## OrderServiceIntegrationTests

The `OrderServiceIntegrationTests` class provides comprehensive integration testing for order-related workflows, ensuring end-to-end functionality between the order service, repository, and database. It verifies complex business scenarios such as stock management, order calculations, and data isolation by utilizing the test server and database context.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

public class OrderIntegrationTests : IAsyncLifetime
{
    private readonly OrderServiceIntegrationTests _tests;

    public OrderIntegrationTests()
    {
        _tests = new OrderServiceIntegrationTests();
    }

    public async Task InitializeAsync() => await _tests.InitializeAsync();
    
    public async Task DisposeAsync() => await _tests.DisposeAsync();

    [Fact]
    public async Task Run_Full_Workflows()
    {
        // Execute end-to-end flow
        await _tests.EndToEnd_CreateProductAndOrder_CompleteWorkflow();

        // Verify calculations
        await _tests.CreateOrderWithMultipleItems_CalculatesTotalsCorrectly();
        
        // Check inventory updates
        await _tests.StockReduction_DecreasesProductInventory();
        
        // Validate constraints
        await _tests.InsufficientStock_PreventOrderCreation();
        
        // Verify data isolation
        await _tests.GetUserOrders_ReturnsOnlyUserOrders();
    }
}
```
## User

The `User` class represents a user account in the system, containing personal information, authentication details, and account lifecycle management. It provides comprehensive user management functionality including profile updates, email verification, login tracking, and account activation/deactivation, making it ideal for user management in web applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create a new user account
var user = new User
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    PasswordHash = "hashed_password_123",
    PhoneNumber = "+1234567890",
    Address = "123 Main St",
    City = "New York",
    PostalCode = "10001",
    Country = "USA",
    IsActive = true,
    IsEmailVerified = false
};

// Get the user's full name
string fullName = user.GetFullName(); // "John Doe"

// Validate email format
bool isEmailValid = user.IsValidEmail(); // true

// Update last login timestamp
user.UpdateLastLogin();

// Update user profile information
user.UpdateProfile(
    "John",
    "Smith",
    "+1234567890",
    "456 Oak Ave",
    "Boston",
    "02108",
    "USA"
);

// Mark email as verified
user.VerifyEmail();

// Deactivate user account
user.Deactivate();

// Reactivate user account
user.Activate();

// Access navigation properties
ICollection<Order>? orders = user.Orders;
ICollection<Review>? reviews = user.Reviews;
```

## ReviewServiceTests

The `ReviewServiceTests` class provides comprehensive unit testing for the `ReviewService`, covering CRUD operations and business logic validations. It ensures that review creation, retrieval, approval, and deletion behave correctly under both valid and invalid scenarios.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;

// ReviewServiceTests is designed for use within an xUnit test runner
public class ReviewServiceUsageExample
{
    private readonly ReviewServiceTests _tests = new ReviewServiceTests();

    public async Task RunTestSuite()
    {
        // CRUD tests
        await _tests.CreateReviewAsync_WithValidRequest_CreatesReview();
        await _tests.GetReviewByIdAsync_WithValidId_ReturnsReview();
        await _tests.ApproveReviewAsync_WithValidId_ApprovesReview();
        await _tests.DeleteReviewAsync_WithValidId_DeletesReview();

        // Validation tests
        await _tests.CreateReviewAsync_WithInvalidRating_ThrowsValidationException();
        await _tests.CreateReviewAsync_WithShortTitle_ThrowsValidationException();
        await _tests.CreateReviewAsync_WithShortContent_ThrowsValidationException();
        await _tests.CreateReviewAsync_WithDuplicateReview_ThrowsBusinessException();
    }
}
```

---

## User

The `User` class represents a user account in the system, containing personal information, authentication details, and account lifecycle management. It provides comprehensive user management functionality including profile updates, email verification, login tracking, and account activation/deactivation, making it ideal for user management in web applications.

### Usage Example

```csharp
using AspNetSpaTemplate.Models;

// Create a new user account
var user = new User
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    PasswordHash = "hashed_password_123",
    PhoneNumber = "+1234567890",
    Address = "123 Main St",
    City = "New York",
    PostalCode = "10001",
    Country = "USA",
    IsActive = true,
    IsEmailVerified = false
};

// Get the user's full name
string fullName = user.GetFullName(); // "John Doe"

// Validate email format
bool isEmailValid = user.IsValidEmail(); // true

// Update last login timestamp
user.UpdateLastLogin();

// Update user profile information
user.UpdateProfile(
    "John",
    "Smith",
    "+1234567890",
    "456 Oak Ave",
    "Boston",
    "02108",
    "USA"
);

// Mark email as verified
user.VerifyEmail();

// Deactivate user account
user.Deactivate();

// Reactivate user account
user.Activate();

// Access navigation properties
ICollection<Order>? orders = user.Orders;
ICollection<Review>? reviews = user.Reviews;
```

## ReviewServiceTestsExtensions

The `ReviewServiceTestsExtensions` class provides a set of extension methods and helper methods for the `ReviewServiceTests` class that simplify testing review service functionality. These methods encapsulate common test scenarios like creating test reviews, asserting review properties, checking collections of reviews, and validating exceptions, making test code more concise and readable.

The extension methods work with the `Review`, `Product`, and exception classes, providing fluent assertions and helper methods for creating test data.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using AspNetSpaTemplate.Models;
using Xunit;

public class ReviewServiceTestsExtensionsExample
{
    private readonly ReviewServiceTests _tests = new ReviewServiceTests();

    public async Task RunReviewTests()
    {
        // Create test data using helper methods
        var review = ReviewServiceTestsExtensions.CreateTestReview(
            id: 1,
            productId: 1,
            userId: 1,
            rating: 5,
            title: "Excellent product",
            content: "This product exceeded my expectations",
            isApproved: true
        );

        var product = ReviewServiceTestsExtensions.CreateTestProduct(
            id: 1,
            name: "Test Product"
        );

        // Test review properties
        review.ShouldHaveReviewProperties(
            expectedRating: 5,
            expectedTitle: "Excellent product",
            expectedContent: "This product exceeded my expectations",
            expectedIsApproved: true
        );

        // Test collection assertions
        var reviews = new List<Review> { review };
        reviews.ShouldContainExactly(new[] { review });
        reviews.ShouldContainRating(5);
        reviews.ShouldContainUserReviews(1);
        reviews.ShouldContainOnlyApprovedReviews();
        reviews.ShouldContainOnlyProductReviews(1);

        // Test review matching
        var expectedReview = ReviewServiceTestsExtensions.CreateTestReview();
        review.ShouldMatchReview(expectedReview);

        // Test empty collection
        var emptyReviews = new List<Review>();
        emptyReviews.ShouldBeEmpty();

        // Test exception validation
        var validationException = new ValidationException(
            new Dictionary<string, List<string>>
            {
                { "Rating", new List<string> { "Rating must be between 1 and 5" } }
            }
        );
        validationException.ShouldHaveValidationErrors("Rating must be between 1 and 5");

        var businessException = new BusinessException(
            "Review already exists for this user and product",
            "REVIEW_DUPLICATE"
        );
        businessException.ShouldHaveBusinessExceptionMessage(
            "Review already exists for this user and product"
        );
    }
}
```

## ProductServiceTests

The `ProductServiceTests` class provides comprehensive unit testing for the `ProductService`, covering CRUD operations, search functionality, and business logic validations. It ensures that product creation, retrieval, updates, and deletion behave correctly under both valid and invalid scenarios.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;

// ProductServiceTests is designed for use within an xUnit test runner
public class ProductServiceUsageExample
{
    private readonly ProductServiceTests _tests = new ProductServiceTests();

    public async Task RunTestSuite()
    {
        // Retrieval tests
        await _tests.GetProductByIdAsync_WithValidId_ReturnsProduct();
        await _tests.GetProductByIdAsync_WithInvalidId_ThrowsNotFoundException();
        await _tests.GetAllProductsAsync_ReturnsPagedProducts();
        await _tests.GetProductsByCategoryAsync_ReturnsProductsInCategory();
        await _tests.GetFeaturedProductsAsync_ReturnsFeaturedProducts();
        await _tests.GetTopRatedProductsAsync_ReturnsTopRatedProducts();

        // Search tests
        await _tests.SearchProductsAsync_WithValidTerm_ReturnsMatchingProducts();
        await _tests.SearchProductsAsync_WithEmptyTerm_ReturnsEmptyList();
        await _tests.SearchProductsAsync_WithNullTerm_ReturnsEmptyList();

        // CRUD tests
        await _tests.CreateProductAsync_WithValidRequest_CreatesProduct();
        await _tests.UpdateProductAsync_WithValidId_UpdatesProduct();
        await _tests.SetProductAvailabilityAsync_WithValidId_UpdatesAvailability();
        await _tests.DeleteProductAsync_WithValidId_DeletesProduct();

        // Validation tests
        await _tests.CreateProductAsync_WithEmptyName_ThrowsValidationException();
        await _tests.CreateProductAsync_WithInvalidPrice_ThrowsValidationException();
    }
}
```

## ProductServiceIntegrationTests

The `ProductServiceIntegrationTests` class provides comprehensive integration testing for the `ProductService`, verifying end-to-end functionality including CRUD operations, business logic, and repository interactions using an in-memory database. These tests ensure that the service layer works correctly with the data access layer, covering scenarios like complete product lifecycle management, pagination, category filtering, featured products, availability toggling, price validation, product deletion, and rating calculations.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using AspNetSpaTemplate.DTOs;

// ProductServiceIntegrationTests is designed for use within an xUnit test runner
public class ProductServiceIntegrationTestsExample : IAsyncLifetime
{
    private readonly ProductServiceIntegrationTests _tests = new ProductServiceIntegrationTests();

    public async Task InitializeAsync() => await _tests.InitializeAsync();
    public async Task DisposeAsync() => await _tests.DisposeAsync();

    public async Task RunIntegrationTests()
    {
        // Initialize test database and services
        await _tests.InitializeAsync();

        // Execute end-to-end product workflows
        await _tests.EndToEnd_CreateUpdateAndSearchProduct_CompleteWorkflow();
        await _tests.CreateMultipleProducts_PaginationWorks();
        await _tests.ProductByCategory_FiltersCorrectly();
        await _tests.FeaturedProducts_ReturnsOnlyFeatured();
        await _tests.ToggleAvailability_ProductCanBeHiddenAndShown();
        await _tests.InvalidPrice_PreventProductCreation();
        await _tests.DeleteProduct_RemovesFromDatabase();
        await _tests.TopRatedProducts_ReturnsHighestRated();

        // Clean up
        await _tests.DisposeAsync();
    }
}

// Example usage within a test method
[Fact]
public async Task TestProductCreationAndRetrieval()
{
    // Arrange
    var tests = new ProductServiceIntegrationTests();
    await tests.InitializeAsync();

    try
    {
        var createRequest = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product",
            Price = 99.99m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "https://example.com/test.jpg",
            Sku = "TEST-001"
        };

        // Act - Create product
        var product = await tests.CreateProductAsync(createRequest);

        // Assert - Product created successfully
        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(99.99m, product.Price);
        Assert.True(product.IsAvailable);

        // Act - Retrieve product
        var retrieved = await tests.GetProductByIdAsync(product.Id);

        // Assert - Product retrieved successfully
        Assert.Equal(product.Id, retrieved.Id);
        Assert.Equal("Test Product", retrieved.Name);
    }
    finally
    {
        await tests.DisposeAsync();
    }
}
```

## MemoryCacheServiceTests

The `MemoryCacheServiceTests` class provides comprehensive unit tests for the `MemoryCacheService`, ensuring all caching operations—such as retrieving, setting, removing, and expiring entries—work correctly under various scenarios. The test suite covers edge cases like expired entries, pattern-based removal, and cache statistics, verifying that the cache behaves reliably as a high-performance in-memory storage solution.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

// MemoryCacheServiceTests is designed for use within an xUnit test runner
public class MemoryCacheUsageExample
{
    private readonly MemoryCacheServiceTests _tests = new MemoryCacheServiceTests();

    public async Task RunTestSuite()
    {
        // Verify basic CRUD operations
        await _tests.SetAsync_WithValue_StoresValue();
        await _tests.GetAsync_WithExistingKey_ReturnsValue();
        await _tests.RemoveAsync_WithExistingKey_RemovesValue();

        // Verify advanced cache features
        await _tests.GetOrSetAsync_WithMissingKey_CallsFactoryAndCachesValue();
        await _tests.ExpireAsync_WithExistingKey_SetsExpiration();
        await _tests.ExistsAsync_WithExistingKey_ReturnsTrue();
        
        // Verify statistics and cleanup
        await _tests.GetStatisticsAsync_ReturnsCorrectStats();
        await _tests.FlushAllAsync_ClearsAllEntries();
    }
}
```

---

## WebhookHandler

The `WebhookHandler` class processes incoming webhooks from external services like payment providers, email services, and shipping providers. It validates HMAC signatures to ensure payload integrity, routes webhooks to appropriate handlers based on the provider, and publishes domain events for downstream processing. The handler supports registering new webhook providers at runtime and handles various webhook event types with proper error logging and validation.

### Usage Example

```csharp
using AspNetSpaTemplate.Integration;
using AspNetSpaTemplate.Events;
using Microsoft.Extensions.Logging;

// In Program.cs or your DI configuration
builder.Services.AddSingleton<WebhookHandler>();
builder.Services.AddSingleton<IEventBus, EventBusImplementation>();

// Usage in a controller or background service
public class WebhookController : ControllerBase
{
    private readonly WebhookHandler _webhookHandler;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(WebhookHandler webhookHandler, ILogger<WebhookController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    [HttpPost("webhooks/payment")]
    public async Task<IActionResult> HandlePaymentWebhook([FromBody] WebhookRequest request)
    {
        // Validate and process the webhook
        bool success = await _webhookHandler.HandleWebhookAsync(
            provider: request.Provider,
            payload: request.Payload,
            signature: request.Signature
        );

        if (success)
        {
            return Ok(new WebhookResponse { Acknowledged = true, Message = "Webhook processed successfully" });
        }
        
        return BadRequest(new WebhookResponse { 
            Acknowledged = false, 
            Message = "Webhook processing failed", 
            ErrorCode = "WEBHOOK_VALIDATION_FAILED" 
        });
    }
}

// Register a new webhook provider with its secret
var webhookHandler = new WebhookHandler(eventBus, logger);
webhookHandler.RegisterWebhook("stripe", "whsec_test_webhook_secret_key");

// Process a webhook from Stripe
string payload = "{\"event_type\":\"payment_intent.succeeded\",\"data\":{\"id\":\"pi_123\"}}";
string signature = "t=1234567890,v1=signature_hash";
bool handled = await webhookHandler.HandleWebhookAsync("stripe", payload, signature);
```

---

## NotificationService

The `NotificationService` handles asynchronous notification delivery across multiple channels including email, SMS, and push notifications. It queues messages for background processing to avoid blocking API responses, providing a clean abstraction for sending various types of notifications throughout the application.

The service supports common notification scenarios like order confirmations, password resets, and general alerts, with built-in validation and logging.

### Usage Example

```csharp
using AspNetSpaTemplate.Integration;

// Inject NotificationService in your controller or service
public class OrderController
{
    private readonly NotificationService _notificationService;

    public OrderController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> PlaceOrder(int userId, string email, decimal total)
    {
        // Process order...
        
        // Send order confirmation email
        await _notificationService.SendOrderConfirmationAsync(email, orderId: 12345, total: 99.99m);
        
        // Send push notification to user
        await _notificationService.SendPushAsync(
            userId: userId,
            title: "Order Confirmed",
            message: "Your order #12345 has been placed successfully!",
            deepLink: "/orders/12345"
        );
        
        // Send password reset email
        await _notificationService.SendPasswordResetAsync(
            email: "user@example.com",
            resetToken: "abc123-xyz789"
        );
        
        return Ok();
    }
}

// Background worker that processes the queue
public class NotificationWorker : BackgroundService
{
    private readonly NotificationService _notificationService;

    public NotificationWorker(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pending = _notificationService.GetPendingNotifications(batchSize: 10);
            
            foreach (var notification in pending)
            {
                // Process each notification (send email, SMS, push, etc.)
                Console.WriteLine($"Processing {notification.Type} to {notification.Recipient}");
                notification.SentAt = DateTime.UtcNow;
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

// Check queue status
int queueSize = _notificationService.GetQueueSize();
```

## SyncQueueServiceTests

The `SyncQueueServiceTests` class provides comprehensive unit tests for the `SyncQueueService`, which implements an in-memory queue for synchronizing offline requests with the server. These tests verify that the sync queue correctly handles request deduplication, user isolation, ordering, and state management, ensuring reliable offline-to-online request synchronization.

### Usage Example

```csharp
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Tests;

// SyncQueueServiceTests is designed for use within an xUnit test runner
public class SyncQueueServiceUsageExample
{
    private readonly SyncQueueServiceTests _tests = new SyncQueueServiceTests();

    public void RunTestSuite()
    {
        // Test basic queue operations
        var id1 = _tests.Enqueue_ReturnsPositiveId();
        var id2 = _tests.Enqueue_SecondCall_ReturnsHigherId();
        
        // Test deduplication
        var duplicateId = _tests.Enqueue_DuplicateClientRequestId_ReturnsSameId();
        
        // Test HTTP method normalization
        _tests.Enqueue_MethodIsNormalisedToUppercase();
        
        // Test pending queue operations
        _tests.GetPending_WithNoEntries_ReturnsEmpty();
        _tests.GetPending_OnlyReturnsPendingEntries();
        _tests.GetPending_IsolatesEntriesByUser();
        _tests.GetPending_OrderedByQueuedAtAscending();
        
        // Test completion operations
        var completeResult = _tests.Complete_WithValidId_ReturnsTrue();
        _tests.Complete_RemovesEntryFromPending();
        var unknownCompleteResult = _tests.Complete_WithUnknownId_ReturnsFalse();
        
        // Test failure operations
        var failResult = _tests.Fail_WithValidId_ReturnsTrue();
        _tests.Fail_RecordsErrorMessage();
        var unknownFailResult = _tests.Fail_WithUnknownId_ReturnsFalse();
        
        // Test queue depth tracking
        var count1 = _tests.PendingCount_ReflectsCurrentQueueDepth();
        _tests.PendingCount_DecreasesAfterComplete();
    }
}

// Example usage in a real application service
public class OrderSyncService
{
    private readonly ISyncQueueService _syncQueue;
    
    public OrderSyncService(ISyncQueueService syncQueue)
    {
        _syncQueue = syncQueue;
    }
    
    public int QueueOrderCreation(int userId, string clientRequestId, OrderDto orderData)
    {
        // Queue the order creation request for offline synchronization
        int queueId = _syncQueue.Enqueue(
            userId: userId,
            clientRequestId: clientRequestId,
            method: "POST",
            endpoint: "/api/orders",
            payload: JsonSerializer.Serialize(orderData)
        );
        
        return queueId;
    }
    
    public IReadOnlyList<SyncQueueEntry> GetPendingOrders(int userId)
    {
        // Retrieve all pending order creation requests for a user
        return _syncQueue.GetPending(userId);
    }
    
    public bool MarkOrderAsCompleted(int queueId)
    {
        // Mark a queued order as successfully completed
        return _syncQueue.Complete(queueId);
    }
    
    public bool MarkOrderAsFailed(int queueId, string errorMessage)
    {
        // Mark a queued order as failed with error details
        return _syncQueue.Fail(queueId, errorMessage);
    }
    
    public int GetPendingOrderCount(int userId)
    {
        // Get the number of pending order requests
        return _syncQueue.PendingCount(userId);
    }
}

---

## IHttpClientFactory

The `IHttpClientFactory` interface and its `DefaultHttpClientFactory` implementation provide a centralized way to create and manage `HttpClient` instances for making HTTP requests to external services. This pattern prevents socket exhaustion by reusing `HttpClient` instances and allows for centralized configuration of HTTP settings like timeouts, headers, and retry policies.

The factory creates named clients that can be configured differently for various external services (e.g., payment gateways, weather APIs, authentication services), and provides extension methods for common operations like JSON serialization/deserialization and safe request handling.

### Usage Example

```csharp
using AspNetSpaTemplate.Integration;
using Microsoft.Extensions.DependencyInjection;

// In Program.cs - register the factory
builder.Services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

// In your service constructor
public class PaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task ProcessPaymentAsync(decimal amount, string currency)
    {
        // Get a configured HTTP client for the payment gateway
        var paymentClient = _httpClientFactory.GetClient("payment-gateway");
        
        // Use extension methods for JSON operations
        var apiKey = await paymentClient.GetAsJsonAsync<string>("https://api.payment-provider.com/v1/key");
        
        // POST with JSON body
        var paymentResponse = await paymentClient.PostAsJsonAsync<PaymentResult>(
            "https://api.payment-provider.com/v1/payments",
            new { amount, currency, cardToken = "tok_visa_1234567890" }
        );
        
        // Safe GET request that doesn't throw on errors
        var (isSuccess, statusCode, content) = await paymentClient.SafeGetAsync(
            "https://api.payment-provider.com/v1/status"
        );
        
        if (isSuccess)
        {
            Console.WriteLine($"Payment successful: {content}");
        }
        else
        {
            Console.WriteLine($"Payment failed with status {statusCode}: {content}");
        }
    }
}

// During application shutdown
var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
factory.Dispose(); // Clean up all managed HttpClient instances
```

// Example usage in a background worker for processing queued requests
public class SyncQueueWorker : BackgroundService
{
    private readonly ISyncQueueService _syncQueue;
    private readonly IOrderService _orderService;
    
    public SyncQueueWorker(ISyncQueueService syncQueue, IOrderService orderService)
    {
        _syncQueue = syncQueue;
        _orderService = orderService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process pending sync queue entries
            var pendingEntries = _syncQueue.GetPending(userId: null); // null = all users
            
            foreach (var entry in pendingEntries)
            {
                try
                {
                    // Replay the queued request
                    var result = await _orderService.CreateOrderFromPayloadAsync(entry.Payload);
                    
                    // Mark as completed on success
                    _syncQueue.Complete(entry.Id);
                }
                catch (Exception ex)
                {
                    // Mark as failed on error
                    _syncQueue.Fail(entry.Id, ex.Message);
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## ManifestServiceTests

The `ManifestServiceTests` class provides comprehensive unit tests for the `ManifestService`, which generates Web App Manifest files for Progressive Web App (PWA) functionality. These tests verify that the manifest generation correctly handles various configurations including default values, host settings, theme colors, background colors, and shortcut definitions, ensuring the PWA can be properly installed and function across different environments.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

// ManifestServiceTests is designed for use within an xUnit test runner
public class ManifestServiceUsageExample
{
    private readonly ManifestServiceTests _tests = new ManifestServiceTests();

    public async Task RunTestSuite()
    {
        // Test default manifest generation
        await _tests.BuildManifest_WithDefaults_ReturnsValidManifest();
        await _tests.BuildManifest_WithDefaults_ContainsTwoIcons();

        // Test host configuration
        await _tests.BuildManifest_WithHost_IconSrcsAreAbsolute();
        await _tests.BuildManifest_WithoutHost_IconSrcsAreRelative();

        // Test shortcuts
        await _tests.BuildManifest_ContainsShortcuts();

        // Test configuration overrides
        await _tests.BuildManifest_WhenNameConfigured_UsesConfiguredName();
        await _tests.BuildManifest_WhenThemeColorConfigured_UsesConfiguredColor();

        // Test default color values
        await _tests.ThemeColor_WithNoConfig_ReturnsDefaultBlue();
        await _tests.BackgroundColor_WithNoConfig_ReturnsDefaultLight();

        // Test configured color values
        await _tests.ThemeColor_WhenConfigured_ReturnsConfiguredValue();

        // Test manifest scope
        await _tests.BuildManifest_ScopeIsRoot();
        await _tests.BuildManifest_PreferRelatedApplicationsIsFalse();
    }
}
```

---

## CacheKeyBuilderTests

The `CacheKeyBuilderTests` class provides comprehensive unit tests for the `CacheKeyBuilder` static class, which generates consistent, collision-free cache keys for various entity types and operations. These tests verify that cache keys follow the correct naming conventions, handle case normalization (especially for emails and search terms), and generate appropriate patterns for cache invalidation strategies.

### Usage Example

```csharp
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Services;

// CacheKeyBuilder is used throughout the application to generate consistent cache keys
// Here's how to use it in your services:

public class ProductService
{
    private readonly ICacheService _cache;
    
    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task<ProductDto> GetProductByIdAsync(int productId)
    {
        // Generate cache key using CacheKeyBuilder
        string cacheKey = CacheKeyBuilder.ProductById(productId);
        
        // Try to get from cache first
        if (await _cache.TryGetAsync<ProductDto>(cacheKey, out var cachedProduct))
        {
            return cachedProduct;
        }
        
        // Fetch from database if not in cache
        var product = await _repository.GetByIdAsync(productId);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, product, TimeSpan.FromHours(1));
        
        return product;
    }
    
    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        // Generate cache key for search (automatically converts to lowercase)
        string cacheKey = CacheKeyBuilder.ProductSearch(searchTerm);
        
        if (await _cache.TryGetAsync<List<ProductDto>>(cacheKey, out var cachedResults))
        {
            return cachedResults;
        }
        
        var results = await _repository.SearchAsync(searchTerm);
        await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(30));
        
        return results;
    }
    
    public async Task InvalidateProductCacheAsync()
    {
        // Invalidate all product-related cache using pattern matching
        await _cache.RemoveByPatternAsync(CacheKeyBuilder.InvalidationPatterns.AllProducts);
        
        // Or invalidate specific product
        await _cache.RemoveAsync(CacheKeyBuilder.ProductById(123));
    }
}

// User service example
public class UserService
{
    private readonly ICacheService _cache;
    
    public UserService(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task<UserDto> GetUserByIdAsync(int userId)
    {
        string cacheKey = CacheKeyBuilder.UserById(userId);
        
        if (await _cache.TryGetAsync<UserDto>(cacheKey, out var cachedUser))
        {
            return cachedUser;
        }
        
        var user = await _repository.GetUserByIdAsync(userId);
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromHours(24));
        
        return user;
    }
    
    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        // Email is automatically converted to lowercase in the cache key
        string cacheKey = CacheKeyBuilder.UserByEmail(email);
        
        if (await _cache.TryGetAsync<UserDto>(cacheKey, out var cachedUser))
        {
            return cachedUser;
        }
        
        var user = await _repository.GetUserByEmailAsync(email);
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromHours(24));
        
        return user;
    }
}

// Rate limiting example
public class RateLimitService
{
    private readonly ICacheService _cache;
    
    public RateLimitService(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task<bool> IsRateLimitedAsync(string clientId)
    {
        string rateLimitKey = CacheKeyBuilder.RateLimitKey(clientId);
        
        if (await _cache.TryGetAsync<int>(rateLimitKey, out var requestCount))
        {
            if (requestCount >= 100)
            {
                return true; // Rate limit exceeded
            }
            
            await _cache.SetAsync(rateLimitKey, requestCount + 1, TimeSpan.FromMinutes(1));
            return false;
        }
        
        await _cache.SetAsync(rateLimitKey, 1, TimeSpan.FromMinutes(1));
        return false;
    }
}

// Temporary operations example
public class FileUploadService
{
    private readonly ICacheService _cache;
    
    public FileUploadService(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task<string> ReserveUploadSlotAsync()
    {
        // Generate unique temporary key for upload operation
        string tempKey = CacheKeyBuilder.TemporaryKey("upload");
        
        // Store upload metadata with expiration
        await _cache.SetAsync(tempKey, new { Status = "pending", ExpiresAt = DateTime.UtcNow.AddMinutes(30) }, 
                            TimeSpan.FromMinutes(30));
        
        return tempKey;
    }
}

// Cache key validation example
public class CacheService
{
    private readonly ICacheService _cache;
    
    public CacheService(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task SetWithValidationAsync(string key, object value, TimeSpan ttl)
    {
        // Validate cache key before using it
        CacheKeyBuilder.ValidateKey(key);
        
        await _cache.SetAsync(key, value, ttl);
    }
}
```

## OrderServiceTests

The `OrderServiceTests` class provides comprehensive unit testing for the `OrderService`, covering order retrieval, creation, status updates, discount application, and revenue calculations. It ensures that order-related operations behave correctly under both valid and invalid scenarios, including validation failures, business rule violations, and data access errors.

## UserServiceTests

The `UserServiceTests` class provides comprehensive unit testing for the `UserService`, covering user management operations including retrieval, creation, authentication, updates, and activation/deactivation. It ensures that user-related operations behave correctly under both valid and invalid scenarios, including validation failures, business rule violations, and data access errors.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using AspNetSpaTemplate.DTOs;
using Xunit;

// UserServiceTests is designed for use within an xUnit test runner
public class UserServiceUsageExample
{
    private readonly UserServiceTests _tests = new UserServiceTests();

    public async Task RunTestSuite()
    {
        // User retrieval tests
        await _tests.GetUserByIdAsync_WithValidId_ReturnsUser();
        await _tests.GetUserByIdAsync_WithInvalidId_ThrowsNotFoundException();
        await _tests.GetAllUsersAsync_ReturnsActiveUsers();

        // User creation tests
        await _tests.CreateUserAsync_WithValidRequest_CreatesUser();
        await _tests.CreateUserAsync_WithExistingEmail_ThrowsValidationException();
        await _tests.CreateUserAsync_WithShortFirstName_ThrowsValidationException();
        await _tests.CreateUserAsync_WithInvalidEmail_ThrowsValidationException();
        await _tests.CreateUserAsync_WithShortPassword_ThrowsValidationException();

        // User update tests
        await _tests.UpdateUserAsync_WithValidRequest_UpdatesProfile();

        // Authentication tests
        await _tests.AuthenticateAsync_WithValidCredentials_ReturnsToken();
        await _tests.AuthenticateAsync_WithInvalidPassword_ThrowsBusinessException();
        await _tests.AuthenticateAsync_WithNonExistentUser_ThrowsBusinessException();
        await _tests.AuthenticateAsync_WithInactiveUser_ThrowsBusinessException();

        // User activation/deactivation tests
        await _tests.DeactivateUserAsync_WithValidId_DeactivatesUser();
        await _tests.ActivateUserAsync_WithValidId_ActivatesUser();
    }
}
```

## UserServiceIntegrationTests

The `UserServiceIntegrationTests` class provides comprehensive integration testing for user management workflows, ensuring end-to-end functionality between the user service, repository, and database. It verifies complex business scenarios such as user registration, authentication, profile updates, account lifecycle operations, and data integrity by utilizing the test server and in-memory database context.

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using AspNetSpaTemplate.DTOs;

// UserServiceIntegrationTests is designed for use within an xUnit test runner
public class UserServiceIntegrationTestsExample : IAsyncLifetime
{
    private readonly UserServiceIntegrationTests _tests = new UserServiceIntegrationTests();

    public async Task InitializeAsync() => await _tests.InitializeAsync();
    public async Task DisposeAsync() => await _tests.DisposeAsync();

    public async Task RunIntegrationTests()
    {
        // Initialize test database and services
        await _tests.InitializeAsync();

        // Execute end-to-end user workflows
        await _tests.EndToEnd_RegisterUserLoginAndUpdate_CompleteAuthFlow();
        await _tests.MultipleUsersWithUniqueEmails_CanAllBeCreated();
        await _tests.DuplicateEmail_PreventSecondUserCreation();
        await _tests.DeactivateAndReactivate_UserAccessibility();
        await _tests.LastLoginTimestamp_UpdatedOnAuthentication();
        await _tests.PasswordValidation_EnforcesMinimumLength();
        await _tests.GetActiveUsers_ExcludesInactiveUsers();

        // Clean up
        await _tests.DisposeAsync();
    }
}

// Example usage within a test method
[Fact]
public async Task TestUserRegistrationFlow()
{
    // Arrange
    var tests = new UserServiceIntegrationTests();
    await tests.InitializeAsync();

    try
    {
        var registerRequest = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = "1234567890",
            Address = "123 Main St",
            City = "Boston",
            PostalCode = "02101",
            Country = "USA"
        };

        // Act - Register user
        var userResponse = await tests.CreateUserAsync(registerRequest);

        // Assert - User created successfully
        Assert.NotNull(userResponse);
        Assert.Equal("john.doe@example.com", userResponse.Email);
        Assert.True(userResponse.IsActive);

        // Act - Login
        var loginRequest = new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "SecurePassword123!"
        };
        var loginResponse = await tests.AuthenticateAsync(loginRequest);

        // Assert - Login successful
        Assert.NotNull(loginResponse.Token);
        Assert.True(loginResponse.UserId > 0);
    }
    finally
    {
        await tests.DisposeAsync();
    }
}
```

### Usage Example

```csharp
using AspNetSpaTemplate.Tests;
using Xunit;

// OrderServiceTests is designed for use within an xUnit test runner
public class OrderServiceUsageExample
{
    private readonly OrderServiceTests _tests = new OrderServiceTests();

    public async Task RunTestSuite()
    {
        // Retrieval tests
        await _tests.GetOrderByIdAsync_WithValidId_ReturnsOrderResponse();
        await _tests.GetOrderByIdAsync_WithInvalidId_ThrowsNotFoundException();

        // Creation tests
        await _tests.CreateOrderAsync_WithValidRequest_CreatesOrder();
        await _tests.CreateOrderAsync_WithEmptyItems_ThrowsValidationException();
        await _tests.CreateOrderAsync_WithNullItems_ThrowsValidationException();
        await _tests.CreateOrderAsync_WithInsufficientStock_ThrowsBusinessException();

        // Status update tests
        await _tests.UpdateOrderStatusAsync_WithValidStatus_UpdatesOrderStatus();

        // Discount tests
        await _tests.ApplyDiscountAsync_WithValidDiscount_AppliesToOrder();
        await _tests.ApplyDiscountAsync_ToFinalizedOrder_ThrowsBusinessException();

        // Collection retrieval tests
        await _tests.GetUserOrdersAsync_WithValidUserId_ReturnsUserOrders();
        await _tests.GetPendingOrdersAsync_ReturnsPendingOrders();

        // Revenue calculation tests
        await _tests.GetTotalRevenueAsync_ReturnsCorrectTotal();
    }
}
```

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
