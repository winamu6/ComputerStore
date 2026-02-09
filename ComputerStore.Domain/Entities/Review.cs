using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int HelpfulCount { get; set; } = 0;

        public Product Product { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }

}
