using ComputerStore.Domain.Enums;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(string userId);
        Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId);
        Task<OrderDetailsDto?> GetOrderByNumberAsync(string orderNumber);
        Task<OrderDetailsDto?> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> UpdateTrackingNumberAsync(int orderId, string trackingNumber);
        Task<bool> CancelOrderAsync(string userId, int orderId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
    }
}
