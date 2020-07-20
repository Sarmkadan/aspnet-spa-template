# ASP.NET SPA Template

A production-ready template for building modern Single Page Applications with ASP.NET Core backend and vanilla JavaScript frontend. No React, Vue, or Angular—just clean, semantic HTML, CSS, and JavaScript with a powerful RESTful API backend.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)
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

### DevOps & Infrastructure
- ✅ Docker support (Dockerfile included)
- ✅ Docker Compose for multi-container setup
- ✅ GitHub Actions CI/CD workflow
- ✅ Configurable environment support
- ✅ Health check endpoints
- ✅ Development/Production configurations

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

### 3. Make Your First API Call

```bash
curl https://localhost:7001/api/products
```

### 4. Create Your First Product

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

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AspNetSpaTemplate;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  }
}
```

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001
ConnectionStrings__DefaultConnection=...
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
