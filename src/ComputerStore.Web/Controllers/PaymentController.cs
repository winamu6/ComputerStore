using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentController(
            IPaymentService paymentService,
            IOrderService orderService,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name!;
        }

        // GET: Payment/Index/{orderId}
        public async Task<IActionResult> Index(int orderId)
        {
            var order = await _orderService.GetOrderDetailsAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var userId = GetUserId();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null || order.Customer.Id != customer.Id)
            {
                return Forbid();
            }

            if (order.IsPaid)
            {
                TempData["Info"] = "Этот заказ уже оплачен";
                return RedirectToAction("Details", "Orders", new { id = orderId });
            }

            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                var createPaymentDto = new CreatePaymentDto
                {
                    OrderId = orderId,
                    PaymentMethod = order.PaymentMethod,
                    Amount = order.TotalAmount
                };
                payment = await _paymentService.CreatePaymentAsync(createPaymentDto);
            }

            var viewModel = new PaymentViewModel
            {
                Order = order,
                Payment = payment!
            };

            return View(viewModel);
        }

        // POST: Payment/ProcessCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCard(ProcessPaymentDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Проверьте правильность введенных данных";
                var payment = await _paymentService.GetPaymentByIdAsync(dto.PaymentId);
                if (payment != null)
                {
                    return RedirectToAction(nameof(Index), new { orderId = payment.OrderId });
                }
                return RedirectToAction("Index", "Orders");
            }

            var result = await _paymentService.ProcessPaymentAsync(dto);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Success), new { transactionId = result.TransactionId });
            }
            else
            {
                TempData["Error"] = result.Message;
                var payment = await _paymentService.GetPaymentByIdAsync(dto.PaymentId);
                if (payment != null)
                {
                    return RedirectToAction(nameof(Index), new { orderId = payment.OrderId });
                }
                return RedirectToAction("Index", "Orders");
            }
        }

        // POST: Payment/ProcessCash
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCash(int orderId)
        {
            var result = await _paymentService.ProcessCashPaymentAsync(orderId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Success), new { transactionId = result.TransactionId });
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index), new { orderId });
            }
        }

        // GET: Payment/Success
        public async Task<IActionResult> Success(string transactionId)
        {
            var payment = await _paymentService.GetPaymentByTransactionIdAsync(transactionId);
            if (payment == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderDetailsAsync(payment.OrderId);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new PaymentSuccessViewModel
            {
                Payment = payment,
                Order = order,
                TransactionId = transactionId
            };

            return View(viewModel);
        }

        // POST: Payment/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int paymentId)
        {
            var success = await _paymentService.CancelPaymentAsync(paymentId);

            if (success)
            {
                TempData["Success"] = "Платеж отменен";
            }
            else
            {
                TempData["Error"] = "Не удалось отменить платеж";
            }

            return RedirectToAction("Index", "Orders");
        }
    }
}
