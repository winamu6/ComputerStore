using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10);
        Task<Product?> GetProductWithDetailsAsync(int id);
    }
}
