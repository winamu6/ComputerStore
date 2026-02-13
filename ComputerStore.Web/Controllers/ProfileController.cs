using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(
            ICustomerService customerService,
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor)
        {
            _customerService = customerService;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name!;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var orders = await _orderService.GetOrdersByCustomerAsync(customer.Id);

            var viewModel = new ProfileViewModel
            {
                Customer = customer,
                RecentOrders = orders.Take(5)
            };

            return View(viewModel);
        }

        // GET: Profile/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = GetUserId();
            var customer = await _customerService.CreateCustomerAsync(userId, dto);

            if (customer == null)
            {
                TempData["Error"] = "Не удалось создать профиль";
                return View(dto);
            }

            TempData["Success"] = "Профиль успешно создан";
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var dto = new UpdateCustomerDto
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                City = customer.City,
                PostalCode = customer.PostalCode,
                Country = customer.Country
            };

            return View(dto);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = GetUserId();
            var customer = await _customerService.UpdateCustomerAsync(userId, dto);

            if (customer == null)
            {
                TempData["Error"] = "Не удалось обновить профиль";
                return View(dto);
            }

            TempData["Success"] = "Профиль успешно обновлён";
            return RedirectToAction(nameof(Index));
        }
    }
}
