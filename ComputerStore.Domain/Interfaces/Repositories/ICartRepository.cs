using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface ICartRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserAsync(string userId);
        Task<CartItem?> GetCartItemAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}
