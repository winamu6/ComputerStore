using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;

        public Product Product { get; set; } = null!;
    }

}
