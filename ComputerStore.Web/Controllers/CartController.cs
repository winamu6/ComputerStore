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
            // Если пользователь авторизован, используем его имя
            // Иначе используем session ID
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

            // Если запрос AJAX, возвращаем JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cartCount = await _cartService.GetCartItemsCountAsync(userId);
                return Json(new { success = true, cartCount });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            var dto = new UpdateCartItemDto
            {
                CartItemId = cartItemId,
                Quantity = quantity
            };

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
