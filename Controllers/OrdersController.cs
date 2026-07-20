#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for order management.
/// </summary>
public sealed class OrdersController : ApiControllerBase
{
    private readonly OrderService _orderService;
    private readonly ICacheService _cacheService;

    public OrdersController(OrderService orderService, ICacheService cacheService)
    {
        _orderService = orderService;
        _cacheService = cacheService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return ApiSuccess(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateOrderIdempotencyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();

        // Check for idempotency key
        var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var keyHeader) && !string.IsNullOrWhiteSpace(keyHeader)
            ? keyHeader.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            // Idempotency key provided - check cache for existing response
            var cacheKey = $"idempotency:order:{userId}:{idempotencyKey}";
            var cachedResponse = await _cacheService.GetAsync<OrderResponse>(cacheKey);

            if (cachedResponse != null)
            {
                return ApiSuccess(cachedResponse, "Order created successfully (idempotent)", StatusCodes.Status201Created);
            }
        }

        var order = await _orderService.CreateOrderAsync(userId, request);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            // Store response snapshot with 24h TTL
            var cacheKey = $"idempotency:order:{userId}:{idempotencyKey}";
            await _cacheService.SetAsync(cacheKey, order, TimeSpan.FromHours(24));
        }

        return ApiSuccess(order, "Order created successfully", StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, request);
        return ApiSuccess(order, "Order status updated successfully");
    }

    [HttpPost("{id:int}/discount")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyDiscount(int id, [FromBody] ApplyDiscountRequest request)
    {
        var order = await _orderService.ApplyDiscountAsync(id, request);
        return ApiSuccess(order, "Discount applied successfully");
    }

    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return ApiSuccess(orders);
    }

    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return ApiSuccess(orders);
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOrders()
    {
        var orders = await _orderService.GetPendingOrdersAsync();
        return ApiSuccess(orders);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        var order = await _orderService.CancelOrderAsync(id, userId);
        return ApiSuccess(order, "Order cancelled successfully");
    }

    [HttpGet("{id:int}/status-history")]
    [ProducesResponseType(typeof(List<StatusHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatusHistory(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return ApiSuccess(order.StatusHistory);
    }

    [HttpGet("revenue/total")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalRevenue([FromQuery] int? days = null)
    {
        decimal revenue = days.HasValue
            ? await _orderService.GetTotalRevenueAsync(days.Value)
            : await _orderService.GetTotalRevenueAsync();

        return ApiSuccess(revenue);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(OrderStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStats([FromQuery] int? days = null)
    {
        var stats = await _orderService.GetOrderStatsAsync(days);
        return ApiSuccess(stats);
    }
}