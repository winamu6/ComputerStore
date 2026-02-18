using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Enums;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.GetByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetCustomerReviewsAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return Enumerable.Empty<ReviewDto>();

            var reviews = await _unitOfWork.Reviews.GetByCustomerIdAsync(customer.Id);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ProductRatingDto> GetProductRatingAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.GetByProductIdAsync(productId);
            var reviewList = reviews.ToList();

            var ratingDto = new ProductRatingDto
            {
                ProductId = productId,
                TotalReviews = reviewList.Count,
                AverageRating = reviewList.Any() ? reviewList.Average(r => r.Rating) : 0,
                FiveStarCount = reviewList.Count(r => r.Rating == 5),
                FourStarCount = reviewList.Count(r => r.Rating == 4),
                ThreeStarCount = reviewList.Count(r => r.Rating == 3),
                TwoStarCount = reviewList.Count(r => r.Rating == 2),
                OneStarCount = reviewList.Count(r => r.Rating == 1)
            };

            return ratingDto;
        }

        public async Task<IEnumerable<TopRatedProductDto>> GetTopRatedProductsAsync(int count = 10)
        {
            var reviews = await _unitOfWork.Reviews.GetTopRatedProductsReviewsAsync(count);
            var reviewGroups = reviews.GroupBy(r => r.ProductId);

            var topRatedProducts = new List<TopRatedProductDto>();

            foreach (var group in reviewGroups)
            {
                var product = group.First().Product;
                var avgRating = group.Average(r => r.Rating);
                var reviewCount = group.Count();

                topRatedProducts.Add(new TopRatedProductDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    AverageRating = avgRating,
                    ReviewCount = reviewCount
                });
            }

            return topRatedProducts
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.ReviewCount)
                .Take(count);
        }

        public async Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return null;

            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return null;

            var canReview = await CanReviewProductAsync(userId, dto.ProductId, dto.OrderId);
            if (!canReview)
                return null;

            var hasReviewed = await _unitOfWork.Reviews.HasCustomerReviewedProductAsync(customer.Id, dto.ProductId);
            if (hasReviewed)
                return null;

            var isVerifiedPurchase = false;
            if (dto.OrderId.HasValue)
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(dto.OrderId.Value);
                if (order != null &&
                    order.CustomerId == customer.Id &&
                    order.Status == OrderStatus.Delivered &&
                    order.OrderItems.Any(oi => oi.ProductId == dto.ProductId))
                {
                    isVerifiedPurchase = true;
                }
            }

            var review = new Review
            {
                ProductId = dto.ProductId,
                CustomerId = customer.Id,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Title = dto.Title,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                IsVerifiedPurchase = isVerifiedPurchase,
                IsApproved = true,
                IsDeleted = false,
                HelpfulCount = 0,
                NotHelpfulCount = 0
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(dto.ProductId);

            var createdReview = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(string userId, UpdateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return null;

            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return null;

            var review = await _unitOfWork.Reviews.GetByIdAsync(dto.Id);
            if (review == null || review.CustomerId != customer.Id || review.IsDeleted)
                return null;

            review.Rating = dto.Rating;
            review.Title = dto.Title;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(review.ProductId);

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> DeleteReviewAsync(string userId, int reviewId)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return false;

            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.CustomerId != customer.Id || review.IsDeleted)
                return false;

            var productId = review.ProductId;

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(productId);

            return true;
        }

        public async Task<bool> MarkHelpfulAsync(int reviewId, bool isHelpful)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted || !review.IsApproved)
                return false;

            if (isHelpful)
                review.HelpfulCount++;
            else
                review.NotHelpfulCount++;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CanReviewProductAsync(string userId, int productId, int? orderId = null)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return false;

            if (orderId.HasValue)
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId.Value);
                if (order == null ||
                    order.CustomerId != customer.Id ||
                    order.Status != OrderStatus.Delivered ||
                    !order.OrderItems.Any(oi => oi.ProductId == productId))
                {
                    return false;
                }
            }

            var hasReviewed = await _unitOfWork.Reviews.HasCustomerReviewedProductAsync(customer.Id, productId);
            return !hasReviewed;
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return;

            var avgRating = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);
            var reviewCount = await _unitOfWork.Reviews.GetReviewCountAsync(productId);

            product.Rating = avgRating;
            product.ReviewCount = reviewCount;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}