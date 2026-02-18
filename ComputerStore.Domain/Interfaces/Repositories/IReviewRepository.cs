using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId);
        Task<Review?> GetByOrderAndProductAsync(int orderId, int productId);
        Task<double> GetAverageRatingAsync(int productId);
        Task<int> GetReviewCountAsync(int productId);
        Task<IEnumerable<Review>> GetTopRatedProductsReviewsAsync(int count);
        Task<bool> HasCustomerReviewedProductAsync(int customerId, int productId);
    }
}
