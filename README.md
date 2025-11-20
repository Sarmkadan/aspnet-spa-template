# AspNet SPA Template

A production-ready e-commerce platform demonstrating best practices for building a Single Page Application (SPA) with vanilla JavaScript and a robust ASP.NET backend API.

## Architecture

### Backend (.NET 10)
- **Framework**: ASP.NET Core Web API
- **Database**: Entity Framework Core with SQL Server
- **Architecture**: Layered architecture with clear separation of concerns

### Frontend
- **Technology**: Vanilla JavaScript (no frameworks)
- **UI**: Clean, responsive HTML5 and CSS3
- **Communication**: Fetch API for REST calls

## Features

### Core Features
- User authentication and authorization
- Product catalog with search and filtering
- Shopping cart functionality
- Order management system
- Product reviews and ratings
- Admin-friendly product management

### Technical Highlights
- Exception handling middleware
- Request/response logging
- RESTful API design
- Dependency injection
- Repository pattern for data access
- Service layer for business logic
- DTO pattern for data transfer
- Comprehensive validation
- Pagination support
- SPA routing without server-side page rendering

## Project Structure

```
aspnet-spa-template/
├── Controllers/              # API endpoints
├── Data/                    # Database context and repositories
├── DTOs/                    # Data transfer objects
├── Exceptions/              # Custom exceptions
├── Middleware/              # HTTP middleware
├── Models/                  # Domain entities
├── Services/                # Business logic
├── Constants/               # Enums and constants
├── wwwroot/                # Frontend assets
│   ├── css/
│   ├── js/
│   └── index.html
├── Program.cs              # Application entry point
├── AspNetSpaTemplate.csproj # Project configuration
└── README.md
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (local or cloud instance)
- A modern web browser

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/vladyslavzaiets/aspnet-spa-template.git
   cd aspnet-spa-template
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   Edit `appsettings.json` and update the `DefaultConnection`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=AspNetSpaTemplate;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```

4. **Create the database**
   ```bash
   dotnet ef database create
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

   The application will be available at `https://localhost:5001`

## API Endpoints

### Authentication
- `POST /api/v1/users/register` - Create a new user account
- `POST /api/v1/users/login` - Authenticate user and get token

### Users
- `GET /api/v1/users/{id}` - Get user details
- `GET /api/v1/users/profile` - Get current user profile
- `PUT /api/v1/users/{id}` - Update user profile

### Products
- `GET /api/v1/products` - List all products (paginated)
- `GET /api/v1/products/{id}` - Get product details
- `GET /api/v1/products/category/{category}` - Filter by category
- `GET /api/v1/products/featured` - Get featured products
- `GET /api/v1/products/search?searchTerm=...` - Search products
- `POST /api/v1/products` - Create product (admin)
- `PUT /api/v1/products/{id}` - Update product (admin)

### Orders
- `GET /api/v1/orders/{id}` - Get order details
- `POST /api/v1/orders` - Create a new order
- `GET /api/v1/orders/my-orders` - Get user's orders
- `PUT /api/v1/orders/{id}/status` - Update order status

### Reviews
- Similar endpoints for product reviews

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Database Models

### User
- Id, FirstName, LastName, Email, PasswordHash
- PhoneNumber, Address, City, PostalCode, Country
- IsActive, IsEmailVerified, CreatedAt, UpdatedAt, LastLoginAt

### Product
- Id, Name, Description, Price, StockQuantity
- Category, ImageUrl, Sku, Rating, ReviewCount
- IsAvailable, IsFeatured, CreatedAt, UpdatedAt

### Order
- Id, UserId, OrderNumber, Status, SubTotal
- TaxAmount, ShippingCost, Discount, Total
- ShippingAddress, BillingAddress, Notes
- OrderedAt, ShippedAt, DeliveredAt, CancelledAt

### OrderItem
- Id, OrderId, ProductId, Quantity
- UnitPrice, TaxAmount, Discount, Total

### Review
- Id, ProductId, UserId, Rating, Title, Content
- HelpfulCount, IsVerifiedPurchase, IsApproved
- CreatedAt, UpdatedAt

## Development

### Key Classes

**Services**
- `UserService` - User management and authentication
- `ProductService` - Product catalog management
- `OrderService` - Order processing and fulfillment
- `ReviewService` - Review management

**Repositories**
- `UserRepository` - User data access
- `ProductRepository` - Product data access
- `OrderRepository` - Order data access
- `RepositoryBase<T>` - Generic CRUD operations

**Middleware**
- `ExceptionHandlingMiddleware` - Global exception handling
- `LoggingMiddleware` - Request/response logging

## Best Practices

1. **Clean Architecture** - Separation of concerns with distinct layers
2. **Entity Framework Core** - ORM for data access
3. **Repository Pattern** - Abstract data access logic
4. **Dependency Injection** - Built-in ASP.NET Core DI
5. **DTOs** - Transfer data between API and clients
6. **Exception Handling** - Custom exceptions and middleware
7. **Validation** - Input validation at service layer
8. **Logging** - Request/response tracking

## Security

- Passwords are hashed using BCrypt
- SQL injection prevention through parameterized queries
- CORS configured for origin validation
- Request validation and error handling
- Authentication and authorization checks

## Licensing

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

## Author

**Vladyslav Zaiets**
- Website: https://sarmkadan.com
- Software Architect & CTO

---

Built with passion for clean, production-grade code.
