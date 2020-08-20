using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSpaTemplate.Benchmarks;

[MemoryDiagnoser]
public class ProductServiceBenchmarks
{
    private AppDbContext _context = null!;
    private ProductService _productService = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "BenchmarksDb")
            .Options;

        _context = new AppDbContext(options);
        var repo = new ProductRepository(_context);
        _productService = new ProductService(repo, NullLogger<ProductService>.Instance);

        // Seed data
        for (int i = 1; i <= 1000; i++)
        {
            _context.Products.Add(new Product { Id = i, Name = $"Product {i}", IsAvailable = true, Price = 10.0m });
        }
        _context.SaveChanges();
    }

    [Benchmark]
    public async Task<List<AspNetSpaTemplate.DTOs.ProductResponse>> GetAllProducts()
    {
        var result = await _productService.GetAllProductsAsync(1, 10);
        return result.Products;
    }

    [Benchmark]
    public async Task<AspNetSpaTemplate.DTOs.ProductResponse?> GetProductById()
    {
        return await _productService.GetProductByIdAsync(1);
    }

    [Benchmark]
    public async Task<List<AspNetSpaTemplate.DTOs.ProductResponse>> GetFeaturedProducts()
    {
        return await _productService.GetFeaturedProductsAsync(10);
    }
}
