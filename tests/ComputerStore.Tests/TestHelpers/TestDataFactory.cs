using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Shared.DTOs;

namespace ComputerStore.Tests.TestHelpers;

/// <summary>
/// Фабрика тестовых данных для создания объектов домена в тестах.
/// </summary>
public static class TestDataFactory
{
    public static Category CreateCategory(int id = 1, string name = "Ноутбуки") =>
        new()
        {
            Id = id,
            Name = name,
            Description = "Тестовая категория",
            Products = new List<Product>()
        };

    public static Product CreateProduct(
        int id = 1,
        string name = "Тестовый ноутбук",
        decimal price = 1000m,
        decimal? discountPrice = null,
        int stock = 10,
        bool isAvailable = true,
        int categoryId = 1) =>
        new()
        {
            Id = id,
            Name = name,
            Description = "Описание тестового продукта",
            Price = price,
            DiscountPrice = discountPrice,
            StockQuantity = stock,
            IsAvailable = isAvailable,
            IsFeatured = false,
            CategoryId = categoryId,
            Category = CreateCategory(categoryId),
            ViewCount = 0,
            Rating = 0,
            ReviewCount = 0
        };

    public static Customer CreateCustomer(int id = 1, string userId = "user-001") =>
        new()
        {
            Id = id,
            UserId = userId,
            FirstName = "Иван",
            LastName = "Иванов",
            Orders = new List<Order>()
        };

    public static CartItem CreateCartItem(
        int id = 1,
        string userId = "user-001",
        int productId = 1,
        int quantity = 2,
        Product? product = null) =>
        new()
        {
            Id = id,
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            AddedDate = DateTime.UtcNow,
            Product = product ?? CreateProduct(productId)
        };

    public static Order CreateOrder(
        int id = 1,
        int customerId = 1,
        OrderStatus status = OrderStatus.Pending,
        decimal subtotal = 200m) =>
        new()
        {
            Id = id,
            CustomerId = customerId,
            OrderNumber = $"ORD-TEST-{id:D6}",
            OrderDate = DateTime.UtcNow,
            Status = status,
            SubTotal = subtotal,
            ShippingCost = subtotal >= 100 ? 0 : 10m,
            TotalAmount = subtotal + (subtotal >= 100 ? 0 : 10m),
            ShippingAddress = "ул. Тестовая, 1",
            ShippingCity = "Москва",
            ShippingPostalCode = "101000",
            ShippingCountry = "Россия",
            IsPaid = false,
            Customer = CreateCustomer(customerId),
            OrderItems = new List<OrderItem>()
        };

    public static OrderItem CreateOrderItem(
        int id = 1,
        int orderId = 1,
        int productId = 1,
        int quantity = 2,
        decimal unitPrice = 1000m) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            ProductName = "Тестовый ноутбук",
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity,
            Product = CreateProduct(productId)
        };

    public static Review CreateReview(
        int id = 1,
        int productId = 1,
        int customerId = 1,
        int rating = 5,
        bool isApproved = true,
        bool isDeleted = false) =>
        new()
        {
            Id = id,
            ProductId = productId,
            CustomerId = customerId,
            Rating = rating,
            Title = "Отличный товар",
            Comment = "Очень доволен покупкой",
            CreatedAt = DateTime.UtcNow,
            IsApproved = isApproved,
            IsDeleted = isDeleted,
            IsVerifiedPurchase = false,
            HelpfulCount = 0,
            NotHelpfulCount = 0,
            Product = CreateProduct(productId)
        };

    public static CreateProductDto CreateProductDto(
        string name = "Новый ноутбук",
        decimal price = 999m,
        int categoryId = 1) =>
        new()
        {
            Name = name,
            Description = "Описание нового продукта",
            Price = price,
            StockQuantity = 5,
            CategoryId = categoryId,
            IsFeatured = false
        };

    public static AddToCartDto CreateAddToCartDto(int productId = 1, int quantity = 1) =>
        new() { ProductId = productId, Quantity = quantity };

    public static CreateOrderDto CreateOrderDto() =>
        new()
        {
            PaymentMethod = PaymentMethod.CreditCard,
            ShippingAddress = "ул. Доставки, 10",
            ShippingCity = "Санкт-Петербург",
            ShippingPostalCode = "190000",
            ShippingCountry = "Россия"
        };

    public static CreateReviewDto CreateReviewDto(int productId = 1, int rating = 5, int? orderId = null) =>
        new()
        {
            ProductId = productId,
            OrderId = orderId,
            Rating = rating,
            Title = "Отлично",
            Comment = "Рекомендую всем"
        };
}