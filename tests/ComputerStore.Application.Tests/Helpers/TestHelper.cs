using AutoMapper;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Tests.Helpers
{
    public static class TestHelper
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            return config.CreateMapper();
        }

        public static Product CreateTestProduct(int id = 1)
        {
            return new Product
            {
                Id = id,
                Name = $"Test Product {id}",
                Description = "Test Description",
                Price = 99.99m,
                DiscountPrice = 79.99m,
                StockQuantity = 10,
                IsAvailable = true,
                IsFeatured = false,
                CategoryId = 1,
                SKU = $"TEST-{id}",
                ImageUrl = "/images/test.jpg",
                Rating = 4.5,
                ReviewCount = 10,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Customer CreateTestCustomer(int id = 1, string userId = "test-user-1")
        {
            return new Customer
            {
                Id = id,
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "1234567890",
                Address = "123 Test St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Order CreateTestOrder(int id = 1, int customerId = 1)
        {
            return new Order
            {
                Id = id,
                CustomerId = customerId,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{id:D8}",
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.CreditCard,
                IsPaid = false,
                ShippingAddress = "123 Test St",
                ShippingCity = "Test City",
                ShippingPostalCode = "12345",
                ShippingCountry = "Test Country",
                SubTotal = 100.00m,
                ShippingCost = 10.00m,
                TotalAmount = 110.00m,
                OrderItems = new List<OrderItem>()
            };
        }

        public static OrderItem CreateTestOrderItem(int id = 1, int orderId = 1, int productId = 1)
        {
            return new OrderItem
            {
                Id = id,
                OrderId = orderId,
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 50.00m,
                TotalPrice = 100.00m
            };
        }

        public static CartItem CreateTestCartItem(int id = 1, string userId = "test-user-1", int productId = 1)
        {
            return new CartItem
            {
                Id = id,
                UserId = userId,
                ProductId = productId,
                Quantity = 1,
                AddedDate = DateTime.UtcNow,
                Product = CreateTestProduct(productId)
            };
        }

        public static Review CreateTestReview(int id = 1, int productId = 1, int customerId = 1)
        {
            return new Review
            {
                Id = id,
                ProductId = productId,
                CustomerId = customerId,
                Rating = 5,
                Title = "Great product!",
                Comment = "This is a test review comment",
                CreatedAt = DateTime.UtcNow,
                IsVerifiedPurchase = true,
                IsApproved = true,
                IsDeleted = false,
                HelpfulCount = 5,
                NotHelpfulCount = 1
            };
        }

        public static Payment CreateTestPayment(int id = 1, int orderId = 1)
        {
            return new Payment
            {
                Id = id,
                OrderId = orderId,
                Amount = 110.00m,
                PaymentMethod = PaymentMethod.CreditCard,
                Status = PaymentStatus.Completed,
                TransactionId = $"TXN-{Guid.NewGuid():N}",
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };
        }

        public static Category CreateTestCategory(int id = 1, string name = "Test Category")
        {
            return new Category
            {
                Id = id,
                Name = name,
                Description = "Test category description",
                ImageUrl = "/images/category.jpg",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
