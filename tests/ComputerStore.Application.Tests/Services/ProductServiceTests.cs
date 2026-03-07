using AutoMapper;
using ComputerStore.Application.Services;
using ComputerStore.Application.Tests.Helpers;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using FluentAssertions;
using Moq;
using Xunit;

namespace ComputerStore.Application.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = TestHelper.CreateMapper();
            _productService = new ProductService(_unitOfWorkMock.Object, _mapper);
        }

        #region GetAllProductsAsync Tests

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            TestHelper.CreateTestProduct(1),
            TestHelper.CreateTestProduct(2),
            TestHelper.CreateTestProduct(3)
        };

            _unitOfWorkMock.Setup(u => u.Products.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            _unitOfWorkMock.Verify(u => u.Products.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_WhenNoProducts_ShouldReturnEmptyList()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Products.GetAllAsync())
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetProductByIdAsync Tests

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var product = TestHelper.CreateTestProduct(1);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Test Product 1");
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetProductDetailsAsync Tests

        [Fact]
        public async Task GetProductDetailsAsync_WithValidId_ShouldReturnProductDetails()
        {
            // Arrange
            var product = TestHelper.CreateTestProduct(1);
            product.Category = TestHelper.CreateTestCategory(1);
            product.Specifications = new List<ProductSpecification>
        {
            new() { Id = 1, ProductId = 1, Name = "CPU", Value = "Intel i7", DisplayOrder = 1 }
        };

            _unitOfWorkMock.Setup(u => u.Products.GetProductWithDetailsAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Specifications.Should().HaveCount(1);
        }

        #endregion

        #region SearchProductsAsync Tests

        [Fact]
        public async Task SearchProductsAsync_WithValidTerm_ShouldReturnMatchingProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            TestHelper.CreateTestProduct(1),
            TestHelper.CreateTestProduct(2)
        };

            _unitOfWorkMock.Setup(u => u.Products.SearchProductsAsync("Test"))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.SearchProductsAsync("Test");

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchProductsAsync_WithEmptyTerm_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            TestHelper.CreateTestProduct(1),
            TestHelper.CreateTestProduct(2),
            TestHelper.CreateTestProduct(3)
        };

            _unitOfWorkMock.Setup(u => u.Products.SearchProductsAsync(""))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.SearchProductsAsync("");

            // Assert
            result.Should().HaveCount(3);
        }

        #endregion

        #region CreateProductAsync Tests

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldReturnCreatedProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Description",
                Price = 99.99m,
                CategoryId = 1,
                StockQuantity = 10
            };

            var createdProduct = TestHelper.CreateTestProduct(1);
            createdProduct.Name = createDto.Name;

            _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            _unitOfWorkMock.Verify(u => u.Products.AddAsync(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UpdateProductAsync Tests

        [Fact]
        public async Task UpdateProductAsync_WithValidData_ShouldReturnUpdatedProduct()
        {
            // Arrange
            var existingProduct = TestHelper.CreateTestProduct(1);
            var updateDto = new UpdateProductDto
            {
                Id = 1,
                Name = "Updated Product",
                Price = 129.99m
            };

            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
                .ReturnsAsync(existingProduct);
            _unitOfWorkMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            _unitOfWorkMock.Verify(u => u.Products.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var updateDto = new UpdateProductDto { Id = 999 };
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.UpdateProductAsync(updateDto);

            // Assert
            result.Should().BeNull();
            _unitOfWorkMock.Verify(u => u.Products.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        #endregion

        #region DeleteProductAsync Tests

        [Fact]
        public async Task DeleteProductAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var product = TestHelper.CreateTestProduct(1);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.DeleteProductAsync(1);

            // Assert
            result.Should().BeTrue();
            product.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteProductAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.DeleteProductAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetFeaturedProductsAsync Tests

        [Fact]
        public async Task GetFeaturedProductsAsync_ShouldReturnFeaturedProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            TestHelper.CreateTestProduct(1),
            TestHelper.CreateTestProduct(2)
        };
            products.ForEach(p => p.IsFeatured = true);

            _unitOfWorkMock.Setup(u => u.Products.GetFeaturedProductsAsync(10))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetFeaturedProductsAsync(10);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.IsFeatured == true);
        }

        #endregion

        #region GetTopRatedProductsAsync Tests

        [Fact]
        public async Task GetTopRatedProductsAsync_ShouldReturnTopRatedProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            TestHelper.CreateTestProduct(1),
            TestHelper.CreateTestProduct(2)
        };

            _unitOfWorkMock.Setup(u => u.Products.GetTopRatedProductsAsync(10))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetTopRatedProductsAsync(10);

            // Assert
            result.Should().HaveCount(2);
        }

        #endregion
    }
}
