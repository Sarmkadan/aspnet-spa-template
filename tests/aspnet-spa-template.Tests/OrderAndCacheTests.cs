// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public class OrderAndCacheTests
{
    [Fact]
    public void MarkAsProcessing_WhenStatusIsPending_ChangesStatusToProcessing()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending };

        // Act
        order.MarkAsProcessing();

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void RecalculateTotal_WhenDiscountExceedsSubTotal_ClampsTotalToZero()
    {
        // Arrange
        var order = new Order { SubTotal = 10m, TaxAmount = 1m, ShippingCost = 0m };

        // Act — discount of 50 exceeds the 11 subtotal+tax, total cannot go negative
        order.ApplyDiscount(50m);

        // Assert
        order.Total.Should().Be(0m);
    }

    [Fact]
    public async Task GetUserAsync_WithValidUserId_CallsGetAsyncWithCorrectCacheKey()
    {
        // Arrange
        var expectedKey = "user:id:7";
        var expectedUser = new User { Id = 7, FirstName = "Test" };
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<User>(expectedKey))
                 .ReturnsAsync(expectedUser);

        // Act
        var result = await mockCache.Object.GetUserAsync<User>(7);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
        mockCache.Verify(c => c.GetAsync<User>(expectedKey), Times.Once);
    }

    [Fact]
    public async Task CacheUserAsync_WithValidUser_CallsSetAsyncWithCorrectKey()
    {
        // Arrange
        var userId = 42;
        var expectedKey = "user:id:42";
        var user = new User { Id = userId, FirstName = "Jane" };
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.SetAsync(expectedKey, user, It.IsAny<TimeSpan?>()))
                 .Returns(Task.CompletedTask);

        // Act
        await mockCache.Object.CacheUserAsync(userId, user);

        // Assert
        mockCache.Verify(c => c.SetAsync(expectedKey, user, It.IsAny<TimeSpan?>()), Times.Once);
    }
}
