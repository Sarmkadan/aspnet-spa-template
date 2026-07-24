#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Controllers;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for pagination parameter validation in ProductsController endpoints.
/// Tests that invalid pagination parameters (page=0, page=-1, pageSize=0, pageSize=-1)
/// are rejected with 400 Bad Request responses rather than being silently clamped.
/// </summary>
public sealed class PaginationValidationTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductsController _controller;

    public PaginationValidationTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    #region GetProducts Validation Tests

    /// <summary>
    /// Tests that GetProducts rejects page=0 with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProducts_PageZero_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProducts(pageNumber: 0, pageSize: 10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page number must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProducts rejects negative page numbers with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProducts_NegativePage_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProducts(pageNumber: -5, pageSize: 10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page number must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProducts rejects pageSize=0 with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProducts_PageSizeZero_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProducts(pageNumber: 1, pageSize: 0) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page size must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProducts rejects negative page size with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProducts_NegativePageSize_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProducts(pageNumber: 1, pageSize: -10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page size must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProducts clamps pageSize > max to max value.
    /// </summary>
    [Fact]
    public async Task GetProducts_PageSizeGreaterThanMax_ClampsToMax()
    {
        // Arrange
        var listResponse = new ProductListResponse
        {
            Products = [new ProductResponse { Id = 1, Name = "Product 1" }],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _productServiceMock
            .Setup(s => s.GetAllProductsAsync(1, 100))
            .ReturnsAsync(listResponse);

        // Act - request pageSize of 2000000 which should be clamped to 100
        var result = await _controller.GetProducts(pageNumber: 1, pageSize: 2000000) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        // Verify service was called with clamped value (100)
        _productServiceMock.Verify(s => s.GetAllProductsAsync(1, 100), Times.Once);

        var successResponse = result.Value as SuccessResponse<PagedResult<ProductResponse>>;
        successResponse.Should().NotBeNull();
        successResponse!.Success.Should().BeTrue();
        successResponse.Data.Should().NotBeNull();
        successResponse.Data!.Items.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that GetProducts accepts valid pagination parameters and returns 200 OK.
    /// </summary>
    [Fact]
    public async Task GetProducts_ValidPagination_Returns200OK()
    {
        // Arrange
        var listResponse = new ProductListResponse
        {
            Products = [new ProductResponse { Id = 1, Name = "Product 1" }],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _productServiceMock
            .Setup(s => s.GetAllProductsAsync(1, 10))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _controller.GetProducts(pageNumber: 1, pageSize: 10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var successResponse = result.Value as SuccessResponse<PagedResult<ProductResponse>>;
        successResponse.Should().NotBeNull();
        successResponse!.Success.Should().BeTrue();
        successResponse.Data.Should().NotBeNull();
        successResponse.Data!.Items.Should().HaveCount(1);

        // Verify service was called with correct parameters
        _productServiceMock.Verify(s => s.GetAllProductsAsync(1, 10), Times.Once);
    }

    #endregion

    #region GetProductsByCategory Validation Tests

    /// <summary>
    /// Tests that GetProductsByCategory rejects page=0 with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_PageZero_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: 0, pageSize: 10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page number must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProductsByCategory rejects negative page numbers with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_NegativePage_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: -3, pageSize: 10) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page number must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProductsByCategory rejects pageSize=0 with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_PageSizeZero_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: 1, pageSize: 0) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page size must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProductsByCategory rejects negative page size with a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_NegativePageSize_Returns400BadRequest()
    {
        // Arrange & Act
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: 1, pageSize: -5) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var errorResponse = result.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Page size must be greater than 0.");
        errorResponse.ErrorCode.Should().Be("VALIDATION_ERROR");

        // Verify service was not called
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that GetProductsByCategory clamps pageSize > max to max value.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_PageSizeGreaterThanMax_ClampsToMax()
    {
        // Arrange
        var listResponse = new ProductListResponse
        {
            Products = [new ProductResponse { Id = 1, Name = "Electronics Product 1" }],
            PageNumber = 2,
            PageSize = 15,
            TotalCount = 25
        };

        _productServiceMock
            .Setup(s => s.GetProductsByCategoryAsync(ProductCategory.Electronics, 2, 100))
            .ReturnsAsync(listResponse);

        // Act - request pageSize of 2000 which should be clamped to 100
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: 2, pageSize: 2000) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        // Verify service was called with clamped value (100)
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(ProductCategory.Electronics, 2, 100),
            Times.Once);

        var successResponse = result.Value as SuccessResponse<PagedResult<ProductResponse>>;
        successResponse.Should().NotBeNull();
        successResponse!.Success.Should().BeTrue();
        successResponse.Data.Should().NotBeNull();
        successResponse.Data!.Items.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that GetProductsByCategory accepts valid pagination parameters and returns 200 OK.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_ValidPagination_Returns200OK()
    {
        // Arrange
        var listResponse = new ProductListResponse
        {
            Products = [new ProductResponse { Id = 1, Name = "Electronics Product 1" }],
            PageNumber = 2,
            PageSize = 15,
            TotalCount = 25
        };

        _productServiceMock
            .Setup(s => s.GetProductsByCategoryAsync(ProductCategory.Electronics, 2, 15))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _controller.GetProductsByCategory("electronics", pageNumber: 2, pageSize: 15) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var successResponse = result.Value as SuccessResponse<PagedResult<ProductResponse>>;
        successResponse.Should().NotBeNull();
        successResponse!.Success.Should().BeTrue();
        successResponse.Data.Should().NotBeNull();
        successResponse.Data!.Items.Should().HaveCount(1);

        // Verify service was called with correct parameters
        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(ProductCategory.Electronics, 2, 15),
            Times.Once);
    }

    #endregion
}
