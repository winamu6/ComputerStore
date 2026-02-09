using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const decimal DEFAULT_SHIPPING_COST = 10.00m;
        private const decimal FREE_SHIPPING_THRESHOLD = 100.00m;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cartItems = await _unitOfWork.CartItems.GetCartItemsByUserAsync(userId);
            var cartItemDtos = _mapper.Map<List<CartItemDto>>(cartItems);

            var cart = new CartDto
            {
                Items = cartItemDtos
            };

            cart.ShippingCost = cart.Subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : DEFAULT_SHIPPING_COST;

            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null || !product.IsAvailable || product.StockQuantity < dto.Quantity)
                return false;

            var existingItem = await _unitOfWork.CartItems.GetCartItemAsync(userId, dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;

                if (existingItem.Quantity > product.StockQuantity)
                    return false;

                await _unitOfWork.CartItems.UpdateAsync(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    AddedDate = DateTime.UtcNow
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemAsync(string userId, UpdateCartItemDto dto)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(dto.CartItemId);
            if (cartItem == null || cartItem.UserId != userId)
                return false;

            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product == null || !product.IsAvailable || product.StockQuantity < dto.Quantity)
                return false;

            cartItem.Quantity = dto.Quantity;
            await _unitOfWork.CartItems.UpdateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.UserId != userId)
                return false;

            await _unitOfWork.CartItems.DeleteAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(string userId)
        {
            await _unitOfWork.CartItems.ClearCartAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCartItemsCountAsync(string userId)
        {
            var cartItems = await _unitOfWork.CartItems.GetCartItemsByUserAsync(userId);
            return cartItems.Sum(ci => ci.Quantity);
        }
    }
}