using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
        Task<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<PaymentDto?> CreatePaymentAsync(CreatePaymentDto dto);
        Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto dto);
        Task<PaymentResultDto> ProcessCashPaymentAsync(int orderId);
        Task<bool> CancelPaymentAsync(int paymentId);
    }
}
