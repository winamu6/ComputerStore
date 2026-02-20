using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface ICartRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(string userId);
        Task<CartItem?> GetCartItemAsync(string userId, int productId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId, string userId);
        Task<IEnumerable<CartItem>> GetCartWithDetailsAsync(string userId);
        Task<int> GetCartItemsCountAsync(string userId);
        Task ClearCartAsync(string userId);
    }
}