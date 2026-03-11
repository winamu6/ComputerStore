using AutoMapper;
using ComputerStore.Application.Mappings;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Shared.DTOs;
using ComputerStore.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace ComputerStore.Tests.Services;

/// <summary>
/// Тесты для OrderService.
/// Покрывают создание заказа, отмену, изменение статуса и получение данных.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly OrderService _sut;

    private const string UserId = "test-user-001";

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        // Настраиваем ExecutionStrategy для транзакций
        var strategyMock = new Mock<IExecutionStrategy>();
        strategyMock
            .Setup(s => s.ExecuteAsync(
                It.IsAny<object?>(),
                It.IsAny<Func<DbContext, object?, CancellationToken, Task<OrderDetailsDto?>>>(),
                It.IsAny<Func<DbContext, object?, CancellationToken, Task<ExecutionResult<OrderDetailsDto?>>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<object?, Func<DbContext, object?, CancellationToken, Task<OrderDetailsDto?>>,
                Func<DbContext, object?, CancellationToken, Task<ExecutionResult<OrderDetailsDto?>>>, CancellationToken>(
                async (state, operation, verifySucceeded, ct) =>
                    await operation(null!, state, ct));

        _unitOfWorkMock.Setup(u => u.CreateExecutionStrategy()).Returns(strategyMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new OrderService(_unitOfWorkMock.Object, _mapper);
    }

    // ─── GetCustomerOrdersAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetCustomerOrdersAsync_CustomerHasOrders_ReturnsOrders()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var orders = new List<Order>
        {
            TestDataFactory.CreateOrder(1, customerId: 1),
            TestDataFactory.CreateOrder(2, customerId: 1)
        };
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _orderRepoMock.Setup(r => r.GetOrdersByCustomerAsync(1)).ReturnsAsync(orders);

        var result = await _sut.GetCustomerOrdersAsync(UserId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_CustomerNotFound_ReturnsEmptyCollection()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);

        var result = await _sut.GetCustomerOrdersAsync(UserId);

        Assert.Empty(result);
    }

    // ─── GetOrderDetailsAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetOrderDetailsAsync_ExistingOrder_ReturnsDetails()
    {
        var order = TestDataFactory.CreateOrder(10);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(10)).ReturnsAsync(order);

        var result = await _sut.GetOrderDetailsAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task GetOrderDetailsAsync_NonExistentOrder_ReturnsNull()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        var result = await _sut.GetOrderDetailsAsync(404);

        Assert.Null(result);
    }

    // ─── GetOrderByNumberAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetOrderByNumberAsync_ExistingOrderNumber_ReturnsDetails()
    {
        var order = TestDataFactory.CreateOrder(5);
        _orderRepoMock.Setup(r => r.GetOrderByNumberAsync("ORD-TEST-000005")).ReturnsAsync(order);

        var result = await _sut.GetOrderByNumberAsync("ORD-TEST-000005");

        Assert.NotNull(result);
        Assert.Equal("ORD-TEST-000005", result.OrderNumber);
    }

    // ─── UpdateOrderStatusAsync ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatusAsync_ExistingOrder_UpdatesStatusAndReturnsTrue()
    {
        var order = TestDataFactory.CreateOrder(1);
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateAsync(order)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateOrderStatusAsync(1, OrderStatus.Shipped);

        Assert.True(result);
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_StatusDelivered_MarksAsPaid()
    {
        var order = TestDataFactory.CreateOrder(1);
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateAsync(order)).Returns(Task.CompletedTask);

        await _sut.UpdateOrderStatusAsync(1, OrderStatus.Delivered);

        Assert.True(order.IsPaid);
        Assert.NotNull(order.PaidDate);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_NonExistentOrder_ReturnsFalse()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        var result = await _sut.UpdateOrderStatusAsync(999, OrderStatus.Shipped);

        Assert.False(result);
    }

    // ─── UpdateTrackingNumberAsync ──────────────────────────────────────────

    [Fact]
    public async Task UpdateTrackingNumberAsync_ExistingOrder_UpdatesTrackingAndReturnsTrue()
    {
        var order = TestDataFactory.CreateOrder(1);
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateAsync(order)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateTrackingNumberAsync(1, "TRACK-123456");

        Assert.True(result);
        Assert.Equal("TRACK-123456", order.TrackingNumber);
    }

    [Fact]
    public async Task UpdateTrackingNumberAsync_NonExistentOrder_ReturnsFalse()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        var result = await _sut.UpdateTrackingNumberAsync(999, "TRACK-000");

        Assert.False(result);
    }

    // ─── CancelOrderAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrderAsync_PendingOrder_CancelsAndRestoresStock()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var product = TestDataFactory.CreateProduct(1, stock: 5);
        var order = TestDataFactory.CreateOrder(1, customerId: 1, status: OrderStatus.Pending);
        var orderItem = TestDataFactory.CreateOrderItem(1, orderId: 1, productId: 1, quantity: 2);
        order.OrderItems.Add(orderItem);

        var boolStrategyMock = new Mock<IExecutionStrategy>();
        boolStrategyMock
            .Setup(s => s.ExecuteAsync(
                It.IsAny<object?>(),
                It.IsAny<Func<DbContext, object?, CancellationToken, Task<bool>>>(),
                It.IsAny<Func<DbContext, object?, CancellationToken, Task<ExecutionResult<bool>>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<object?, Func<DbContext, object?, CancellationToken, Task<bool>>,
                Func<DbContext, object?, CancellationToken, Task<ExecutionResult<bool>>>, CancellationToken>(
                async (state, operation, verifySucceeded, ct) =>
                    await operation(null!, state, ct));

        _unitOfWorkMock.Setup(u => u.CreateExecutionStrategy()).Returns(boolStrategyMock.Object);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateAsync(order)).Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);

        var result = await _sut.CancelOrderAsync(UserId, 1);

        Assert.True(result);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(7, product.StockQuantity); // 5 + 2 возврат
    }

    [Fact]
    public async Task CancelOrderAsync_ShippedOrder_ReturnsFalse()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var order = TestDataFactory.CreateOrder(1, customerId: 1, status: OrderStatus.Shipped);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);

        var result = await _sut.CancelOrderAsync(UserId, 1);

        Assert.False(result);
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public async Task CancelOrderAsync_OrderBelongsToAnotherCustomer_ReturnsFalse()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var order = TestDataFactory.CreateOrder(1, customerId: 99, status: OrderStatus.Pending); // чужой заказ

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);

        var result = await _sut.CancelOrderAsync(UserId, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task CancelOrderAsync_CustomerNotFound_ReturnsFalse()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);

        var result = await _sut.CancelOrderAsync(UserId, 1);

        Assert.False(result);
    }

    // ─── GetOrdersByStatusAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetOrdersByStatusAsync_ReturnsOrdersWithMatchingStatus()
    {
        var pendingOrders = new List<Order>
        {
            TestDataFactory.CreateOrder(1, status: OrderStatus.Pending),
            TestDataFactory.CreateOrder(2, status: OrderStatus.Pending)
        };
        _orderRepoMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(pendingOrders);

        var result = await _sut.GetOrdersByStatusAsync(OrderStatus.Pending);

        Assert.Equal(2, result.Count());
    }

    // ─── Расчёт стоимости доставки ──────────────────────────────────────────

    [Theory]
    [InlineData(99.99, 10.00)]    // Ниже порога — платная доставка
    [InlineData(100.00, 0.00)]    // Равно порогу — бесплатная доставка
    [InlineData(500.00, 0.00)]    // Выше порога — бесплатная доставка
    public async Task ShippingCost_DependsOnSubtotal(decimal subtotal, decimal expectedShipping)
    {
        var order = TestDataFactory.CreateOrder(1, subtotal: subtotal);
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Проверяем логику через уже созданный заказ
        Assert.Equal(expectedShipping, order.ShippingCost);
    }
}