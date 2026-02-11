using ComputerStore.Application.Abstractions;
using ComputerStore.Application.Features;
using ComputerStore.Application.Features.CatalogImportModel;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Json;

namespace ComputerStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ImportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ImportController(
            IUnitOfWork unitOfWork,
            IProductService productService,
            ICategoryService categoryService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Admin/Import
        public IActionResult Index()
        {
            return View();
        }

        // POST: Admin/Import/UploadJson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Пожалуйста, выберите файл";
                return RedirectToAction(nameof(Index));
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Можно загружать только JSON файлы";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                var jsonContent = await stream.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<CatalogImportModel>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null)
                {
                    TempData["Error"] = "Неверный формат JSON файла";
                    return RedirectToAction(nameof(Index));
                }

                await ImportCatalogAsync(data);

                TempData["Success"] = $"Успешно импортировано: {data.Categories?.Count ?? 0} категорий и {data.Products?.Count ?? 0} товаров";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при импорте: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Import/UploadExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Пожалуйста, выберите файл";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "Можно загружать только Excel файлы (.xlsx, .xls)";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0]; // Первый лист

                var rowCount = worksheet.Dimension.Rows;
                var importedCount = 0;

                await _unitOfWork.BeginTransactionAsync();

                for (int row = 2; row <= rowCount; row++) // Пропускаем заголовок
                {
                    var name = worksheet.Cells[row, 1].Value?.ToString();
                    var description = worksheet.Cells[row, 2].Value?.ToString();
                    var categoryName = worksheet.Cells[row, 3].Value?.ToString();
                    var priceStr = worksheet.Cells[row, 4].Value?.ToString();
                    var stockStr = worksheet.Cells[row, 5].Value?.ToString();
                    var manufacturer = worksheet.Cells[row, 6].Value?.ToString();
                    var model = worksheet.Cells[row, 7].Value?.ToString();
                    var sku = worksheet.Cells[row, 8].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryName))
                        continue;

                    // Находим или создаём категорию
                    var category = (await _unitOfWork.Categories.FindAsync(c => c.Name == categoryName)).FirstOrDefault();
                    if (category == null)
                    {
                        category = new Category
                        {
                            Name = categoryName,
                            Description = categoryName
                        };
                        await _unitOfWork.Categories.AddAsync(category);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Создаём товар
                    var product = new Product
                    {
                        Name = name,
                        Description = description ?? name,
                        CategoryId = category.Id,
                        Price = decimal.TryParse(priceStr, out var price) ? price : 0,
                        StockQuantity = int.TryParse(stockStr, out var stock) ? stock : 0,
                        Manufacturer = manufacturer,
                        Model = model,
                        SKU = sku ?? $"SKU-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                        IsAvailable = true
                    };

                    await _unitOfWork.Products.AddAsync(product);
                    importedCount++;
                }

                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = $"Успешно импортировано {importedCount} товаров из Excel";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = $"Ошибка при импорте из Excel: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Import/ExportTemplate
        public IActionResult ExportTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Название*";
            worksheet.Cells[1, 2].Value = "Описание";
            worksheet.Cells[1, 3].Value = "Категория*";
            worksheet.Cells[1, 4].Value = "Цена*";
            worksheet.Cells[1, 5].Value = "Количество";
            worksheet.Cells[1, 6].Value = "Производитель";
            worksheet.Cells[1, 7].Value = "Модель";
            worksheet.Cells[1, 8].Value = "Артикул (SKU)";

            // Примеры данных
            worksheet.Cells[2, 1].Value = "Intel Core i9-14900K";
            worksheet.Cells[2, 2].Value = "Топовый процессор Intel 14-го поколения";
            worksheet.Cells[2, 3].Value = "Процессоры";
            worksheet.Cells[2, 4].Value = 589.99;
            worksheet.Cells[2, 5].Value = 15;
            worksheet.Cells[2, 6].Value = "Intel";
            worksheet.Cells[2, 7].Value = "i9-14900K";
            worksheet.Cells[2, 8].Value = "INTEL-i9-14900K";

            // Форматирование заголовков
            using (var range = worksheet.Cells[1, 1, 1, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "catalog_template.xlsx");
        }

        // GET: Admin/Import/ExportJson
        public async Task<IActionResult> ExportJson()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var products = await _productService.GetAllProductsAsync();

            var exportData = new CatalogImportModel
            {
                Categories = categories.Select(c => new CategoryImportModel
                {
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl
                }).ToList(),
                Products = products.Select(p => new ProductImportModel
                {
                    Name = p.Name,
                    Description = p.Description,
                    CategoryName = p.CategoryName,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    StockQuantity = p.StockQuantity,
                    Manufacturer = p.Manufacturer,
                    Model = p.Model,
                    SKU = p.SKU,
                    ImageUrl = p.ImageUrl,
                    IsFeatured = p.IsFeatured
                }).ToList()
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "catalog_export.json");
        }

        private async Task ImportCatalogAsync(CatalogImportModel data)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Импортируем категории
                var categoryMap = new Dictionary<string, int>();

                if (data.Categories != null)
                {
                    foreach (var catData in data.Categories)
                    {
                        var existing = (await _unitOfWork.Categories.FindAsync(c => c.Name == catData.Name)).FirstOrDefault();

                        if (existing == null)
                        {
                            var category = new Category
                            {
                                Name = catData.Name,
                                Description = catData.Description ?? catData.Name,
                                ImageUrl = catData.ImageUrl
                            };
                            await _unitOfWork.Categories.AddAsync(category);
                            await _unitOfWork.SaveChangesAsync();
                            categoryMap[catData.Name] = category.Id;
                        }
                        else
                        {
                            categoryMap[catData.Name] = existing.Id;
                        }
                    }
                }

                // Импортируем товары
                if (data.Products != null)
                {
                    foreach (var prodData in data.Products)
                    {
                        if (!categoryMap.ContainsKey(prodData.CategoryName))
                        {
                            // Создаём категорию если её нет
                            var category = new Category
                            {
                                Name = prodData.CategoryName,
                                Description = prodData.CategoryName
                            };
                            await _unitOfWork.Categories.AddAsync(category);
                            await _unitOfWork.SaveChangesAsync();
                            categoryMap[prodData.CategoryName] = category.Id;
                        }

                        var product = new Product
                        {
                            Name = prodData.Name,
                            Description = prodData.Description ?? prodData.Name,
                            CategoryId = categoryMap[prodData.CategoryName],
                            Price = prodData.Price,
                            DiscountPrice = prodData.DiscountPrice,
                            StockQuantity = prodData.StockQuantity,
                            Manufacturer = prodData.Manufacturer,
                            Model = prodData.Model,
                            SKU = prodData.SKU ?? $"SKU-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                            ImageUrl = prodData.ImageUrl,
                            IsAvailable = true,
                            IsFeatured = prodData.IsFeatured
                        };

                        await _unitOfWork.Products.AddAsync(product);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}