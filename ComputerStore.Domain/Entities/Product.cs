using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? DetailedDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SKU { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public double Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
