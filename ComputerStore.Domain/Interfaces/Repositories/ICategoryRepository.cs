using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetMainCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId);
        Task<Category?> GetCategoryWithProductsAsync(int id);
        IQueryable<Category> Query();

    }
}
