using AutoMapper;
using ComputerStore.Application.Services;
using ComputerStore.Application.Tests.Helpers;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace ComputerStore.Application.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = TestHelper.CreateMapper();
            _orderService = new OrderService(_unitOfWorkMock.Object, _mapper);
        }

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_WithValidData_ShouldCreateOrder()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);
            var product = TestHelper.CreateTestProduct(1);
            product.StockQuantity = 10;

            var cartItems = new List<CartItem>
        {
            TestHelper.CreateTestCartItem(1, userId, 1)
        };

            var createDto = new CreateOrderDto
            {
                PaymentMethod = PaymentMethod.CreditCard,
                ShippingAddress = "123 Test St",
                ShippingCity = "Test City",
                ShippingPostalCode = "12345",
                ShippingCountry = "Test Country"
            };

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.CartItems.GetCartWithDetailsAsync(userId))
                .ReturnsAsync(cartItems);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.CreateExecutionStrategy())
                .Returns(new TestExecutionStrategy());
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Orders.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CartItems.ClearCartAsync(userId))
                .Returns(Task.CompletedTask);

            var createdOrder = TestHelper.CreateTestOrder(1, customer.Id);
            _unitOfWorkMock.Setup(u => u.Orders.GetOrderWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(userId, createDto);

            // Assert
            result.Should().NotBeNull();
            result!.ShippingAddress.Should().Be("123 Test St");
            _unitOfWorkMock.Verify(u => u.Orders.AddAsync(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CartItems.ClearCartAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithEmptyCart_ShouldReturnNull()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.CartItems.GetCartWithDetailsAsync(userId))
                .ReturnsAsync(new List<CartItem>());

            var createDto = new CreateOrderDto();

            // Act
            var result = await _orderService.CreateOrderAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateOrderAsync_WithInsufficientStock_ShouldReturnNull()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);
            var product = TestHelper.CreateTestProduct(1);
            product.StockQuantity = 0; // Нет на складе

            var cartItem = TestHelper.CreateTestCartItem(1, userId, 1);
            cartItem.Quantity = 5;

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.CartItems.GetCartWithDetailsAsync(userId))
                .ReturnsAsync(new List<CartItem> { cartItem });
            _unitOfWorkMock.Setup(u => u.CreateExecutionStrategy())
                .Returns(new TestExecutionStrategy());
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            var createDto = new CreateOrderDto();

            // Act
            var result = await _orderService.CreateOrderAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        #endregion

        #region GetCustomerOrdersAsync Tests

        [Fact]
        public async Task GetCustomerOrdersAsync_ShouldReturnCustomerOrders()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);
            var orders = new List<Order>
        {
            TestHelper.CreateTestOrder(1, customer.Id),
            TestHelper.CreateTestOrder(2, customer.Id)
        };

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.Orders.GetOrdersByCustomerAsync(customer.Id))
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetCustomerOrdersAsync(userId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCustomerOrdersAsync_WhenCustomerNotFound_ShouldReturnEmpty()
        {
            // Arrange
            var userId = "non-existent";
            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _orderService.GetCustomerOrdersAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetOrderDetailsAsync Tests

        [Fact]
        public async Task GetOrderDetailsAsync_WithValidId_ShouldReturnOrderDetails()
        {
            // Arrange
            var order = TestHelper.CreateTestOrder(1);
            order.Customer = TestHelper.CreateTestCustomer(1);
            order.OrderItems.Add(TestHelper.CreateTestOrderItem(1, 1, 1));

            _unitOfWorkMock.Setup(u => u.Orders.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.OrderItems.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetOrderDetailsAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Orders.GetOrderWithDetailsAsync(999))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.GetOrderDetailsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CancelOrderAsync Tests

        [Fact]
        public async Task CancelOrderAsync_WithPendingOrder_ShouldCancelAndRestoreStock()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);
            var product = TestHelper.CreateTestProduct(1);
            product.StockQuantity = 5;

            var order = TestHelper.CreateTestOrder(1, customer.Id);
            order.Status = OrderStatus.Pending;
            order.OrderItems.Add(TestHelper.CreateTestOrderItem(1, 1, product.Id));
            order.OrderItems[0].Quantity = 2;

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.Orders.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(product.Id))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.CreateExecutionStrategy())
                .Returns(new TestExecutionStrategy());
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.CancelOrderAsync(userId, 1);

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            product.StockQuantity.Should().Be(7); // 5 + 2
        }

        [Fact]
        public async Task CancelOrderAsync_WithShippedOrder_ShouldReturnFalse()
        {
            // Arrange
            var userId = "test-user";
            var customer = TestHelper.CreateTestCustomer(1, userId);
            var order = TestHelper.CreateTestOrder(1, customer.Id);
            order.Status = OrderStatus.Shipped; // Уже отправлен

            _unitOfWorkMock.Setup(u => u.Customers.GetByUserIdAsync(userId))
                .ReturnsAsync(customer);
            _unitOfWorkMock.Setup(u => u.Orders.GetOrderWithDetailsAsync(1))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.CancelOrderAsync(userId, 1);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateOrderStatusAsync Tests

        [Fact]
        public async Task UpdateOrderStatusAsync_WithValidData_ShouldUpdateStatus()
        {
            // Arrange
            var order = TestHelper.CreateTestOrder(1);
            _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(1, OrderStatus.Shipped);

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Shipped);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ToDelivered_ShouldMarkAsPaid()
        {
            // Arrange
            var order = TestHelper.CreateTestOrder(1);
            order.IsPaid = false;

            _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(1, OrderStatus.Delivered);

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Delivered);
            order.IsPaid.Should().BeTrue();
            order.PaidDate.Should().NotBeNull();
        }

        #endregion
    }

    // Helper class for testing execution strategy
    public class TestExecutionStrategy : IExecutionStrategy
    {
        public TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = default)
        {
            return operation(null!, state, cancellationToken);
        }

        public bool RetriesOnFailure => false;
    }
}
