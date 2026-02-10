using ComputerStore.Application.Abstractions;
using ComputerStore.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ComputerStore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<HomeController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = await _productService.GetFeaturedProductsAsync(8),
                TopRatedProducts = await _productService.GetTopRatedProductsAsync(6),
                MainCategories = await _categoryService.GetMainCategoriesAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
