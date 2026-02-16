using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;
        private const decimal DEFAULT_SHIPPING_COST = 10.00m;
        private const decimal FREE_SHIPPING_THRESHOLD = 100.00m;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cartService = cartService;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            return order != null ? _mapper.Map<OrderDetailsDto>(order) : null;
        }

        public async Task<OrderDetailsDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetOrderByNumberAsync(orderNumber);
            return order != null ? _mapper.Map<OrderDetailsDto>(order) : null;
        }

        public async Task<OrderDetailsDto?> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
                    if (customer == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return null;
                    }

                    var cart = await _cartService.GetCartAsync(userId);
                    if (!cart.Items.Any() || cart.HasUnavailableItems)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return null;
                    }

                    var order = new Order
                    {
                        CustomerId = customer.Id,
                        OrderNumber = GenerateOrderNumber(),
                        OrderDate = DateTime.UtcNow,
                        Status = OrderStatus.Pending,
                        PaymentMethod = dto.PaymentMethod,
                        IsPaid = false,
                        ShippingAddress = dto.ShippingAddress,
                        ShippingCity = dto.ShippingCity,
                        ShippingPostalCode = dto.ShippingPostalCode,
                        ShippingCountry = dto.ShippingCountry,
                        SubTotal = cart.Subtotal,
                        ShippingCost = cart.ShippingCost,
                        TotalAmount = cart.TotalAmount,
                        Notes = dto.Notes
                    };

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (var item in cart.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            UnitPrice = item.FinalPrice,
                            Quantity = item.Quantity,
                            TotalPrice = item.Subtotal
                        };

                        order.OrderItems.Add(orderItem);

                        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _cartService.ClearCartAsync(userId);

                    await _unitOfWork.CommitTransactionAsync();

                    var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.Id);
                    return _mapper.Map<OrderDetailsDto>(createdOrder);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return false;

            order.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.TrackingNumber))
                order.TrackingNumber = dto.TrackingNumber;

            if (dto.Status == OrderStatus.Delivered)
            {
                order.IsPaid = true;
                order.PaidDate = DateTime.UtcNow;
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                return false;

            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null || order.CustomerId != customer.Id)
                return false;

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                return false;

            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);
                        }
                    }

                    order.Status = OrderStatus.Cancelled;
                    await _unitOfWork.Orders.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}
