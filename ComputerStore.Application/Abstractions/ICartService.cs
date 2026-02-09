using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, AddToCartDto dto);
        Task<bool> UpdateCartItemAsync(string userId, UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
        Task ClearCartAsync(string userId);
        Task<int> GetCartItemsCountAsync(string userId);
    }
}
