using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Enums;
using ComputerStore.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService)
        {
            _orderService = orderService;
            _customerService = customerService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string status = "all")
        {
            ViewBag.CurrentStatus = status;

            IEnumerable<OrderDto> orders;

            if (status == "all")
            {
                orders = await _orderService.GetAllOrdersAsync();
            }
            else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orders = await _orderService.GetOrdersByStatusAsync(parsedStatus);
            }
            else
            {
                orders = await _orderService.GetAllOrdersAsync();
            }

            return View(orders);
        }



        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные";
                return RedirectToAction(nameof(Details), new { id = dto.OrderId });
            }

            var success = await _orderService.UpdateOrderStatusAsync(dto.OrderId, dto.Status);

            if (!success)
            {
                TempData["Error"] = "Не удалось обновить статус заказа";
            }
            else
            {
                TempData["Success"] = "Статус заказа успешно обновлён";
            }

            return RedirectToAction(nameof(Details), new { id = dto.OrderId });
        }
    }
}