using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Shared.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        public decimal FinalPrice => DiscountPrice ?? Price;
        public decimal Subtotal => FinalPrice * Quantity;
        public bool InStock => StockQuantity >= Quantity;
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount => Subtotal + ShippingCost;
        public bool HasUnavailableItems => Items.Any(i => !i.IsAvailable || !i.InStock);
    }

    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}