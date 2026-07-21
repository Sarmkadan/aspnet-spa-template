#nullable enable
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Unit tests for <see cref="OrderService"/> that verify individual service methods in isolation.
/// Uses in-memory database for testing repository interactions.
/// </summary>
public sealed class OrderServiceUnitTests : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private AppDbContext _dbContext = null!;
    private OrderService _orderService = null!;
    private OrderRepository _orderRepository = null!;
    private ProductRepository _productRepository = null!;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private User _testUser = null!;
    private Product _testProduct = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderServiceUnitTests"/> class.
    /// Configures in-memory database with a unique name for each test run.
    /// </summary>
    public OrderServiceUnitTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"UnitTestDb_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Initializes test database and services before each test.
    /// Creates in-memory database, initializes repositories and services.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        _dbContext = new AppDbContext(_dbOptions);
        await _dbContext.Database.EnsureCreatedAsync();

        _orderRepository = new OrderRepository(_dbContext);
        _productRepository = new ProductRepository(_dbContext);
        _orderService = new OrderService(_orderRepository, _productRepository, NullLogger<OrderService>.Instance, _mockHttpContextAccessor.Object);

        // Create test user
        _testUser = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(_testUser);

        // Create test product
        _testProduct = new Product
        {
            Name = "Test Product",
            Description = "A test product for unit tests",
            Price = 100.00m,
            StockQuantity = 50,
            IsAvailable = true,
            Sku = "TEST-001",
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(_testProduct);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Cleans up test database after each test.
    /// Deletes the in-memory database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }

    [Fact]
    /// <summary>
    /// Tests happy path for creating an order with valid items.
    /// Verifies that order is created successfully with correct calculations.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_HappyPath_OrderCreatedSuccessfully()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 2 },
                new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 3 }
            },
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            Notes = "Please deliver quickly"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(_testUser.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(OrderStatus.Pending.ToDisplayName());
        result.SubTotal.Should().Be(500.00m); // (100 * 2) + (100 * 3)
        result.TaxAmount.Should().BeGreaterThan(0);
        result.Total.Should().BeGreaterThan(result.SubTotal);
        result.OrderNumber.Should().StartWith("ORD-");
        result.ShippingAddress.Should().Be("123 Main St");
        result.Items.Should().HaveCount(2);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(100.00m);
        result.Items[0].Total.Should().BeGreaterThan(0);
    }

    [Fact]
    /// <summary>
    /// Tests that order creation fails when request is null.
    /// Verifies proper argument validation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        CreateOrderRequest? nullRequest = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _orderService.CreateOrderAsync(_testUser.Id, nullRequest!));
    }

    [Fact]
    /// <summary>
    /// Tests that order creation fails when user ID is invalid.
    /// Verifies proper argument validation for user ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_InvalidUserId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 1 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _orderService.CreateOrderAsync(0, request));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _orderService.CreateOrderAsync(-1, request));
    }

    [Fact]
    /// <summary>
    /// Tests that order creation fails when items list is empty.
    /// Verifies validation exception for empty items.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_EmptyItemsList_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _orderService.CreateOrderAsync(_testUser.Id, request));
        exception.Message.Should().Contain("at least one item");
    }

    [Fact]
    /// <summary>
    /// Tests that order creation fails when product is not found.
    /// Verifies NotFoundException for missing product.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 99999, Quantity = 1 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.CreateOrderAsync(_testUser.Id, request));
    }

    [Fact]
    /// <summary>
    /// Tests that order creation fails when there's insufficient stock.
    /// Verifies BusinessException for insufficient stock.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_InsufficientStock_ThrowsBusinessException()
    {
        // Arrange - reduce stock to 0
        _testProduct.StockQuantity = 0;
        _dbContext.Products.Update(_testProduct);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 1 } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _orderService.CreateOrderAsync(_testUser.Id, request));
        exception.Message.Should().Contain("Insufficient stock");
        exception.ErrorCode.Should().Be("INSUFFICIENT_STOCK");
    }

    [Fact]
    /// <summary>
    /// Tests totals calculation for order with multiple items and different tax rates.
    /// Verifies correct subtotal, tax, and total calculations.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_TotalsCalculation_CalculatesCorrectly()
    {
        // Arrange - create products with different categories and tax rates
        var product1 = new Product
        {
            Name = "Electronics Product",
            Price = 200.00m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        var product2 = new Product
        {
            Name = "Clothing Product",
            Price = 50.00m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Clothing,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.AddRange(product1, product2);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = product1.Id, Quantity = 2 }, // 200 * 2 = 400
                new OrderItemRequest { ProductId = product2.Id, Quantity = 3 }  // 50 * 3 = 150
            },
            ShippingAddress = "Test Address",
            BillingAddress = "Test Address"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(_testUser.Id, request);

        // Assert - verify calculations
        result.Should().NotBeNull();
        result.SubTotal.Should().Be(550.00m); // 400 + 150

        // Electronics tax rate is 15% (0.15), Clothing tax rate is 10% (0.10)
        var expectedTax = (200 * 2 * 0.15m) + (50 * 3 * 0.10m); // 60 + 15 = 75
        result.TaxAmount.Should().BeApproximately(expectedTax, 0.01m);

        // Total = SubTotal + Tax + Shipping - Discount (default 0)
        var expectedTotal = result.SubTotal + result.TaxAmount + 0 - 0;
        result.Total.Should().BeApproximately(expectedTotal, 0.01m);
    }

    [Fact]
    /// <summary>
    /// Tests that order retrieval by ID returns correct order when it exists.
    /// Verifies successful retrieval and proper mapping to response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetOrderById_OrderExists_ReturnsOrderResponse()
    {
        // Arrange - create an order first
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 1 } },
            ShippingAddress = "123 Test St",
            BillingAddress = "123 Test St"
        };
        var createdOrder = await _orderService.CreateOrderAsync(_testUser.Id, orderRequest);
        var orderId = createdOrder.Id;

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.OrderNumber.Should().Be(createdOrder.OrderNumber);
        result.Status.Should().Be(OrderStatus.Pending.ToDisplayName());
        result.SubTotal.Should().Be(100.00m);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductId.Should().Be(_testProduct.Id);
    }

    [Fact]
    /// <summary>
    /// Tests that order retrieval throws NotFoundException when order doesn't exist.
    /// Verifies proper exception handling for non-existent orders.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetOrderById_OrderNotFound_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.GetOrderByIdAsync(99999));
    }

    [Fact]
    /// <summary>
    /// Tests that order retrieval throws ArgumentOutOfRangeException for invalid order ID.
    /// Verifies proper validation of order ID parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetOrderById_InvalidOrderId_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _orderService.GetOrderByIdAsync(0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _orderService.GetOrderByIdAsync(-1));
    }

    [Fact]
    /// <summary>
    /// Tests that invalid items in order request are properly validated.
    /// Verifies that each item is validated before processing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_InvalidItemQuantity_ThrowsBusinessException()
    {
        // Arrange - create a product with limited stock
        var limitedProduct = new Product
        {
            Name = "Limited Product",
            Price = 50.00m,
            StockQuantity = 5,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(limitedProduct);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = limitedProduct.Id, Quantity = 10 } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _orderService.CreateOrderAsync(_testUser.Id, request));
        exception.Message.Should().Contain("Insufficient stock");
    }

    [Fact]
    /// <summary>
    /// Tests order creation with single item.
    /// Verifies correct handling of single-item orders.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_SingleItem_OrderCreatedSuccessfully()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 1 } },
            ShippingAddress = "Single Item Address",
            BillingAddress = "Single Item Address"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(_testUser.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.SubTotal.Should().Be(100.00m);
        result.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    /// <summary>
    /// Tests that order items are properly mapped in the response.
    /// Verifies that item details are correctly returned.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrder_ItemDetailsMappedCorrectly()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = _testProduct.Id, Quantity = 2 } },
            ShippingAddress = "Test Address",
            BillingAddress = "Test Address"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(_testUser.Id, request);

        // Assert
        result.Items.Should().HaveCount(1);
        var item = result.Items[0];
        item.ProductId.Should().Be(_testProduct.Id);
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(100.00m);
        item.TaxAmount.Should().BeGreaterThan(0);
        item.Total.Should().BeGreaterThan(0);
        item.ProductName.Should().Be(_testProduct.Name);
    }
}
