using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewsController(
            IReviewService reviewService,
            IUnitOfWork unitOfWork)
        {
            _reviewService = reviewService;
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/Reviews
        public async Task<IActionResult> Index(string filter = "all")
        {
            ViewBag.CurrentFilter = filter;

            var allReviews = await GetAllReviewsAsync();

            var filteredReviews = filter switch
            {
                "pending" => allReviews.Where(r => !r.IsApproved && !r.IsDeleted),
                "approved" => allReviews.Where(r => r.IsApproved && !r.IsDeleted),
                "rejected" => allReviews.Where(r => !r.IsApproved && !r.IsDeleted),
                "deleted" => allReviews.Where(r => r.IsDeleted),
                _ => allReviews.Where(r => !r.IsDeleted)
            };

            return View(filteredReviews.OrderByDescending(r => r.CreatedAt));
        }

        // GET: Admin/Reviews/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Загружаем связанные данные
            var product = await _unitOfWork.Products.GetByIdAsync(review.ProductId);
            var customer = await _unitOfWork.Customers.GetByIdAsync(review.CustomerId);

            ViewBag.Product = product;
            ViewBag.Customer = customer;

            if (review.OrderId.HasValue)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(review.OrderId.Value);
                ViewBag.Order = order;
            }

            return View(review);
        }

        // POST: Admin/Reviews/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Отзыв не найден";
                return RedirectToAction(nameof(Index));
            }

            review.IsApproved = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Обновляем рейтинг товара
            await UpdateProductRatingAsync(review.ProductId);

            TempData["Success"] = "Отзыв одобрен";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reviews/Reject/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Отзыв не найден";
                return RedirectToAction(nameof(Index));
            }

            review.IsApproved = false;
            review.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Обновляем рейтинг товара
            await UpdateProductRatingAsync(review.ProductId);

            TempData["Success"] = "Отзыв отклонён";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reviews/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Отзыв не найден";
                return RedirectToAction(nameof(Index));
            }

            var productId = review.ProductId;

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Обновляем рейтинг товара
            await UpdateProductRatingAsync(productId);

            TempData["Success"] = "Отзыв удалён";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reviews/Restore/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Отзыв не найден";
                return RedirectToAction(nameof(Index));
            }

            review.IsDeleted = false;
            review.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Обновляем рейтинг товара
            await UpdateProductRatingAsync(review.ProductId);

            TempData["Success"] = "Отзыв восстановлен";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reviews/HardDelete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Отзыв не найден";
                return RedirectToAction(nameof(Index));
            }

            var productId = review.ProductId;

            await _unitOfWork.Reviews.DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Обновляем рейтинг товара
            await UpdateProductRatingAsync(productId);

            TempData["Success"] = "Отзыв удалён навсегда";
            return RedirectToAction(nameof(Index), new { filter = "deleted" });
        }

        // Helper methods
        private async Task<IEnumerable<dynamic>> GetAllReviewsAsync()
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync();

            var result = new List<dynamic>();
            foreach (var review in reviews)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(review.ProductId);
                var customer = await _unitOfWork.Customers.GetByIdAsync(review.CustomerId);

                result.Add(new
                {
                    review.Id,
                    review.ProductId,
                    ProductName = product?.Name ?? "Неизвестный товар",
                    review.CustomerId,
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Неизвестный покупатель",
                    review.Rating,
                    review.Title,
                    review.Comment,
                    review.IsApproved,
                    review.IsDeleted,
                    review.IsVerifiedPurchase,
                    review.HelpfulCount,
                    review.NotHelpfulCount,
                    review.CreatedAt,
                    review.UpdatedAt
                });
            }

            return result;
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return;

            var avgRating = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);
            var reviewCount = await _unitOfWork.Reviews.GetReviewCountAsync(productId);

            product.Rating = avgRating;
            product.ReviewCount = reviewCount;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}
