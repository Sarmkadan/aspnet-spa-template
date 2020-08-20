#nullable enable
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class OrderServiceIntegrationTests : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private AppDbContext _dbContext = null!;
    private OrderService _orderService = null!;
    private ProductService _productService = null!;
    private OrderRepository _orderRepository = null!;
    private ProductRepository _productRepository = null!;

    public OrderServiceIntegrationTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    public async Task InitializeAsync()
    {
        _dbContext = new AppDbContext(_dbOptions);
        await _dbContext.Database.EnsureCreatedAsync();

        _orderRepository = new OrderRepository(_dbContext);
        _productRepository = new ProductRepository(_dbContext);
        _orderService = new OrderService(_orderRepository, _productRepository, NullLogger<OrderService>.Instance);
        _productService = new ProductService(_productRepository, NullLogger<ProductService>.Instance);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task EndToEnd_CreateProductAndOrder_CompleteWorkflow()
    {
        // Arrange
        var productRequest = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product",
            Price = 100m,
            StockQuantity = 50,
            Category = ProductCategory.Electronics,
            ImageUrl = "https://example.com/img.jpg",
            Sku = "TEST-001"
        };

        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act - Create Product
        var product = await _productService.CreateProductAsync(productRequest);

        // Assert - Product Created
        product.Should().NotBeNull();
        product.Name.Should().Be("Test Product");

        // Act - Create Order
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = product.Id, Quantity = 2 } },
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            Notes = "Please deliver quickly"
        };

        var order = await _orderService.CreateOrderAsync(user.Id, orderRequest);

        // Assert - Order Created
        order.Should().NotBeNull();
        order.SubTotal.Should().Be(200m);
        order.OrderNumber.Should().StartWith("ORD-");

        // Act - Update Order Status
        var updateRequest = new UpdateOrderStatusRequest { Status = "Processing", TrackingNumber = null };
        var updatedOrder = await _orderService.UpdateOrderStatusAsync(product.Id, updateRequest);

        // Assert - Order Status Updated
        updatedOrder.Should().NotBeNull();

        // Act - Apply Discount
        var discountRequest = new ApplyDiscountRequest { DiscountAmount = 10m };
        var discountedOrder = await _orderService.ApplyDiscountAsync(product.Id, discountRequest);

        // Assert - Discount Applied
        discountedOrder.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOrderWithMultipleItems_CalculatesTotalsCorrectly()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);

        var product1 = new Product
        {
            Name = "Widget",
            Price = 100m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        var product2 = new Product
        {
            Name = "Gadget",
            Price = 50m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.AddRange(product1, product2);
        await _dbContext.SaveChangesAsync();

        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = product1.Id, Quantity = 2 },
                new OrderItemRequest { ProductId = product2.Id, Quantity = 1 }
            },
            ShippingAddress = "456 Oak Ave",
            BillingAddress = "456 Oak Ave",
            Notes = ""
        };

        // Act
        var order = await _orderService.CreateOrderAsync(user.Id, orderRequest);

        // Assert
        order.Should().NotBeNull();
        order.SubTotal.Should().Be(250m); // (100 * 2) + (50 * 1)
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task StockReduction_DecreasesProductInventory()
    {
        // Arrange
        var product = new Product
        {
            Name = "Limited Stock",
            Price = 75m,
            StockQuantity = 10,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(product);

        var user = new User
        {
            FirstName = "Bob",
            LastName = "Builder",
            Email = "bob@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = product.Id, Quantity = 3 } },
            ShippingAddress = "789 Pine St",
            BillingAddress = "789 Pine St",
            Notes = ""
        };

        // Act
        await _orderService.CreateOrderAsync(user.Id, orderRequest);

        // Assert
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task InsufficientStock_PreventOrderCreation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Low Stock",
            Price = 50m,
            StockQuantity = 2,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(product);

        var user = new User
        {
            FirstName = "Alice",
            LastName = "Wonder",
            Email = "alice@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = product.Id, Quantity = 10 } },
            ShippingAddress = "321 Cedar",
            BillingAddress = "321 Cedar",
            Notes = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _orderService.CreateOrderAsync(user.Id, orderRequest));
    }

    [Fact]
    public async Task GetUserOrders_ReturnsOnlyUserOrders()
    {
        // Arrange
        var user1 = new User
        {
            FirstName = "User",
            LastName = "One",
            Email = "user1@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new User
        {
            FirstName = "User",
            LastName = "Two",
            Email = "user2@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.AddRange(user1, user2);

        var product = new Product
        {
            Name = "Shared Product",
            Price = 100m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = product.Id, Quantity = 1 } },
            ShippingAddress = "123 Test",
            BillingAddress = "123 Test",
            Notes = ""
        };

        await _orderService.CreateOrderAsync(user1.Id, orderRequest);
        await _orderService.CreateOrderAsync(user2.Id, orderRequest);

        // Act
        var user1Orders = await _orderService.GetUserOrdersAsync(user1.Id);

        // Assert
        user1Orders.Should().HaveCount(1);
    }
}
