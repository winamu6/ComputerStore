using ComputerStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public Order Order { get; set; } = null!;
    }

}
