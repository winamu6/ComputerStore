using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetApprovedReviewsByProductAsync(int productId);
        Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto dto);
        Task<bool> ApproveReviewAsync(int reviewId);
        Task<bool> DeleteReviewAsync(int reviewId, string userId);
    }
}
