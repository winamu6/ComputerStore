using AutoMapper;
using ComputerStore.Application.Mappings;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Shared.DTOs;
using ComputerStore.Tests.TestHelpers;
using Moq;
using Xunit;

namespace ComputerStore.Tests.Services;

/// <summary>
/// Тесты для CartService.
/// Покрывают добавление, обновление, удаление товаров и расчёт стоимости корзины.
/// </summary>
public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly IMapper _mapper;
    private readonly CartService _sut;

    private const string UserId = "test-user-001";

    public CartServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _cartRepoMock = new Mock<ICartRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new CartService(_unitOfWorkMock.Object, _mapper);
    }

    // ─── GetCartAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetCartAsync_WithItems_ReturnsCartWithCorrectTotals()
    {
        var product = TestDataFactory.CreateProduct(1, price: 500m);
        var cartItems = new List<CartItem>
        {
            TestDataFactory.CreateCartItem(1, UserId, 1, quantity: 2, product: product)
        };
        _cartRepoMock.Setup(r => r.GetCartItemsByUserIdAsync(UserId)).ReturnsAsync(cartItems);

        var result = await _sut.GetCartAsync(UserId);

        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1000m, result.Subtotal);
    }

    [Fact]
    public async Task GetCartAsync_SubtotalAboveThreshold_ShippingIsFree()
    {
        // Заказ на сумму >= 100 — доставка бесплатна
        var product = TestDataFactory.CreateProduct(1, price: 150m);
        var cartItems = new List<CartItem>
        {
            TestDataFactory.CreateCartItem(1, UserId, quantity: 1, product: product)
        };
        _cartRepoMock.Setup(r => r.GetCartItemsByUserIdAsync(UserId)).ReturnsAsync(cartItems);

        var result = await _sut.GetCartAsync(UserId);

        Assert.Equal(0m, result.ShippingCost);
    }

    [Fact]
    public async Task GetCartAsync_SubtotalBelowThreshold_ShippingCostApplied()
    {
        // Заказ на сумму < 100 — стоимость доставки 10
        var product = TestDataFactory.CreateProduct(1, price: 50m);
        var cartItems = new List<CartItem>
        {
            TestDataFactory.CreateCartItem(1, UserId, quantity: 1, product: product)
        };
        _cartRepoMock.Setup(r => r.GetCartItemsByUserIdAsync(UserId)).ReturnsAsync(cartItems);

        var result = await _sut.GetCartAsync(UserId);

        Assert.Equal(10m, result.ShippingCost);
        Assert.Equal(60m, result.TotalAmount);
    }

    [Fact]
    public async Task GetCartAsync_EmptyCart_ReturnsEmptyCartDto()
    {
        _cartRepoMock.Setup(r => r.GetCartItemsByUserIdAsync(UserId)).ReturnsAsync(new List<CartItem>());

        var result = await _sut.GetCartAsync(UserId);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0m, result.Subtotal);
    }

    // ─── AddToCartAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task AddToCartAsync_NewProduct_AddsItemAndReturnsTrue()
    {
        var product = TestDataFactory.CreateProduct(1, stock: 5);
        var dto = TestDataFactory.CreateAddToCartDto(1, quantity: 1);

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(UserId, 1)).ReturnsAsync((CartItem?)null);
        _cartRepoMock.Setup(r => r.AddAsync(It.IsAny<CartItem>())).ReturnsAsync(new ComputerStore.Domain.Entities.CartItem());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.AddToCartAsync(UserId, dto);

        Assert.True(result);
        _cartRepoMock.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ExistingCartItem_IncrementsQuantity()
    {
        var product = TestDataFactory.CreateProduct(1, stock: 10);
        var existingItem = TestDataFactory.CreateCartItem(1, UserId, 1, quantity: 2, product: product);
        var dto = TestDataFactory.CreateAddToCartDto(1, quantity: 3);

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(UserId, 1)).ReturnsAsync(existingItem);
        _cartRepoMock.Setup(r => r.UpdateAsync(existingItem)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.AddToCartAsync(UserId, dto);

        Assert.True(result);
        Assert.Equal(5, existingItem.Quantity); // 2 + 3
        _cartRepoMock.Verify(r => r.UpdateAsync(existingItem), Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ProductNotFound_ReturnsFalse()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _sut.AddToCartAsync(UserId, TestDataFactory.CreateAddToCartDto());

        Assert.False(result);
    }

    [Fact]
    public async Task AddToCartAsync_ProductUnavailable_ReturnsFalse()
    {
        var unavailableProduct = TestDataFactory.CreateProduct(1, isAvailable: false);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(unavailableProduct);

        var result = await _sut.AddToCartAsync(UserId, TestDataFactory.CreateAddToCartDto(1));

        Assert.False(result);
        _cartRepoMock.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_RequestedQuantityExceedsStock_ReturnsFalse()
    {
        var product = TestDataFactory.CreateProduct(1, stock: 2);
        var dto = TestDataFactory.CreateAddToCartDto(1, quantity: 5);

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.AddToCartAsync(UserId, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task AddToCartAsync_AfterIncrementExceedsStock_ReturnsFalse()
    {
        var product = TestDataFactory.CreateProduct(1, stock: 4);
        var existingItem = TestDataFactory.CreateCartItem(1, UserId, quantity: 3, product: product);
        var dto = TestDataFactory.CreateAddToCartDto(1, quantity: 2);

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(UserId, 1)).ReturnsAsync(existingItem);

        var result = await _sut.AddToCartAsync(UserId, dto);

        Assert.False(result); // 3 + 2 = 5 > 4 (stock)
    }

    // ─── UpdateCartItemAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCartItemAsync_ValidRequest_UpdatesQuantityAndReturnsTrue()
    {
        var product = TestDataFactory.CreateProduct(1, stock: 10);
        var cartItem = TestDataFactory.CreateCartItem(1, UserId, quantity: 2, product: product);
        var dto = new UpdateCartItemDto { CartItemId = 1, Quantity = 4 };

        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _cartRepoMock.Setup(r => r.UpdateAsync(cartItem)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateCartItemAsync(UserId, dto);

        Assert.True(result);
        Assert.Equal(4, cartItem.Quantity);
    }

    [Fact]
    public async Task UpdateCartItemAsync_ItemBelongsToAnotherUser_ReturnsFalse()
    {
        var cartItem = TestDataFactory.CreateCartItem(1, userId: "another-user");
        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);

        var result = await _sut.UpdateCartItemAsync(UserId, new UpdateCartItemDto { CartItemId = 1, Quantity = 2 });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartItemNotFound_ReturnsFalse()
    {
        _cartRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CartItem?)null);

        var result = await _sut.UpdateCartItemAsync(UserId, new UpdateCartItemDto { CartItemId = 99, Quantity = 1 });

        Assert.False(result);
    }

    // ─── RemoveFromCartAsync ────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFromCartAsync_OwnItem_RemovesAndReturnsTrue()
    {
        var cartItem = TestDataFactory.CreateCartItem(1, UserId);
        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);
        _cartRepoMock.Setup(r => r.DeleteAsync(cartItem)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.RemoveFromCartAsync(UserId, 1);

        Assert.True(result);
        _cartRepoMock.Verify(r => r.DeleteAsync(cartItem), Times.Once);
    }

    [Fact]
    public async Task RemoveFromCartAsync_ItemBelongsToAnotherUser_ReturnsFalse()
    {
        var cartItem = TestDataFactory.CreateCartItem(1, userId: "different-user");
        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cartItem);

        var result = await _sut.RemoveFromCartAsync(UserId, 1);

        Assert.False(result);
        _cartRepoMock.Verify(r => r.DeleteAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFromCartAsync_ItemNotFound_ReturnsFalse()
    {
        _cartRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CartItem?)null);

        var result = await _sut.RemoveFromCartAsync(UserId, 99);

        Assert.False(result);
    }

    // ─── ClearCartAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ClearCartAsync_CallsClearAndSaves()
    {
        _cartRepoMock.Setup(r => r.ClearCartAsync(UserId)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.ClearCartAsync(UserId);

        _cartRepoMock.Verify(r => r.ClearCartAsync(UserId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ─── GetCartItemsCountAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetCartItemsCountAsync_ReturnsCorrectTotalQuantity()
    {
        var product = TestDataFactory.CreateProduct(1);
        var cartItems = new List<CartItem>
        {
            TestDataFactory.CreateCartItem(1, UserId, quantity: 3, product: product),
            TestDataFactory.CreateCartItem(2, UserId, quantity: 2, product: product)
        };
        _cartRepoMock.Setup(r => r.GetCartItemsByUserIdAsync(UserId)).ReturnsAsync(cartItems);

        var result = await _sut.GetCartItemsCountAsync(UserId);

        Assert.Equal(5, result);
    }
}