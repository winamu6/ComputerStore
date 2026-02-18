using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(r => r.Product)
                .Include(r => r.Order)
                .Where(r => r.CustomerId == customerId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByOrderAndProductAsync(int orderId, int productId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId && !r.IsDeleted);
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _dbSet
                .Where(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            return await _dbSet
                .CountAsync(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetTopRatedProductsReviewsAsync(int count)
        {
            var topProducts = await _dbSet
                .Where(r => r.IsApproved && !r.IsDeleted)
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    ReviewCount = g.Count()
                })
                .Where(x => x.ReviewCount >= 3)
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(count)
                .ToListAsync();

            var productIds = topProducts.Select(x => x.ProductId).ToList();

            return await _dbSet
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Where(r => productIds.Contains(r.ProductId) && r.IsApproved && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> HasCustomerReviewedProductAsync(int customerId, int productId)
        {
            return await _dbSet
                .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId && !r.IsDeleted);
        }

        public async Task<IEnumerable<Review>> GetAllReviewsIncludingUnapprovedAsync(int productId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteAsync(int reviewId)
        {
            var review = await _dbSet.FindAsync(reviewId);
            if (review == null)
                return false;

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveReviewAsync(int reviewId, bool approve)
        {
            var review = await _dbSet.FindAsync(reviewId);
            if (review == null)
                return false;

            review.IsApproved = approve;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
