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

            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            var categories = new List<Category>
        {
            new Category
            {
                Id = 1,
                Name = "Процессоры",
                Description = "Центральные процессоры для настольных ПК",
                ImageUrl = "/images/categories/cpu.jpg"
            },
            new Category
            {
                Id = 2,
                Name = "Видеокарты",
                Description = "Графические ускорители",
                ImageUrl = "/images/categories/gpu.jpg"
            },
            new Category
            {
                Id = 3,
                Name = "Материнские платы",
                Description = "Платы для сборки ПК",
                ImageUrl = "/images/categories/motherboard.jpg"
            },
            new Category
            {
                Id = 4,
                Name = "Оперативная память",
                Description = "Модули RAM DDR4 и DDR5",
                ImageUrl = "/images/categories/ram.jpg"
            },
            new Category
            {
                Id = 5,
                Name = "SSD накопители",
                Description = "Твердотельные накопители",
                ImageUrl = "/images/categories/ssd.jpg"
            },
            new Category
            {
                Id = 6,
                Name = "HDD накопители",
                Description = "Жёсткие диски",
                ImageUrl = "/images/categories/hdd.jpg"
            },
            new Category
            {
                Id = 7,
                Name = "Блоки питания",
                Description = "БП для настольных ПК",
                ImageUrl = "/images/categories/psu.jpg"
            },
            new Category
            {
                Id = 8,
                Name = "Корпуса",
                Description = "Корпуса для сборки ПК",
                ImageUrl = "/images/categories/case.jpg"
            },
            new Category
            {
                Id = 9,
                Name = "Системы охлаждения",
                Description = "Кулеры и системы водяного охлаждения",
                ImageUrl = "/images/categories/cooling.jpg"
            },
            new Category
            {
                Id = 10,
                Name = "Периферия",
                Description = "Мыши, клавиатуры, наушники",
                ImageUrl = "/images/categories/peripherals.jpg"
            }
        };

            await context.Categories.AddRangeAsync(categories);
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
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
                CategoryId = 1,
                Manufacturer = "Intel",
                Model = "i9-14900K",
                SKU = "INTEL-i9-14900K",
                IsAvailable = true,
                IsFeatured = true,
                ImageUrl = "/images/products/i9-14900k.jpg",
                Rating = 4.8
            },
            new Product
            {
                Name = "AMD Ryzen 9 7950X",
                Description = "16-ядерный процессор AMD на архитектуре Zen 4",
                DetailedDescription = "Мощный процессор с 16 ядрами и 32 потоками, частотой до 5.7 ГГц.",
                Price = 549.99m,
                StockQuantity = 20,
                CategoryId = 1,
                Manufacturer = "AMD",
                Model = "Ryzen 9 7950X",
                SKU = "AMD-R9-7950X",
                IsAvailable = true,
                IsFeatured = true,
                ImageUrl = "/images/products/ryzen9-7950x.jpg",
                Rating = 4.9
            },
            // Видеокарты
            new Product
            {
                Name = "NVIDIA GeForce RTX 4090",
                Description = "Флагманская видеокарта NVIDIA",
                DetailedDescription = "24 ГБ GDDR6X памяти, поддержка Ray Tracing и DLSS 3.0",
                Price = 1599.99m,
                DiscountPrice = 1499.99m,
                StockQuantity = 8,
                CategoryId = 2,
                Manufacturer = "NVIDIA",
                Model = "RTX 4090",
                SKU = "NV-RTX-4090",
                IsAvailable = true,
                IsFeatured = true,
                ImageUrl = "/images/products/rtx-4090.jpg",
                Rating = 5.0
            },
            new Product
            {
                Name = "AMD Radeon RX 7900 XTX",
                Description = "Топовая видеокарта AMD",
                DetailedDescription = "24 ГБ GDDR6 памяти, архитектура RDNA 3",
                Price = 999.99m,
                StockQuantity = 12,
                CategoryId = 2,
                Manufacturer = "AMD",
                Model = "RX 7900 XTX",
                SKU = "AMD-RX-7900XTX",
                IsAvailable = true,
                IsFeatured = true,
                ImageUrl = "/images/products/rx-7900xtx.jpg",
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
                CategoryId = 4,
                Manufacturer = "Corsair",
                Model = "Vengeance DDR5 32GB",
                SKU = "CORS-DDR5-32GB",
                IsAvailable = true,
                ImageUrl = "/images/products/corsair-ddr5.jpg",
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
                CategoryId = 5,
                Manufacturer = "Samsung",
                Model = "990 PRO",
                SKU = "SAMS-990PRO-2TB",
                IsAvailable = true,
                IsFeatured = true,
                ImageUrl = "/images/products/samsung-990pro.jpg",
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
                CategoryId = 7,
                Manufacturer = "Corsair",
                Model = "RM1000x",
                SKU = "CORS-RM1000X",
                IsAvailable = true,
                ImageUrl = "/images/products/corsair-rm1000x.jpg",
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
                CategoryId = 10,
                Manufacturer = "Logitech",
                Model = "G Pro X Superlight",
                SKU = "LOGI-GPRO-SL",
                IsAvailable = true,
                ImageUrl = "/images/products/logitech-gpro.jpg",
                Rating = 4.7
            }
        };

            await context.Products.AddRangeAsync(products);

            // Добавляем характеристики для некоторых товаров
            var specifications = new List<ProductSpecification>
        {
            // Характеристики для Intel i9-14900K
            new ProductSpecification { ProductId = 1, Name = "Количество ядер", Value = "24 (8P+16E)", DisplayOrder = 1 },
            new ProductSpecification { ProductId = 1, Name = "Количество потоков", Value = "32", DisplayOrder = 2 },
            new ProductSpecification { ProductId = 1, Name = "Базовая частота", Value = "3.2 ГГц", DisplayOrder = 3 },
            new ProductSpecification { ProductId = 1, Name = "Максимальная частота", Value = "6.0 ГГц", DisplayOrder = 4 },
            new ProductSpecification { ProductId = 1, Name = "Сокет", Value = "LGA1700", DisplayOrder = 5 },
            new ProductSpecification { ProductId = 1, Name = "TDP", Value = "125W", DisplayOrder = 6 },
            
            // Характеристики для RTX 4090
            new ProductSpecification { ProductId = 3, Name = "Объём памяти", Value = "24 ГБ GDDR6X", DisplayOrder = 1 },
            new ProductSpecification { ProductId = 3, Name = "Частота GPU", Value = "2520 МГц", DisplayOrder = 2 },
            new ProductSpecification { ProductId = 3, Name = "CUDA ядра", Value = "16384", DisplayOrder = 3 },
            new ProductSpecification { ProductId = 3, Name = "Интерфейс", Value = "PCIe 4.0 x16", DisplayOrder = 4 },
            new ProductSpecification { ProductId = 3, Name = "Энергопотребление", Value = "450W", DisplayOrder = 5 }
        };

            await context.ProductSpecifications.AddRangeAsync(specifications);
        }
    }
}
