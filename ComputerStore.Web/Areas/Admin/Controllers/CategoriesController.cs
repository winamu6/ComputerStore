using ComputerStore.Application.Abstractions;
using ComputerStore.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(dto);
            }

            await _categoryService.CreateCategoryAsync(dto);
            TempData["Success"] = "Категория успешно создана";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

            var dto = new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId
            };

            return View(dto);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(dto);
            }

            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            if (result == null)
            {
                return NotFound();
            }

            TempData["Success"] = "Категория успешно обновлена";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                TempData["Error"] = "Не удалось удалить категорию. Возможно, в ней есть товары.";
            }
            else
            {
                TempData["Success"] = "Категория успешно удалена";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
