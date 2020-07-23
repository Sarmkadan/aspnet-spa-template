// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for order management.
/// </summary>
public class OrdersController : ApiControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id:int}")]
    [ProduceResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return ApiSuccess(order);
    }

    [HttpPost]
    [ProduceResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        var order = await _orderService.CreateOrderAsync(userId, request);
        return ApiSuccess(order, "Order created successfully", StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}/status")]
    [ProduceResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, request);
        return ApiSuccess(order, "Order status updated successfully");
    }

    [HttpPost("{id:int}/discount")]
    [ProduceResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyDiscount(int id, [FromBody] ApplyDiscountRequest request)
    {
        var order = await _orderService.ApplyDiscountAsync(id, request);
        return ApiSuccess(order, "Discount applied successfully");
    }

    [HttpGet("user/{userId:int}")]
    [ProduceResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return ApiSuccess(orders);
    }

    [HttpGet("my-orders")]
    [ProduceResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAuthenticated)
            return Unauthorized();

        var userId = GetUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return ApiSuccess(orders);
    }

    [HttpGet("pending")]
    [ProduceResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOrders()
    {
        var orders = await _orderService.GetPendingOrdersAsync();
        return ApiSuccess(orders);
    }

    [HttpGet("revenue/total")]
    [ProduceResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalRevenue([FromQuery] int? days = null)
    {
        decimal revenue = days.HasValue
            ? await _orderService.GetTotalRevenueAsync(days.Value)
            : await _orderService.GetTotalRevenueAsync();

        return ApiSuccess(revenue);
    }
}
