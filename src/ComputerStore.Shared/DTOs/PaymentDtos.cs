using ComputerStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComputerStore.Shared.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodDisplay => PaymentMethod.ToString();
        public PaymentStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? TransactionId { get; set; }
    }

    public class CreatePaymentDto
    {
        public int OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int PaymentId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
    }

    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public PaymentDto? Payment { get; set; }
    }
}
