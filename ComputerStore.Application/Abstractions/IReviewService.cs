using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetCustomerReviewsAsync(string userId);
        Task<ProductRatingDto> GetProductRatingAsync(int productId);
        Task<IEnumerable<TopRatedProductDto>> GetTopRatedProductsAsync(int count = 10);
        Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto dto);
        Task<ReviewDto?> UpdateReviewAsync(string userId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(string userId, int reviewId);
        Task<bool> MarkHelpfulAsync(int reviewId, bool isHelpful);
        Task<bool> CanReviewProductAsync(string userId, int productId, int? orderId = null);
    }
}