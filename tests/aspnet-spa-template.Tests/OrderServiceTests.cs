#nullable enable
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class OrderServiceTests
{
    private readonly Mock<OrderRepository> _mockOrderRepository;
    private readonly Mock<ProductRepository> _mockProductRepository;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<OrderRepository>();
        _mockProductRepository = new Mock<ProductRepository>();
        _orderService = new OrderService(_mockOrderRepository.Object, _mockProductRepository.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithValidId_ReturnsOrderResponse()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, OrderNumber = "ORD-001", Status = OrderStatus.Pending, UserId = 10 };
        _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result?.OrderNumber.Should().Be("ORD-001");
        _mockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = 999;
        _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act
        var act = () => _orderService.GetOrderByIdAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_CreatesOrder()
    {
        // Arrange
        var userId = 5;
        var productId = 10;
        var product = new Product { Id = productId, Name = "Widget", Price = 100m, StockQuantity = 10, IsAvailable = true };
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = productId, Quantity = 2 } },
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            Notes = ""
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockOrderRepository.Setup(r => r.Add(It.IsAny<Order>()));
        _mockOrderRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().StartWith("ORD-");
        result.SubTotal.Should().Be(200m); // 100 * 2
        _mockOrderRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyItems_ThrowsValidationException()
    {
        // Arrange
        var userId = 5;
        var request = new CreateOrderRequest { Items = new List<OrderItemRequest>() };

        // Act
        var act = () => _orderService.CreateOrderAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WithNullItems_ThrowsValidationException()
    {
        // Arrange
        var userId = 5;
        var request = new CreateOrderRequest { Items = null };

        // Act
        var act = () => _orderService.CreateOrderAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ThrowsBusinessException()
    {
        // Arrange
        var userId = 5;
        var productId = 10;
        var product = new Product { Id = productId, Name = "Widget", Price = 100m, StockQuantity = 1, IsAvailable = true };
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = productId, Quantity = 5 } },
            ShippingAddress = "123 Main St",
            BillingAddress = "123 Main St",
            Notes = ""
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var act = () => _orderService.CreateOrderAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidStatus_UpdatesOrderStatus()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, Status = OrderStatus.Pending };
        var request = new UpdateOrderStatusRequest { Status = "Processing", TrackingNumber = null };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockOrderRepository.Setup(r => r.Update(It.IsAny<Order>()));
        _mockOrderRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, request);

        // Assert
        result.Should().NotBeNull();
        _mockOrderRepository.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task ApplyDiscountAsync_WithValidDiscount_AppliesToOrder()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, SubTotal = 100m, TaxAmount = 10m, ShippingCost = 5m, Discount = 0m, Status = OrderStatus.Pending };
        var request = new ApplyDiscountRequest { DiscountAmount = 10m };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockOrderRepository.Setup(r => r.Update(It.IsAny<Order>()));
        _mockOrderRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _orderService.ApplyDiscountAsync(orderId, request);

        // Assert
        result.Should().NotBeNull();
        result.Discount.Should().Be(10m);
        _mockOrderRepository.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task ApplyDiscountAsync_ToFinalizedOrder_ThrowsBusinessException()
    {
        // Arrange
        var orderId = 1;
        var order = new Order { Id = orderId, Status = OrderStatus.Delivered };
        var request = new ApplyDiscountRequest { DiscountAmount = 10m };

        _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var act = () => _orderService.ApplyDiscountAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Cannot apply discount*");
    }

    [Fact]
    public async Task GetUserOrdersAsync_WithValidUserId_ReturnsUserOrders()
    {
        // Arrange
        var userId = 5;
        var orders = new List<Order>
        {
            new Order { Id = 1, UserId = userId, Status = OrderStatus.Pending },
            new Order { Id = 2, UserId = userId, Status = OrderStatus.Processing }
        };
        _mockOrderRepository.Setup(r => r.GetUserOrdersAsync(userId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingOrdersAsync_ReturnsPendingOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = 1, Status = OrderStatus.Pending },
            new Order { Id = 2, Status = OrderStatus.Pending }
        };
        _mockOrderRepository.Setup(r => r.GetPendingOrdersAsync()).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetPendingOrdersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTotalRevenueAsync_ReturnsCorrectTotal()
    {
        // Arrange
        var expectedRevenue = 1000m;
        _mockOrderRepository.Setup(r => r.GetTotalRevenueAsync()).ReturnsAsync(expectedRevenue);

        // Act
        var result = await _orderService.GetTotalRevenueAsync();

        // Assert
        result.Should().Be(expectedRevenue);
    }
}
