using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _dbSet
                .Where(p => p.IsFeatured && p.IsAvailable)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbSet
                .Where(p => p.IsAvailable &&
                           (p.Name.ToLower().Contains(lowerSearchTerm) ||
                            p.Description.ToLower().Contains(lowerSearchTerm) ||
                            (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(lowerSearchTerm)) ||
                            (p.Model != null && p.Model.ToLower().Contains(lowerSearchTerm))))
                .Include(p => p.Category)
                .OrderByDescending(p => p.Rating)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10)
        {
            return await _dbSet
                .Where(p => p.IsAvailable && p.ReviewCount > 0)
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            var product = await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Specifications)
                .Include(p => p.Images)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                if (product.Specifications != null)
                {
                    product.Specifications = product.Specifications
                        .OrderBy(s => s.DisplayOrder)
                        .ToList();
                }

                if (product.Images != null)
                {
                    product.Images = product.Images
                        .OrderBy(i => i.DisplayOrder)
                        .ToList();
                }

                if (product.Reviews != null)
                {
                    product.Reviews = product.Reviews
                        .OrderByDescending(r => r.CreatedAt)
                        .ToList();
                }
            }

            return product;
        }


        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
