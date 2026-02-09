using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10);
        Task<Product?> GetProductWithDetailsAsync(int id);
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetMainCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId);
        Task<Category?> GetCategoryWithProductsAsync(int id);
    }

    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
    }

    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByUserIdAsync(string userId);
        Task<Customer?> GetCustomerWithOrdersAsync(int id);
    }

    public interface ICartRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserAsync(string userId);
        Task<CartItem?> GetCartItemAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }

    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<Review>> GetApprovedReviewsByProductAsync(int productId);
        Task<double> GetAverageRatingAsync(int productId);
    }
}
