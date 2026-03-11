using AutoMapper;
using ComputerStore.Application.Mappings;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Shared.DTOs;
using ComputerStore.Tests.TestHelpers;
using Moq;
using System.Timers;
using Xunit;

namespace ComputerStore.Tests.Services;

/// <summary>
/// Тесты для ProductService.
/// Покрывают получение, создание, обновление, удаление и поиск продуктов.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new ProductService(_unitOfWorkMock.Object, _mapper);
    }

    // ─── GetAllProductsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            TestDataFactory.CreateProduct(1, "Ноутбук HP"),
            TestDataFactory.CreateProduct(2, "Ноутбук Dell")
        };
        _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _productRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_EmptyRepository_ReturnsEmptyCollection()
    {
        _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        var result = await _sut.GetAllProductsAsync();

        Assert.Empty(result);
    }

    // ─── GetProductByIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsDto()
    {
        var product = TestDataFactory.CreateProduct(42, "Топовый ноутбук");
        _productRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(product);

        var result = await _sut.GetProductByIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Топовый ноутбук", result.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistentProduct_ReturnsNull()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _sut.GetProductByIdAsync(999);

        Assert.Null(result);
    }

    // ─── GetProductDetailsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetProductDetailsAsync_ExistingProduct_ReturnsDetailsDto()
    {
        var product = TestDataFactory.CreateProduct(5);
        product.Specifications = new List<ProductSpecification>
        {
            new() { Id = 1, Name = "RAM", Value = "16GB", DisplayOrder = 1 }
        };
        _productRepoMock.Setup(r => r.GetProductWithDetailsAsync(5)).ReturnsAsync(product);

        var result = await _sut.GetProductDetailsAsync(5);

        Assert.NotNull(result);
        Assert.Single(result.Specifications);
    }

    [Fact]
    public async Task GetProductDetailsAsync_NonExistentProduct_ReturnsNull()
    {
        _productRepoMock.Setup(r => r.GetProductWithDetailsAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _sut.GetProductDetailsAsync(404);

        Assert.Null(result);
    }

    // ─── GetProductsByCategoryAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsProductsForCategory()
    {
        var products = new List<Product>
        {
            TestDataFactory.CreateProduct(1, categoryId: 3),
            TestDataFactory.CreateProduct(2, categoryId: 3)
        };
        _productRepoMock.Setup(r => r.GetProductsByCategoryAsync(3)).ReturnsAsync(products);

        var result = await _sut.GetProductsByCategoryAsync(3);

        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(3, p.CategoryId));
    }

    // ─── SearchProductsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SearchProductsAsync_WithValidTerm_ReturnsMatchingProducts()
    {
        var products = new List<Product> { TestDataFactory.CreateProduct(1, "Ноутбук ASUS") };
        _productRepoMock.Setup(r => r.SearchProductsAsync("asus")).ReturnsAsync(products);

        var result = await _sut.SearchProductsAsync("asus");

        Assert.Single(result);
        Assert.Contains("ASUS", result.First().Name);
    }

    // ─── GetFeaturedProductsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedProductsAsync_ReturnsRequestedCount()
    {
        var products = Enumerable.Range(1, 5)
            .Select(i => { var p = TestDataFactory.CreateProduct(i); p.IsFeatured = true; return p; })
            .ToList();
        _productRepoMock.Setup(r => r.GetFeaturedProductsAsync(5)).ReturnsAsync(products);

        var result = await _sut.GetFeaturedProductsAsync(5);

        Assert.Equal(5, result.Count());
    }

    // ─── CreateProductAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateProductAsync_ValidDto_CreatesAndReturnsProduct()
    {
        var dto = TestDataFactory.CreateProductDto("Новый SSD", 5999m);
        var createdProduct = TestDataFactory.CreateProduct(10, dto.Name, dto.Price);

        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync(new ComputerStore.Domain.Entities.Product());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(createdProduct);

        var result = await _sut.CreateProductAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Новый SSD", result.Name);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ─── UpdateProductAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProductAsync_ExistingProduct_UpdatesAndReturns()
    {
        var existing = TestDataFactory.CreateProduct(7, "Старое название");
        var updateDto = new UpdateProductDto { Id = 7, Name = "Новое название", Price = 1500m, CategoryId = 1 };
        var updated = TestDataFactory.CreateProduct(7, "Новое название", 1500m);

        _productRepoMock.SetupSequence(r => r.GetByIdAsync(7))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateProductAsync(7, updateDto);

        Assert.NotNull(result);
        Assert.Equal("Новое название", result.Name);
        _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_NonExistentProduct_ReturnsNull()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateProductAsync(999, new UpdateProductDto());

        Assert.Null(result);
    }

    // ─── DeleteProductAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_ReturnsTrueAndSoftDeletes()
    {
        var product = TestDataFactory.CreateProduct(3);
        _productRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.SoftDeleteAsync(product)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteProductAsync(3);

        Assert.True(result);
        _productRepoMock.Verify(r => r.SoftDeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_NonExistentProduct_ReturnsFalse()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteProductAsync(999);

        Assert.False(result);
        _productRepoMock.Verify(r => r.SoftDeleteAsync(It.IsAny<Product>()), Times.Never);
    }

    // ─── IncrementViewCountAsync ────────────────────────────────────────────

    [Fact]
    public async Task IncrementViewCountAsync_ExistingProduct_IncrementsCounter()
    {
        var product = TestDataFactory.CreateProduct(1);
        var initialCount = product.ViewCount;
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.IncrementViewCountAsync(1);

        Assert.Equal(initialCount + 1, product.ViewCount);
        _productRepoMock.Verify(r => r.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task IncrementViewCountAsync_NonExistentProduct_DoesNotThrow()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var exception = await Record.ExceptionAsync(() => _sut.IncrementViewCountAsync(999));

        Assert.Null(exception);
        _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    // ─── Маппинг DTO ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllProductsAsync_MapsDiscountAndPriceFieldsCorrectly()
    {
        var product = TestDataFactory.CreateProduct(1, price: 2000m, discountPrice: 1500m);
        _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { product });

        var result = (await _sut.GetAllProductsAsync()).First();

        Assert.Equal(1500m, result.FinalPrice);
        Assert.True(result.HasDiscount);
        Assert.Equal(25m, result.DiscountPercentage);
    }
}