using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetMainCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _dbSet
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithProductsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products.Where(p => p.IsAvailable))
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
