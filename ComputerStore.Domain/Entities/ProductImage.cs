using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        public Product Product { get; set; } = null!;
    }

}
