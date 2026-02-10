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
    public class CartRepository : Repository<CartItem>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsByUserAsync(string userId)
        {
            return await _dbSet
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Category)
                .OrderBy(ci => ci.AddedDate)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(string userId, int productId)
        {
            return await _dbSet
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cartItems = await _dbSet
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _dbSet.RemoveRange(cartItems);
            }
        }

        public override async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _dbSet
                .Include(ci => ci.Product)
                .OrderBy(ci => ci.AddedDate)
                .ToListAsync();
        }
    }
}
