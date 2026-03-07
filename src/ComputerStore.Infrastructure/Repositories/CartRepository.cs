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

        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(string userId, int productId)
        {
            return await _dbSet
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId, string userId)
        {
            return await _dbSet
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
        }

        public async Task<IEnumerable<CartItem>> GetCartWithDetailsAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.AddedDate)
                .ToListAsync();
        }

        public async Task<int> GetCartItemsCountAsync(string userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cartItems = await _dbSet
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _dbSet.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}