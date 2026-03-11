using AutoMapper;
using ComputerStore.Application.Mappings;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Tests.TestHelpers;
using Moq;
using Xunit;

namespace ComputerStore.Tests.Services;

/// <summary>
/// Тесты для ReviewService.
/// Покрывают создание, обновление, удаление отзывов, подсчёт рейтинга и голосование.
/// </summary>
public class ReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly ReviewService _sut;

    private const string UserId = "test-user-001";

    public ReviewServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _reviewRepoMock = new Mock<IReviewRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _productRepoMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new ReviewService(_unitOfWorkMock.Object, _mapper);
    }

    // ─── GetProductReviewsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetProductReviewsAsync_ReturnsReviewsForProduct()
    {
        var reviews = new List<Review>
        {
            TestDataFactory.CreateReview(1, productId: 5, rating: 5),
            TestDataFactory.CreateReview(2, productId: 5, rating: 3)
        };
        _reviewRepoMock.Setup(r => r.GetByProductIdAsync(5)).ReturnsAsync(reviews);

        var result = await _sut.GetProductReviewsAsync(5);

        Assert.Equal(2, result.Count());
    }

    // ─── GetProductRatingAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetProductRatingAsync_MultipleReviews_CalculatesAverageAndCounts()
    {
        var reviews = new List<Review>
        {
            TestDataFactory.CreateReview(1, productId: 1, rating: 5),
            TestDataFactory.CreateReview(2, productId: 1, rating: 4),
            TestDataFactory.CreateReview(3, productId: 1, rating: 5),
            TestDataFactory.CreateReview(4, productId: 1, rating: 3)
        };
        _reviewRepoMock.Setup(r => r.GetByProductIdAsync(1)).ReturnsAsync(reviews);

        var result = await _sut.GetProductRatingAsync(1);

        Assert.Equal(4, result.TotalReviews);
        Assert.Equal(4.25, result.AverageRating);
        Assert.Equal(2, result.FiveStarCount);
        Assert.Equal(1, result.FourStarCount);
        Assert.Equal(1, result.ThreeStarCount);
        Assert.Equal(0, result.TwoStarCount);
        Assert.Equal(0, result.OneStarCount);
    }

    [Fact]
    public async Task GetProductRatingAsync_NoReviews_ReturnsZeroRating()
    {
        _reviewRepoMock.Setup(r => r.GetByProductIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Review>());

        var result = await _sut.GetProductRatingAsync(1);

        Assert.Equal(0, result.TotalReviews);
        Assert.Equal(0, result.AverageRating);
    }

    // ─── CreateReviewAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateReviewAsync_ValidRequest_CreatesReview()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var dto = TestDataFactory.CreateReviewDto(productId: 1, rating: 5);
        var createdReview = TestDataFactory.CreateReview(10, productId: 1, customerId: 1);
        var product = TestDataFactory.CreateProduct(1);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.HasCustomerReviewedProductAsync(1, 1)).ReturnsAsync(false);
        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Review>())).ReturnsAsync(new ComputerStore.Domain.Entities.Review());
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(createdReview);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
        _reviewRepoMock.Setup(r => r.GetAverageRatingAsync(1)).ReturnsAsync(5.0);
        _reviewRepoMock.Setup(r => r.GetReviewCountAsync(1)).ReturnsAsync(1);

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.NotNull(result);
        _reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_InvalidRating_ReturnsNull()
    {
        var dto = TestDataFactory.CreateReviewDto(rating: 0); // невалидный рейтинг

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.Null(result);
        _reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task CreateReviewAsync_RatingAboveMax_ReturnsNull()
    {
        var dto = TestDataFactory.CreateReviewDto(rating: 6); // выше максимума

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateReviewAsync_CustomerNotFound_ReturnsNull()
    {
        var dto = TestDataFactory.CreateReviewDto(rating: 5);
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateReviewAsync_AlreadyReviewed_ReturnsNull()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var dto = TestDataFactory.CreateReviewDto(productId: 1, rating: 4);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.HasCustomerReviewedProductAsync(1, 1)).ReturnsAsync(true);

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateReviewAsync_WithDeliveredOrder_SetsVerifiedPurchase()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var product = TestDataFactory.CreateProduct(1);
        var orderItem = TestDataFactory.CreateOrderItem(productId: 1);
        var order = TestDataFactory.CreateOrder(10, customerId: 1, status: OrderStatus.Delivered);
        order.OrderItems.Add(orderItem);

        var dto = TestDataFactory.CreateReviewDto(productId: 1, rating: 5, orderId: 10);
        var createdReview = TestDataFactory.CreateReview(1, productId: 1, customerId: 1);
        createdReview.IsVerifiedPurchase = true;

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.HasCustomerReviewedProductAsync(1, 1)).ReturnsAsync(false);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(10)).ReturnsAsync(order);
        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Review>())).ReturnsAsync(new ComputerStore.Domain.Entities.Review());
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(createdReview);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
        _reviewRepoMock.Setup(r => r.GetAverageRatingAsync(1)).ReturnsAsync(5.0);
        _reviewRepoMock.Setup(r => r.GetReviewCountAsync(1)).ReturnsAsync(1);

        var result = await _sut.CreateReviewAsync(UserId, dto);

        Assert.NotNull(result);
        // Проверяем, что отзыв создан с флагом подтверждённой покупки
        _reviewRepoMock.Verify(r => r.AddAsync(It.Is<Review>(rv => rv.IsVerifiedPurchase == true)), Times.Once);
    }

    // ─── UpdateReviewAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateReviewAsync_OwnReview_UpdatesAndReturnsDto()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var review = TestDataFactory.CreateReview(5, customerId: 1);
        var product = TestDataFactory.CreateProduct(review.ProductId);
        var updateDto = new ComputerStore.Shared.DTOs.UpdateReviewDto
        {
            Id = 5,
            Rating = 4,
            Title = "Обновлённый заголовок",
            Comment = "Изменилось мнение"
        };

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(review);
        _reviewRepoMock.Setup(r => r.UpdateAsync(review)).Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.GetByIdAsync(review.ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
        _reviewRepoMock.Setup(r => r.GetAverageRatingAsync(review.ProductId)).ReturnsAsync(4.0);
        _reviewRepoMock.Setup(r => r.GetReviewCountAsync(review.ProductId)).ReturnsAsync(1);

        var result = await _sut.UpdateReviewAsync(UserId, updateDto);

        Assert.NotNull(result);
        Assert.Equal(4, review.Rating);
        Assert.Equal("Обновлённый заголовок", review.Title);
    }

    [Fact]
    public async Task UpdateReviewAsync_InvalidRating_ReturnsNull()
    {
        var updateDto = new ComputerStore.Shared.DTOs.UpdateReviewDto { Id = 1, Rating = 10 };

        var result = await _sut.UpdateReviewAsync(UserId, updateDto);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateReviewAsync_ReviewBelongsToAnotherCustomer_ReturnsNull()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var review = TestDataFactory.CreateReview(5, customerId: 99); // другой пользователь
        var updateDto = new ComputerStore.Shared.DTOs.UpdateReviewDto { Id = 5, Rating = 4, Title = "X", Comment = "Y" };

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(review);

        var result = await _sut.UpdateReviewAsync(UserId, updateDto);

        Assert.Null(result);
    }

    // ─── DeleteReviewAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteReviewAsync_OwnReview_SoftDeletesAndReturnsTrue()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var review = TestDataFactory.CreateReview(3, customerId: 1);
        var product = TestDataFactory.CreateProduct(review.ProductId);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(review);
        _reviewRepoMock.Setup(r => r.UpdateAsync(review)).Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.GetByIdAsync(review.ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
        _reviewRepoMock.Setup(r => r.GetAverageRatingAsync(review.ProductId)).ReturnsAsync(0);
        _reviewRepoMock.Setup(r => r.GetReviewCountAsync(review.ProductId)).ReturnsAsync(0);

        var result = await _sut.DeleteReviewAsync(UserId, 3);

        Assert.True(result);
        Assert.True(review.IsDeleted);
        Assert.NotNull(review.UpdatedAt);
    }

    [Fact]
    public async Task DeleteReviewAsync_AlreadyDeleted_ReturnsFalse()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var review = TestDataFactory.CreateReview(3, customerId: 1, isDeleted: true);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(review);

        var result = await _sut.DeleteReviewAsync(UserId, 3);

        Assert.False(result);
        _reviewRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task DeleteReviewAsync_CustomerNotFound_ReturnsFalse()
    {
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);

        var result = await _sut.DeleteReviewAsync(UserId, 1);

        Assert.False(result);
    }

    // ─── MarkHelpfulAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task MarkHelpfulAsync_HelpfulVote_IncrementsHelpfulCount()
    {
        var review = TestDataFactory.CreateReview(1);
        var initialCount = review.HelpfulCount;
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _reviewRepoMock.Setup(r => r.UpdateAsync(review)).Returns(Task.CompletedTask);

        var result = await _sut.MarkHelpfulAsync(1, isHelpful: true);

        Assert.True(result);
        Assert.Equal(initialCount + 1, review.HelpfulCount);
    }

    [Fact]
    public async Task MarkHelpfulAsync_NotHelpfulVote_IncrementsNotHelpfulCount()
    {
        var review = TestDataFactory.CreateReview(1);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _reviewRepoMock.Setup(r => r.UpdateAsync(review)).Returns(Task.CompletedTask);

        await _sut.MarkHelpfulAsync(1, isHelpful: false);

        Assert.Equal(1, review.NotHelpfulCount);
    }

    [Fact]
    public async Task MarkHelpfulAsync_ReviewIsDeleted_ReturnsFalse()
    {
        var review = TestDataFactory.CreateReview(1, isDeleted: true);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _sut.MarkHelpfulAsync(1, true);

        Assert.False(result);
        _reviewRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task MarkHelpfulAsync_ReviewNotApproved_ReturnsFalse()
    {
        var review = TestDataFactory.CreateReview(1, isApproved: false);
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _sut.MarkHelpfulAsync(1, true);

        Assert.False(result);
    }

    [Fact]
    public async Task MarkHelpfulAsync_ReviewNotFound_ReturnsFalse()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Review?)null);

        var result = await _sut.MarkHelpfulAsync(999, true);

        Assert.False(result);
    }

    // ─── CanReviewProductAsync ──────────────────────────────────────────────

    [Fact]
    public async Task CanReviewProductAsync_NotYetReviewed_ReturnsTrue()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.HasCustomerReviewedProductAsync(1, 5)).ReturnsAsync(false);

        var result = await _sut.CanReviewProductAsync(UserId, 5);

        Assert.True(result);
    }

    [Fact]
    public async Task CanReviewProductAsync_AlreadyReviewed_ReturnsFalse()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _reviewRepoMock.Setup(r => r.HasCustomerReviewedProductAsync(1, 5)).ReturnsAsync(true);

        var result = await _sut.CanReviewProductAsync(UserId, 5);

        Assert.False(result);
    }

    [Fact]
    public async Task CanReviewProductAsync_WithOrderNotDelivered_ReturnsFalse()
    {
        var customer = TestDataFactory.CreateCustomer(1, UserId);
        var order = TestDataFactory.CreateOrder(10, customerId: 1, status: OrderStatus.Shipped);

        _customerRepoMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(customer);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(10)).ReturnsAsync(order);

        var result = await _sut.CanReviewProductAsync(UserId, 5, orderId: 10);

        Assert.False(result);
    }
}