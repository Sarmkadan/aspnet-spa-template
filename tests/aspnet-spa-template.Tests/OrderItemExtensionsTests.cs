using System;
using AspNetSpaTemplate.Models;
using Xunit;

namespace AspNetSpaTemplate.Tests
{
    public class OrderItemExtensionsTests
    {
        [Fact]
        public void LineTotal_NoDiscount_ReturnsQuantityTimesUnitPrice()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 3,
                UnitPrice = 12.34m,
                Discount = 0m
            };

            // Act
            var total = item.LineTotal();

            // Assert
            var expected = 3 * 12.34m;
            Assert.Equal(Math.Round(expected, 2), total);
        }

        [Fact]
        public void LineTotal_WithDiscount_AppliesDiscountCorrectly()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 2,
                UnitPrice = 20.00m,
                Discount = 2.00m // absolute discount amount
            };

            // Act
            var total = item.LineTotal();

            // Assert
            var expected = (2 * 20.00m) - 2.00m;
            Assert.Equal(Math.Round(expected, 2), total);
        }

        [Fact]
        public void LineTotal_NegativeResult_IsClampedToZero()
        {
            // Arrange: discount larger than subtotal
            var item = new OrderItem
            {
                Quantity = 1,
                UnitPrice = 5.00m,
                Discount = 10.00m
            };

            // Act
            var total = item.LineTotal();

            // Assert
            Assert.Equal(0m, total);
        }

        [Fact]
        public void IsDiscounted_WhenDiscountZero_ReturnsFalse()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 1,
                UnitPrice = 5.00m,
                Discount = 0m
            };

            // Act
            var result = item.IsDiscounted();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsDiscounted_WhenDiscountPositive_ReturnsTrue()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 1,
                UnitPrice = 5.00m,
                Discount = 1.25m
            };

            // Act
            var result = item.IsDiscounted();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ToDisplayString_NoDiscount_FormatsCorrectly()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 4,
                UnitPrice = 7.50m,
                Discount = 0m
            };

            // Act
            var display = item.ToDisplayString();

            // Assert
            var expected = $"4 × {item.UnitPrice:C} = {item.LineTotal():C}";
            Assert.Equal(expected, display);
        }

        [Fact]
        public void ToDisplayString_WithDiscount_FormatsCorrectly()
        {
            // Arrange
            var item = new OrderItem
            {
                Quantity = 2,
                UnitPrice = 15.00m,
                Discount = 3.00m
            };

            // Act
            var display = item.ToDisplayString();

            // Assert
            var expected = $"2 × {item.UnitPrice:C} (−{item.Discount:C}) = {item.LineTotal():C}";
            Assert.Equal(expected, display);
        }
    }
}
