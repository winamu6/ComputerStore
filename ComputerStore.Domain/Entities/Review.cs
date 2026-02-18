using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }

        public Product Product { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
