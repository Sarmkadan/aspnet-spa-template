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

/// <summary>
/// Extension methods for <see cref="OrderServiceIntegrationTests"/> that provide reusable test utilities
/// for order service integration testing scenarios.
/// </summary>
public static class OrderServiceIntegrationTestsExtensions
{
    /// <summary>
    /// Creates a test user with the specified email and adds it to the database context.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>The created user entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
    public static async Task<User> CreateTestUserAsync(this OrderServiceIntegrationTests tests, AppDbContext dbContext, string email)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = "test-hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Creates a test product with the specified parameters and adds it to the database context.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="name">The product name.</param>
    /// <param name="price">The product price.</param>
    /// <param name="stockQuantity">The initial stock quantity.</param>
    /// <param name="category">The product category.</param>
    /// <returns>The created product entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
    public static async Task<Product> CreateTestProductAsync(
        this OrderServiceIntegrationTests tests,
        AppDbContext dbContext,
        string name,
        decimal price,
        int stockQuantity,
        ProductCategory category = ProductCategory.Electronics)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var product = new Product
        {
            Name = name,
            Description = "Test product description",
            Price = price,
            StockQuantity = stockQuantity,
            IsAvailable = true,
            Category = category,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    /// <summary>
    /// Creates a test order with the specified user and items, and verifies the order was created successfully.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="orderService">The order service.</param>
    /// <param name="userId">The user ID to create the order for.</param>
    /// <param name="items">The order items to include.</param>
    /// <param name="shippingAddress">The shipping address.</param>
    /// <param name="billingAddress">The billing address.</param>
    /// <returns>The created order response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orderService"/> is null.</exception>
    public static async Task<OrderResponse> CreateTestOrderAsync(
        this OrderServiceIntegrationTests tests,
        OrderService orderService,
        int userId,
        IReadOnlyList<OrderItemRequest> items,
        string shippingAddress = "Test Shipping Address",
        string billingAddress = "Test Billing Address")
    {
        ArgumentNullException.ThrowIfNull(orderService);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(shippingAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(billingAddress);

        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>(items),
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Notes = "Test order created via extension method"
        };

        var order = await orderService.CreateOrderAsync(userId, orderRequest);
        order.Should().NotBeNull();

        return order;
    }

    /// <summary>
    /// Asserts that a product's stock quantity has been reduced by the expected amount after order creation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="productId">The product ID to check.</param>
    /// <param name="productRepository">The product repository.</param>
    /// <param name="expectedReduction">The expected reduction in stock quantity.</param>
    /// <returns>The updated product entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="productRepository"/> is null.</exception>
    public static async Task<Product> AssertStockReductionAsync(
        this OrderServiceIntegrationTests tests,
        int productId,
        ProductRepository productRepository,
        int expectedReduction)
    {
        ArgumentNullException.ThrowIfNull(productRepository);

        var product = await productRepository.GetByIdAsync(productId);
        product.Should().NotBeNull();

        product!.StockQuantity.Should().BeGreaterThanOrEqualTo(expectedReduction);

        return product;
    }
}