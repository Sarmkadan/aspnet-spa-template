#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Controllers;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using System;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Tests for order creation idempotency functionality.
/// </summary>
public sealed class OrderControllerIdempotencyTests
{
    private readonly Mock<OrderService> _orderServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<HttpRequest> _httpRequestMock;
    private readonly OrdersController _controller;
    private readonly DefaultHttpContext _httpContext;

    public OrderControllerIdempotencyTests()
    {
        _orderServiceMock = new Mock<OrderService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _httpRequestMock = new Mock<HttpRequest>();

        _httpContext = new DefaultHttpContext();

        // Setup controller with mocked services
        _controller = new OrdersController(_orderServiceMock.Object, _cacheServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    /// <summary>
    /// Tests that CreateOrder returns cached response when idempotency key is provided and response exists in cache.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidIdempotencyKey_WhenResponseCached_ReturnsCachedResponse()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            ShippingAddress = "123 Test St"
        };

        var cachedOrder = new OrderResponse
        {
            Id = 1,
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 100m,
            Items = new List<OrderItemResponse> { new() { ProductId = 1, Quantity = 2, Total = 100m } }
        };

        var idempotencyKey = Guid.NewGuid().ToString();

        _httpContext.Request.Headers.Clear();
        _httpContext.Request.Headers["Idempotency-Key"] = idempotencyKey;

        var cacheKey = $"idempotency:order:1:{idempotencyKey}";
        _cacheServiceMock.Setup(c => c.GetAsync<OrderResponse>(cacheKey))
            .ReturnsAsync(cachedOrder);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult?.StatusCode.Should().Be(StatusCodes.Status201Created);
        okResult?.Value.Should().BeEquivalentTo(new SuccessResponse<OrderResponse>(cachedOrder, "Order created successfully (idempotent)"));

        _orderServiceMock.Verify(o => o.CreateOrderAsync(It.IsAny<int>(), It.IsAny<CreateOrderRequest>()), Times.Never);
    }

    /// <summary>
    /// Tests that CreateOrder stores response in cache when idempotency key is provided.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidIdempotencyKey_StoresResponseInCache()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            ShippingAddress = "123 Test St"
        };

        var createdOrder = new OrderResponse
        {
            Id = 1,
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 100m,
            Items = new List<OrderItemResponse> { new() { ProductId = 1, Quantity = 2, Total = 100m } }
        };

        var idempotencyKey = Guid.NewGuid().ToString();

        _httpContext.Request.Headers.Clear();
        _httpContext.Request.Headers["Idempotency-Key"] = idempotencyKey;

        _orderServiceMock.Setup(o => o.CreateOrderAsync(It.IsAny<int>(), request))
            .ReturnsAsync(createdOrder);

        var cacheKey = $"idempotency:order:1:{idempotencyKey}";
        _cacheServiceMock.Setup(c => c.SetAsync(cacheKey, createdOrder, TimeSpan.FromHours(24)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult?.StatusCode.Should().Be(StatusCodes.Status201Created);

        _cacheServiceMock.Verify(c => c.SetAsync(cacheKey, createdOrder, TimeSpan.FromHours(24)), Times.Once);
    }

    /// <summary>
    /// Tests that CreateOrder works normally when no idempotency key is provided.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithoutIdempotencyKey_WorksNormally()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            ShippingAddress = "123 Test St"
        };

        var createdOrder = new OrderResponse
        {
            Id = 1,
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 100m,
            Items = new List<OrderItemResponse> { new() { ProductId = 1, Quantity = 2, Total = 100m } }
        };

        // No idempotency key - clear headers
        _httpContext.Request.Headers.Clear();

        _orderServiceMock.Setup(o => o.CreateOrderAsync(It.IsAny<int>(), request))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult?.StatusCode.Should().Be(StatusCodes.Status201Created);
        okResult?.Value.Should().BeEquivalentTo(new SuccessResponse<OrderResponse>(createdOrder, "Order created successfully"));

        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OrderResponse>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    /// <summary>
    /// Tests that CreateOrder works with empty idempotency key.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithEmptyIdempotencyKey_WorksNormally()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            ShippingAddress = "123 Test St"
        };

        var createdOrder = new OrderResponse
        {
            Id = 1,
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 100m,
            Items = new List<OrderItemResponse> { new() { ProductId = 1, Quantity = 2, Total = 100m } }
        };

        _httpContext.Request.Headers.Clear();
        _httpContext.Request.Headers["Idempotency-Key"] = string.Empty;

        _orderServiceMock.Setup(o => o.CreateOrderAsync(It.IsAny<int>(), request))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult?.StatusCode.Should().Be(StatusCodes.Status201Created);

        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OrderResponse>(), It.IsAny<TimeSpan>()), Times.Never);
    }
}