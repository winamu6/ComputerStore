using ComputerStore.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderService = orderService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name!;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                TempData["Error"] = "Профиль покупателя не найден";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _orderService.GetCustomerOrdersAsync(userId);
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var userId = GetUserId();

            if (order.Customer.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();

            var success = await _orderService.CancelOrderAsync(userId, id);

            if (!success)
            {
                TempData["Error"] = "Не удалось отменить заказ. Возможно, заказ уже отправлен.";
            }
            else
            {
                TempData["Success"] = "Заказ успешно отменён";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}