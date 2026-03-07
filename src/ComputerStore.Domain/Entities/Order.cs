using ComputerStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidDate { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }

        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }

        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
