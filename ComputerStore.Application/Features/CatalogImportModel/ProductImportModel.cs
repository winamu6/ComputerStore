using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Features.CatalogImportModel
{
    public class ProductImportModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
    }
}
