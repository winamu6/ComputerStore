using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Применяем миграции если они есть
            await context.Database.MigrateAsync();

            // Заполняем категории
            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAsync(context);
            }

            // Заполняем товары
            if (!await context.Products.AnyAsync())
            {
                await SeedProductsAsync(context);
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            var categories = new List<Category>
        {
            new Category
            {
                Name = "Процессоры",
                Description = "Центральные процессоры для настольных ПК",
                ImageUrl = "/images/categories/cpu.svg"
            },
            new Category
            {
                Name = "Видеокарты",
                Description = "Графические ускорители",
                ImageUrl = "/images/categories/gpu.svg"
            },
            new Category
            {
                Name = "Материнские платы",
                Description = "Платы для сборки ПК",
                ImageUrl = "/images/categories/motherboard.svg"
            },
            new Category
            {
                Name = "Оперативная память",
                Description = "Модули RAM DDR4 и DDR5",
                ImageUrl = "/images/categories/ram.svg"
            },
            new Category
            {
                Name = "SSD накопители",
                Description = "Твердотельные накопители",
                ImageUrl = "/images/categories/ssd.svg"
            },
            new Category
            {
                Name = "HDD накопители",
                Description = "Жёсткие диски",
                ImageUrl = "/images/categories/hdd.svg"
            },
            new Category
            {
                Name = "Блоки питания",
                Description = "БП для настольных ПК",
                ImageUrl = "/images/categories/psu.svg"
            },
            new Category
            {
                Name = "Корпуса",
                Description = "Корпуса для сборки ПК",
                ImageUrl = "/images/categories/case.svg"
            },
            new Category
            {
                Name = "Системы охлаждения",
                Description = "Кулеры и системы водяного охлаждения",
                ImageUrl = "/images/categories/cooling.svg"
            },
            new Category
            {
                Name = "Периферия",
                Description = "Мыши, клавиатуры, наушники",
                ImageUrl = "/images/categories/peripherals.svg"
            }
        };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            var categories = await context.Categories.ToListAsync();


            var products = new List<Product>
            {
                // Процессоры
                new Product
                {
                    Name = "Intel Core i9-14900K",
                    Description = "Топовый процессор Intel 14-го поколения",
                    DetailedDescription = "24-ядерный процессор с максимальной частотой 6.0 ГГц. Идеально подходит для игр и профессиональных задач.",
                    Price = 589.99m,
                    StockQuantity = 15,
                    Category = categories.FirstOrDefault(c => c.Name == "Процессоры"),
                    Manufacturer = "Intel",
                    Model = "i9-14900K",
                    SKU = "INTEL-i9-14900K",
                    IsAvailable = true,
                    IsFeatured = true,
                    ImageUrl = "/images/products/i9-14900k.webp",
                    Rating = 4.8
                },
                new Product
                {
                    Name = "AMD Ryzen 9 7950X",
                    Description = "16-ядерный процессор AMD на архитектуре Zen 4",
                    DetailedDescription = "Мощный процессор с 16 ядрами и 32 потоками, частотой до 5.7 ГГц.",
                    Price = 549.99m,
                    DiscountPrice = 399.99m,
                    StockQuantity = 20,
                    Category = categories.FirstOrDefault(c => c.Name == "Процессоры"),
                    Manufacturer = "AMD",
                    Model = "Ryzen 9 7950X",
                    SKU = "AMD-R9-7950X",
                    IsAvailable = true,
                    IsFeatured = true,
                    ImageUrl = "/images/products/ryzen9-7950x.webp",
                    Rating = 4.9
                },
                // Видеокарты
                new Product
                {
                    Name = "NVIDIA GeForce RTX 5090",
                    Description = "Флагманская видеокарта NVIDIA",
                    DetailedDescription = "24 ГБ GDDR6X памяти, поддержка Ray Tracing и DLSS 3.0",
                    Price = 1599.99m,
                    DiscountPrice = 1499.99m,
                    StockQuantity = 8,
                    Category =  categories.FirstOrDefault(c => c.Name == "Видеокарты"),
                    Manufacturer = "NVIDIA",
                    Model = "RTX 4090",
                    SKU = "NV-RTX-4090",
                    IsAvailable = true,
                    IsFeatured = true,
                    ImageUrl = "/images/products/rtx-5090.webp",
                    Rating = 5.0
                },
                new Product
                {
                    Name = "AMD Radeon RX 7900 XTX",
                    Description = "Топовая видеокарта AMD",
                    DetailedDescription = "24 ГБ GDDR6 памяти, архитектура RDNA 3",
                    Price = 999.99m,
                    StockQuantity = 12,
                    Category = categories.FirstOrDefault(c => c.Name == "Видеокарты"),
                    Manufacturer = "AMD",
                    Model = "RX 7900 XTX",
                    SKU = "AMD-RX-7900XTX",
                    IsAvailable = true,
                    IsFeatured = true,
                    ImageUrl = "/images/products/rx-7900xtx.webp",
                    Rating = 4.7
                },
                // Оперативная память
                new Product
                {
                    Name = "Corsair Vengeance DDR5 32GB",
                    Description = "Комплект оперативной памяти DDR5",
                    DetailedDescription = "2x16GB DDR5-6000MHz, низкие тайминги, RGB подсветка",
                    Price = 159.99m,
                    StockQuantity = 50,
                    Category = categories.FirstOrDefault(c => c.Name == "Оперативная память"),
                    Manufacturer = "Corsair",
                    Model = "Vengeance DDR5 32GB",
                    SKU = "CORS-DDR5-32GB",
                    IsAvailable = true,
                    ImageUrl = "/images/products/corsair-ddr5.webp",
                    Rating = 4.6
                },
                // SSD
                new Product
                {
                    Name = "Samsung 990 PRO 2TB",
                    Description = "Быстрый NVMe SSD",
                    DetailedDescription = "PCIe 4.0 x4, скорость чтения до 7450 МБ/с",
                    Price = 179.99m,
                    StockQuantity = 35,
                    Category = categories.FirstOrDefault(c => c.Name == "SSD накопители"),
                    Manufacturer = "Samsung",
                    Model = "990 PRO",
                    SKU = "SAMS-990PRO-2TB",
                    IsAvailable = true,
                    IsFeatured = true,
                    ImageUrl = "/images/products/samsung-990pro.webp",
                    Rating = 4.9
                },
                // Блоки питания
                new Product
                {
                    Name = "Corsair RM1000x 1000W",
                    Description = "Модульный блок питания 80+ Gold",
                    DetailedDescription = "Полностью модульный БП с сертификатом 80 PLUS Gold",
                    Price = 189.99m,
                    StockQuantity = 25,
                    Category = categories.FirstOrDefault(c => c.Name == "Блоки питания"),
                    Manufacturer = "Corsair",
                    Model = "RM1000x",
                    SKU = "CORS-RM1000X",
                    IsAvailable = true,
                    ImageUrl = "/images/products/corsair-rm1000x.webp",
                    Rating = 4.8
                },
                // Периферия
                new Product
                {
                    Name = "Logitech G Pro X Superlight",
                    Description = "Беспроводная игровая мышь",
                    DetailedDescription = "Вес всего 63 грамма, сенсор HERO 25K",
                    Price = 149.99m,
                    StockQuantity = 40,
                    Category = categories.FirstOrDefault(c => c.Name == "Периферия"),
                    Manufacturer = "Logitech",
                    Model = "G Pro X Superlight",
                    SKU = "LOGI-GPRO-SL",
                    IsAvailable = true,
                    ImageUrl = "/images/products/logitech-gpro.webp",
                    Rating = 4.7
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var cpuProduct = products.FirstOrDefault(p => p.Name == "Intel Core i9-14900K");
            var gpuProduct = products.FirstOrDefault(p => p.Name == "NVIDIA GeForce RTX 4090");

            // Добавляем характеристики для некоторых товаров
            var specifications = new List<ProductSpecification>
            {
                // Характеристики для Intel i9-14900K
                new ProductSpecification { Product = cpuProduct, Name = "Количество ядер", Value = "24 (8P+16E)", DisplayOrder = 1 },
                new ProductSpecification { Product = cpuProduct, Name = "Количество потоков", Value = "32", DisplayOrder = 2 },
                new ProductSpecification { Product = cpuProduct, Name = "Базовая частота", Value = "3.2 ГГц", DisplayOrder = 3 },
                new ProductSpecification { Product = cpuProduct, Name = "Максимальная частота", Value = "6.0 ГГц", DisplayOrder = 4 },
                new ProductSpecification { Product = cpuProduct, Name = "Сокет", Value = "LGA1700", DisplayOrder = 5 },
                new ProductSpecification { Product = cpuProduct, Name = "TDP", Value = "125W", DisplayOrder = 6 },
            
                // Характеристики для RTX 4090
                new ProductSpecification { Product = gpuProduct, Name = "Объём памяти", Value = "24 ГБ GDDR6X", DisplayOrder = 1 },
                new ProductSpecification { Product = gpuProduct, Name = "Частота GPU", Value = "2520 МГц", DisplayOrder = 2 },
                new ProductSpecification { Product = gpuProduct, Name = "CUDA ядра", Value = "16384", DisplayOrder = 3 },
                new ProductSpecification { Product = gpuProduct, Name = "Интерфейс", Value = "PCIe 4.0 x16", DisplayOrder = 4 },
                new ProductSpecification { Product = gpuProduct, Name = "Энергопотребление", Value = "450W", DisplayOrder = 5 }
            };

            await context.ProductSpecifications.AddRangeAsync(specifications);

            await context.SaveChangesAsync();
        }
    }
}
