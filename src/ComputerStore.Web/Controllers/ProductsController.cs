using ComputerStore.Application.Abstractions;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IReviewService reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId, string search, string sort = "name")
        {
            IEnumerable<Shared.DTOs.ProductDto> products;

            if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId;

                var category = await _categoryService.GetCategoryByIdAsync(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                products = await _productService.SearchProductsAsync(search);
                ViewBag.SearchTerm = search;
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            // Сортировка
            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.FinalPrice),
                "price_desc" => products.OrderByDescending(p => p.FinalPrice),
                "rating" => products.OrderByDescending(p => p.Rating),
                "newest" => products.OrderByDescending(p => p.Id),
                _ => products.OrderBy(p => p.Name)
            };

            ViewBag.Categories = await _categoryService.GetMainCategoriesAsync();
            ViewBag.CurrentSort = sort;

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Увеличиваем счётчик просмотров
            await _productService.IncrementViewCountAsync(id);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = await _productService.GetProductsByCategoryAsync(product.Category.Id)
            };

            // Исключаем текущий товар из похожих
            viewModel.RelatedProducts = viewModel.RelatedProducts
                .Where(p => p.Id != id)
                .Take(4);

            return View(viewModel);
        }

        // GET: Products/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _productService.SearchProductsAsync(query);
            ViewBag.SearchTerm = query;
            ViewBag.Categories = await _categoryService.GetMainCategoriesAsync();

            return View("Index", products);
        }
    }

}
