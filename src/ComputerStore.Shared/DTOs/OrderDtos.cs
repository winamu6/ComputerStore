using System;
using System.Collections.Generic;
using System.Text;
using ComputerStore.Domain.Enums;

namespace ComputerStore.Shared.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public bool IsPaid { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
    }

    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodDisplay => PaymentMethod.ToString();
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }

        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }

        public CustomerDto Customer { get; set; } = null!;

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateOrderDto
    {
        public PaymentMethod PaymentMethod { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string? TrackingNumber { get; set; }
    }

}
