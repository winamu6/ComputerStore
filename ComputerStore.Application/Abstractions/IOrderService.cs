using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId);
        Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId);
        Task<OrderDetailsDto?> GetOrderByNumberAsync(string orderNumber);
        Task<OrderDetailsDto?> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
        Task<bool> CancelOrderAsync(int orderId, string userId);
    }
}
