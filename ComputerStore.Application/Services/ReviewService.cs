using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
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

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsByProductAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetApprovedReviewsByProductAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.GetApprovedReviewsByProductAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return null;

            var customerOrders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customer.Id);
            var hasPurchased = customerOrders
                .Any(o => o.OrderItems.Any(oi => oi.ProductId == dto.ProductId));

            var review = _mapper.Map<Review>(dto);
            review.CustomerId = customer.Id;
            review.IsVerifiedPurchase = hasPurchased;
            review.IsApproved = false;

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(dto.ProductId);

            var createdReview = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
            return _mapper.Map<ReviewDto>(createdReview);
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                return false;

            review.IsApproved = true;
            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(review.ProductId);

            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                return false;

            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null || review.CustomerId != customer.Id)
                return false;

            await _unitOfWork.Reviews.SoftDeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await UpdateProductRatingAsync(review.ProductId);

            return true;
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return;

            var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);
            var reviewCount = await _unitOfWork.Reviews.CountAsync(r => r.ProductId == productId && r.IsApproved);

            product.Rating = averageRating;
            product.ReviewCount = reviewCount;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
