#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Constants;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests that verify the DataAnnotations validation behavior of DTOs.
/// </summary>
public sealed class DtoValidationTests
{
    /// <summary>
    /// Validates a fully populated <see cref="CreateProductRequest"/> instance.
    /// </summary>
    [Fact]
    public void CreateProductRequest_ValidObject_PassesValidation()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A product used for unit testing.",
            Price = 19.99m,
            StockQuantity = 10,
            Category = ProductCategory.Food,
            ImageUrl = "https://example.com/image.png",
            Sku = "TP-001"
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    /// <summary>
    /// Validates a fully populated <see cref="CreateOrderRequest"/> instance.
    /// </summary>
    [Fact]
    public void CreateOrderRequest_ValidObject_PassesValidation()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 2 },
                new OrderItemRequest { ProductId = 2, Quantity = 1 }
            },
            ShippingAddress = "123 Test St, Test City",
            BillingAddress = "123 Test St, Test City",
            Notes = "Leave at the front door."
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    /// <summary>
    /// Validates a fully populated <see cref="CreateUserRequest"/> instance.
    /// </summary>
    [Fact]
    public void CreateUserRequest_ValidObject_PassesValidation()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd!",
            PhoneNumber = "+1-555-1234",
            Address = "123 Main St",
            City = "Anytown",
            PostalCode = "12345",
            Country = "USA"
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    /// <summary>
    /// Demonstrates that validation still succeeds when optional fields are omitted.
    /// </summary>
    [Fact]
    public void CreateUserRequest_OnlyRequiredFields_PassesValidation()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "Secret123"
            // Optional fields left null
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}
