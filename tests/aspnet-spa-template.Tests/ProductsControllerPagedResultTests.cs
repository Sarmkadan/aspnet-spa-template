#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Controllers;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Tests asserting that the paginated product endpoints share an identical
/// <see cref="PagedResult{T}"/> envelope shape, and that category filtering
/// is case-insensitive and never yields a 404 for an unknown category.
/// </summary>
public sealed class ProductsControllerPagedResultTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductsController _controller;

    public ProductsControllerPagedResultTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    private static ProductListResponse BuildListResponse(int count, int pageNumber, int pageSize, int totalCount)
    {
        var products = Enumerable.Range(1, count)
            .Select(i => new ProductResponse { Id = i, Name = $"Product {i}" })
            .ToList();

        return new ProductListResponse
        {
            Products = products,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// GetProducts and GetProductsByCategory must both return a <see cref="PagedResult{T}"/>
    /// with the same set of fields populated, wrapped in the standard success envelope.
    /// </summary>
    [Fact]
    public async Task GetProducts_And_GetProductsByCategory_ReturnIdenticalEnvelopeShape()
    {
        // Arrange
        var listResponse = BuildListResponse(count: 2, pageNumber: 1, pageSize: 10, totalCount: 20);

        _productServiceMock
            .Setup(s => s.GetAllProductsAsync(1, 10))
            .ReturnsAsync(listResponse);

        _productServiceMock
            .Setup(s => s.GetProductsByCategoryAsync(Constants.ProductCategory.Electronics, 1, 10))
            .ReturnsAsync(listResponse);

        var pagination = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var allResult = await _controller.GetProducts(pagination) as ObjectResult;
        var categoryResult = await _controller.GetProductsByCategory("electronics", pagination) as ObjectResult;

        // Assert
        allResult.Should().NotBeNull();
        categoryResult.Should().NotBeNull();

        var allPage = ((SuccessResponse<PagedResult<ProductResponse>>)allResult!.Value!).Data;
        var categoryPage = ((SuccessResponse<PagedResult<ProductResponse>>)categoryResult!.Value!).Data;

        allPage.Should().NotBeNull();
        categoryPage.Should().NotBeNull();

        allPage!.Items.Count.Should().Be(categoryPage!.Items.Count);
        allPage.Page.Should().Be(categoryPage.Page);
        allPage.PageSize.Should().Be(categoryPage.PageSize);
        allPage.TotalCount.Should().Be(categoryPage.TotalCount);
        allPage.TotalPages.Should().Be(categoryPage.TotalPages);
        allPage.HasNext.Should().Be(categoryPage.HasNext);
    }

    /// <summary>
    /// Category lookup must be case-insensitive: "ELECTRONICS" must resolve the same
    /// way as "Electronics".
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_IsCaseInsensitive()
    {
        // Arrange
        var listResponse = BuildListResponse(count: 1, pageNumber: 1, pageSize: 10, totalCount: 1);

        _productServiceMock
            .Setup(s => s.GetProductsByCategoryAsync(Constants.ProductCategory.Electronics, 1, 10))
            .ReturnsAsync(listResponse);

        var pagination = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _controller.GetProductsByCategory("ELECTRONICS", pagination) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var page = ((SuccessResponse<PagedResult<ProductResponse>>)result.Value!).Data;
        page!.Items.Should().HaveCount(1);

        _productServiceMock.Verify(s => s.GetProductsByCategoryAsync(Constants.ProductCategory.Electronics, 1, 10), Times.Once);
    }

    /// <summary>
    /// An unrecognized category must return 200 with an empty <see cref="PagedResult{T}.Items"/>
    /// collection rather than 404.
    /// </summary>
    [Fact]
    public async Task GetProductsByCategory_UnknownCategory_Returns200WithEmptyItems()
    {
        // Arrange
        var pagination = new PaginationRequest { PageNumber = 2, PageSize = 5 };

        // Act
        var result = await _controller.GetProductsByCategory("not-a-real-category", pagination) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var page = ((SuccessResponse<PagedResult<ProductResponse>>)result.Value!).Data;
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(5);

        _productServiceMock.Verify(
            s => s.GetProductsByCategoryAsync(It.IsAny<Constants.ProductCategory>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Featured products must remain a plain, unpaged array rather than a <see cref="PagedResult{T}"/>.
    /// </summary>
    [Fact]
    public async Task GetFeaturedProducts_ReturnsPlainArray_NotPagedResult()
    {
        // Arrange
        var featured = new List<ProductResponse> { new() { Id = 1, Name = "Featured 1" } };
        _productServiceMock.Setup(s => s.GetFeaturedProductsAsync(10)).ReturnsAsync(featured);

        // Act
        var result = await _controller.GetFeaturedProducts() as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        var data = ((SuccessResponse<List<ProductResponse>>)result!.Value!).Data;
        data.Should().BeEquivalentTo(featured);
    }
}
