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

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return Enumerable.Empty<OrderDto>();

            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customer.Id);
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
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return null;

            var cartItems = await _unitOfWork.CartItems.GetCartWithDetailsAsync(userId);
            var cartItemsList = cartItems.ToList();

            if (!cartItemsList.Any())
                return null;

            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    decimal subtotal = 0;
                    foreach (var cartItem in cartItemsList)
                    {
                        var product = cartItem.Product;

                        if (!product.IsAvailable || product.StockQuantity < cartItem.Quantity)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return null;
                        }

                        var itemPrice = product.DiscountPrice ?? product.Price;
                        subtotal += itemPrice * cartItem.Quantity;
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
                        Notes = dto.Notes,

                        SubTotal = subtotal,
                        ShippingCost = CalculateShippingCost(subtotal),
                        TotalAmount = subtotal + CalculateShippingCost(subtotal)
                    };

                    foreach (var cartItem in cartItemsList)
                    {
                        var product = cartItem.Product;
                        var itemPrice = product.DiscountPrice ?? product.Price;

                        var orderItem = new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = cartItem.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * cartItem.Quantity
                        };

                        order.OrderItems.Add(orderItem);

                        product.StockQuantity -= cartItem.Quantity;
                        await _unitOfWork.Products.UpdateAsync(product);
                    }

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CartItems.ClearCartAsync(userId);
                    await _unitOfWork.SaveChangesAsync();

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

        public async Task<bool> CancelOrderAsync(string userId, int orderId)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return false;

            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id)
                return false;

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                return false;

            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    order.Status = OrderStatus.Cancelled;
                    await _unitOfWork.Orders.UpdateAsync(order);

                    foreach (var item in order.OrderItems)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);
                        }
                    }

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

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.Status == status);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return false;

            order.Status = status;

            if (status == OrderStatus.Delivered)
            {
                order.IsPaid = true;
                order.PaidDate = DateTime.UtcNow;
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateTrackingNumberAsync(int orderId, string trackingNumber)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return false;

            order.TrackingNumber = trackingNumber;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private decimal CalculateShippingCost(decimal subtotal)
        {
            if (subtotal >= 100)
                return 0;

            return 10.00m;
        }
    }
}
