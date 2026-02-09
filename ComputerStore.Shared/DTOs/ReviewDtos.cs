using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Shared.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CustomerName { get; set; } = string.Empty;
    }

    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}
