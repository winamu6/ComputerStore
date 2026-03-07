using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<ProductDto>> GetTopRatedProductsAsync(int count = 10);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<ProductDetailsDto?> GetProductDetailsAsync(int id);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task IncrementViewCountAsync(int id);
    }
}
