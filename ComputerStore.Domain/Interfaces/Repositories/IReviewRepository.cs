using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<Review>> GetApprovedReviewsByProductAsync(int productId);
        Task<double> GetAverageRatingAsync(int productId);
    }
}
