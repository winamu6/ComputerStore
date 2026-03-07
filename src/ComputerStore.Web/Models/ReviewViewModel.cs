using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class CreateReviewViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }

    public class EditReviewViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }

    public class ProductReviewsViewModel
    {
        public ProductDto Product { get; set; } = null!;
        public ProductRatingDto Rating { get; set; } = null!;
        public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public bool CanReview { get; set; }
    }
}
