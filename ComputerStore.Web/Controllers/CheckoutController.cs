using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckoutController(
            IOrderService orderService,
            ICartService cartService,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderService = orderService;
            _cartService = cartService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name!;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId);

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index", "Cart");
            }

            if (cart.HasUnavailableItems)
            {
                TempData["Error"] = "В корзине есть недоступные товары. Пожалуйста, проверьте корзину.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                Customer = customer,
                Order = new CreateOrderDto
                {
                    ShippingAddress = customer?.Address ?? "",
                    ShippingCity = customer?.City ?? "",
                    ShippingPostalCode = customer?.PostalCode ?? "",
                    ShippingCountry = customer?.Country ?? ""
                }
            };

            return View(viewModel);
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                var customer = await _customerService.GetCustomerByUserIdAsync(userId);

                var viewModel = new CheckoutViewModel
                {
                    Cart = cart,
                    Customer = customer,
                    Order = dto
                };

                return View("Index", viewModel);
            }

            var order = await _orderService.CreateOrderAsync(GetUserId(), dto);

            if (order == null)
            {
                TempData["Error"] = "Не удалось создать заказ. Пожалуйста, попробуйте снова.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Заказ #{order.OrderNumber} успешно создан!";
            return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
        }

        // GET: Checkout/Confirmation
        public async Task<IActionResult> Confirmation(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);

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

            return View(order);
        }
    }
}
