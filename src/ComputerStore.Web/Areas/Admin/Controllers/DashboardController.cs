using ComputerStore.Application.Abstractions;
using ComputerStore.Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;

        public DashboardController(
            IProductService productService,
            IOrderService orderService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalProducts = (await _productService.GetAllProductsAsync()).Count(),
                TotalCategories = (await _categoryService.GetAllCategoriesAsync()).Count(),
            };

            return View(viewModel);
        }
    }
}
