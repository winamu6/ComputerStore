using ComputerStore.Shared.DTOs;
using Xunit;

namespace ComputerStore.Tests.Domain;

/// <summary>
/// Тесты для вычисляемых свойств ProductDto.
/// </summary>
public class ProductDtoTests
{
    [Fact]
    public void FinalPrice_WithDiscount_ReturnsDiscountPrice()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = 750m };
        Assert.Equal(750m, dto.FinalPrice);
    }

    [Fact]
    public void FinalPrice_WithoutDiscount_ReturnsRegularPrice()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = null };
        Assert.Equal(1000m, dto.FinalPrice);
    }

    [Fact]
    public void HasDiscount_WhenDiscountBelowPrice_ReturnsTrue()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = 800m };
        Assert.True(dto.HasDiscount);
    }

    [Fact]
    public void HasDiscount_WhenNoDiscountPrice_ReturnsFalse()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = null };
        Assert.False(dto.HasDiscount);
    }

    [Fact]
    public void HasDiscount_WhenDiscountEqualToPrice_ReturnsFalse()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = 1000m };
        Assert.False(dto.HasDiscount);
    }

    [Theory]
    [InlineData(1000, 750, 25)]   // 25% скидка
    [InlineData(200, 100, 50)]    // 50% скидка
    [InlineData(1000, 900, 10)]   // 10% скидка
    public void DiscountPercentage_CalculatesCorrectly(decimal price, decimal discountPrice, decimal expected)
    {
        var dto = new ProductDto { Price = price, DiscountPrice = discountPrice };
        Assert.Equal(expected, dto.DiscountPercentage);
    }

    [Fact]
    public void DiscountPercentage_NoDiscount_ReturnsZero()
    {
        var dto = new ProductDto { Price = 1000m, DiscountPrice = null };
        Assert.Equal(0m, dto.DiscountPercentage);
    }

    [Fact]
    public void InStock_WhenStockAboveZero_ReturnsTrue()
    {
        var dto = new ProductDto { StockQuantity = 5 };
        Assert.True(dto.InStock);
    }

    [Fact]
    public void InStock_WhenStockIsZero_ReturnsFalse()
    {
        var dto = new ProductDto { StockQuantity = 0 };
        Assert.False(dto.InStock);
    }
}

/// <summary>
/// Тесты для вычисляемых свойств CartDto.
/// </summary>
public class CartDtoTests
{
    [Fact]
    public void TotalItems_SumsAllItemQuantities()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new() { Price = 100m, Quantity = 2 },
                new() { Price = 50m, Quantity = 3 }
            }
        };

        Assert.Equal(5, cart.TotalItems);
    }

    [Fact]
    public void Subtotal_SumsAllItemSubtotals()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new() { Price = 100m, Quantity = 2 },   // 200
                new() { Price = 50m, Quantity = 3 }     // 150
            }
        };

        Assert.Equal(350m, cart.Subtotal);
    }

    [Fact]
    public void TotalAmount_IncludesShippingCost()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto> { new() { Price = 50m, Quantity = 1 } },
            ShippingCost = 10m
        };

        Assert.Equal(60m, cart.TotalAmount);
    }

    [Fact]
    public void HasUnavailableItems_WhenItemOutOfStock_ReturnsTrue()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new() { Price = 100m, Quantity = 5, StockQuantity = 2, IsAvailable = true }
            }
        };

        Assert.True(cart.HasUnavailableItems);
    }

    [Fact]
    public void HasUnavailableItems_WhenItemUnavailable_ReturnsTrue()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new() { Price = 100m, Quantity = 1, StockQuantity = 10, IsAvailable = false }
            }
        };

        Assert.True(cart.HasUnavailableItems);
    }

    [Fact]
    public void HasUnavailableItems_AllItemsAvailable_ReturnsFalse()
    {
        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new() { Price = 100m, Quantity = 2, StockQuantity = 5, IsAvailable = true }
            }
        };

        Assert.False(cart.HasUnavailableItems);
    }
}

/// <summary>
/// Тесты для вычисляемых свойств CartItemDto.
/// </summary>
public class CartItemDtoTests
{
    [Fact]
    public void FinalPrice_WithDiscount_ReturnsDiscountPrice()
    {
        var item = new CartItemDto { Price = 500m, DiscountPrice = 400m };
        Assert.Equal(400m, item.FinalPrice);
    }

    [Fact]
    public void Subtotal_CalculatesQuantityTimesPrice()
    {
        var item = new CartItemDto { Price = 500m, DiscountPrice = 400m, Quantity = 3 };
        Assert.Equal(1200m, item.Subtotal); // 400 * 3
    }

    [Fact]
    public void InStock_WhenStockLessThanQuantity_ReturnsFalse()
    {
        var item = new CartItemDto { Quantity = 5, StockQuantity = 3 };
        Assert.False(item.InStock);
    }

    [Fact]
    public void InStock_WhenStockEqualToQuantity_ReturnsTrue()
    {
        var item = new CartItemDto { Quantity = 3, StockQuantity = 3 };
        Assert.True(item.InStock);
    }
}