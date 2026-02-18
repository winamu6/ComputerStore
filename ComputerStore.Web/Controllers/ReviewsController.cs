using Azure.Core;
using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewsController(
            IReviewService reviewService,
            IProductService productService,
            IOrderService orderService,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _reviewService = reviewService;
            _productService = productService;
            _orderService = orderService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name!;
        }

        // GET: Reviews/TopRated
        public async Task<IActionResult> TopRated(int count = 10)
        {
            var topProducts = await _reviewService.GetTopRatedProductsAsync(count);
            return View(topProducts);
        }

        // GET: Reviews/Create?productId={productId}&orderId={orderId}
        [Authorize]
        public async Task<IActionResult> Create(int productId, int? orderId = null)
        {
            var userId = GetUserId();

            var canReview = await _reviewService.CanReviewProductAsync(userId, productId, orderId);
            if (!canReview)
            {
                TempData["Error"] = "Вы уже оставили отзыв на этот товар или не можете его оценить";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            OrderDetailsDto? order = null;
            if (orderId.HasValue)
            {
                order = await _orderService.GetOrderDetailsAsync(orderId.Value);
            }

            var viewModel = new CreateReviewViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl,
                OrderId = orderId,
                OrderNumber = order?.OrderNumber
            };

            return View(viewModel);
        }

        // POST: Reviews/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(dto.ProductId);
                OrderDetailsDto? order = null;
                if (dto.OrderId.HasValue)
                {
                    order = await _orderService.GetOrderDetailsAsync(dto.OrderId.Value);
                }

                var viewModel = new CreateReviewViewModel
                {
                    ProductId = dto.ProductId,
                    ProductName = product?.Name ?? "",
                    ProductImageUrl = product?.ImageUrl,
                    OrderId = dto.OrderId,
                    OrderNumber = order?.OrderNumber,
                    Rating = dto.Rating,
                    Title = dto.Title,
                    Comment = dto.Comment
                };

                return View(viewModel);
            }

            var userId = GetUserId();
            var review = await _reviewService.CreateReviewAsync(userId, dto);

            if (review == null)
            {
                TempData["Error"] = "Не удалось создать отзыв";
                return RedirectToAction("Details", "Products", new { id = dto.ProductId });
            }

            TempData["Success"] = "Спасибо за ваш отзыв!";
            return RedirectToAction("Details", "Products", new { id = dto.ProductId });
        }

        // GET: Reviews/Edit/{id}
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
            {
                return Forbid();
            }

            var reviews = await _reviewService.GetCustomerReviewsAsync(userId);
            var review = reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            var viewModel = new EditReviewViewModel
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.ProductName,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment
            };

            return View(viewModel);
        }

        // POST: Reviews/Edit
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                var userId = GetUserId();
                var reviews = await _reviewService.GetCustomerReviewsAsync(userId);
                var reviewData = reviews.FirstOrDefault(r => r.Id == dto.Id);

                var viewModel = new EditReviewViewModel
                {
                    Id = dto.Id,
                    ProductId = reviewData?.ProductId ?? 0,
                    ProductName = reviewData?.ProductName ?? "",
                    Rating = dto.Rating,
                    Title = dto.Title,
                    Comment = dto.Comment
                };

                return View(viewModel);
            }

            var result = await _reviewService.UpdateReviewAsync(GetUserId(), dto);

            if (result == null)
            {
                TempData["Error"] = "Не удалось обновить отзыв";
                return RedirectToAction("MyReviews");
            }

            TempData["Success"] = "Отзыв успешно обновлён";
            return RedirectToAction("Details", "Products", new { id = result.ProductId });
        }

        // POST: Reviews/Delete/{id}
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _reviewService.DeleteReviewAsync(GetUserId(), id);

            if (success)
            {
                TempData["Success"] = "Отзыв удалён";
            }
            else
            {
                TempData["Error"] = "Не удалось удалить отзыв";
            }

            return RedirectToAction("MyReviews");
        }

        // GET: Reviews/MyReviews
        [Authorize]
        public async Task<IActionResult> MyReviews()
        {
            var reviews = await _reviewService.GetCustomerReviewsAsync(GetUserId());
            return View(reviews);
        }

        // POST: Reviews/MarkHelpful
        [HttpPost]
        public async Task<IActionResult> MarkHelpful(int reviewId, bool helpful)
        {
            var success = await _reviewService.MarkHelpfulAsync(reviewId, helpful);
            return Json(new { success });
        }
    }
}