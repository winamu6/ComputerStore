using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
        }

        public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
            return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
        }

        public async Task<PaymentDto?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId);
            return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
        }

        public async Task<PaymentDto?> CreatePaymentAsync(CreatePaymentDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return null;

            var existingPayment = await _unitOfWork.Payments.GetByOrderIdAsync(dto.OrderId);
            if (existingPayment != null)
                return _mapper.Map<PaymentDto>(existingPayment);

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(dto.PaymentId);
            if (payment == null)
            {
                return new PaymentResultDto
                {
                    Success = false,
                    Message = "Платеж не найден"
                };
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                return new PaymentResultDto
                {
                    Success = false,
                    Message = "Платеж уже обработан"
                };
            }

            payment.Status = PaymentStatus.Processing;
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            var validationResult = ValidateCardDetails(dto);
            if (!validationResult.IsValid)
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = validationResult.ErrorMessage;
                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResultDto
                {
                    Success = false,
                    Message = validationResult.ErrorMessage!
                };
            }

            await Task.Delay(1000);

            var random = new Random();
            var isSuccess = random.Next(100) < 90;

            if (isSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.TransactionId = GenerateTransactionId();

                var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
                if (order != null)
                {
                    order.IsPaid = true;
                    order.PaidDate = DateTime.UtcNow;
                    order.Status = OrderStatus.Processing;
                    await _unitOfWork.Orders.UpdateAsync(order);
                }

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResultDto
                {
                    Success = true,
                    Message = "Оплата успешно выполнена",
                    TransactionId = payment.TransactionId,
                    Payment = _mapper.Map<PaymentDto>(payment)
                };
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = "Платеж отклонен банком";
                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentResultDto
                {
                    Success = false,
                    Message = "Платеж отклонен банком. Попробуйте другую карту."
                };
            }
        }

        public async Task<PaymentResultDto> ProcessCashPaymentAsync(int orderId)
        {
            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                return new PaymentResultDto
                {
                    Success = false,
                    Message = "Платеж не найден"
                };
            }

            if (payment.PaymentMethod != PaymentMethod.Cash)
            {
                return new PaymentResultDto
                {
                    Success = false,
                    Message = "Этот заказ не предназначен для оплаты наличными"
                };
            }

            payment.Status = PaymentStatus.Pending;
            payment.TransactionId = GenerateTransactionId();
            await _unitOfWork.Payments.UpdateAsync(payment);

            var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Processing;
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.SaveChangesAsync();

            return new PaymentResultDto
            {
                Success = true,
                Message = "Заказ принят. Оплата при получении.",
                TransactionId = payment.TransactionId,
                Payment = _mapper.Map<PaymentDto>(payment)
            };
        }

        public async Task<bool> CancelPaymentAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (payment.Status == PaymentStatus.Completed)
                return false;

            payment.Status = PaymentStatus.Cancelled;
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private (bool IsValid, string? ErrorMessage) ValidateCardDetails(ProcessPaymentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CardNumber) || dto.CardNumber.Length < 13)
                return (false, "Неверный номер карты");

            if (string.IsNullOrWhiteSpace(dto.CardHolderName))
                return (false, "Укажите имя держателя карты");

            if (string.IsNullOrWhiteSpace(dto.ExpiryDate) || !dto.ExpiryDate.Contains('/'))
                return (false, "Неверный формат даты (MM/YY)");

            if (string.IsNullOrWhiteSpace(dto.CVV) || dto.CVV.Length < 3)
                return (false, "Неверный CVV код");

            var parts = dto.ExpiryDate.Split('/');
            if (parts.Length != 2)
                return (false, "Неверный формат даты");

            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return (false, "Неверный формат даты");

            var currentYear = DateTime.UtcNow.Year % 100;
            var currentMonth = DateTime.UtcNow.Month;

            if (year < currentYear || (year == currentYear && month < currentMonth))
                return (false, "Срок действия карты истек");

            return (true, null);
        }

        private string GenerateTransactionId()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}
