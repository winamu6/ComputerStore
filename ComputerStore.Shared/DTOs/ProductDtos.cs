using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Shared.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SKU { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        public decimal FinalPrice => DiscountPrice ?? Price;
        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
        public decimal DiscountPercentage => HasDiscount
            ? Math.Round(((Price - DiscountPrice!.Value) / Price) * 100, 0)
            : 0;
        public bool InStock => StockQuantity > 0;
    }

    public class ProductDetailsDto
    {
        public int Id { get; set; }
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
        public bool IsAvailable { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int ViewCount { get; set; }

        public CategoryDto Category { get; set; } = null!;
        public List<ProductSpecificationDto> Specifications { get; set; } = new();
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        
        public decimal FinalPrice => DiscountPrice ?? Price;
        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
        public bool InStock => StockQuantity > 0;
    }

    public class CreateProductDto
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
        public int CategoryId { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }
    }

    public class ProductSpecificationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}
