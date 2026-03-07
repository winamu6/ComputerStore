using Azure.Core;
using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(
            ICartService cartService,
            IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? $"guest_{_httpContextAccessor.HttpContext?.Session.Id}";
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            var dto = new AddToCartDto
            {
                ProductId = productId,
                Quantity = quantity
            };

            var success = await _cartService.AddToCartAsync(userId, dto);
            if (!success)
            {
                TempData["Error"] = "Не удалось добавить товар в корзину. Проверьте наличие на складе.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            TempData["Success"] = "Товар добавлен в корзину";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cartCount = await _cartService.GetCartItemsCountAsync(userId);
                return Json(new { success = true, cartCount });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto)
        {
            if (dto == null || dto.Quantity < 1)
            {
                return Json(new { success = false, message = "Некорректные данные" });
            }

            var userId = GetUserId();
            var success = await _cartService.UpdateCartItemAsync(userId, dto);

            if (!success)
            {
                return Json(new { success = false, message = "Не удалось обновить количество" });
            }

            var cart = await _cartService.GetCartAsync(userId);
            return Json(new
            {
                success = true,
                subtotal = cart.Subtotal,
                shippingCost = cart.ShippingCost,
                totalAmount = cart.TotalAmount
            });
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = GetUserId();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (!success)
            {
                TempData["Error"] = "Не удалось удалить товар из корзины";
            }
            else
            {
                TempData["Success"] = "Товар удалён из корзины";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);

            TempData["Success"] = "Корзина очищена";
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/GetCartCount - для обновления счётчика в хедере
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            var count = await _cartService.GetCartItemsCountAsync(userId);
            return Json(new { count });
        }
    }
}