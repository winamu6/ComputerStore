using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class ProductDetailsViewModel
    {
        public ProductDetailsDto Product { get; set; } = null!;
        public IEnumerable<ProductDto> RelatedProducts { get; set; } = new List<ProductDto>();
    }
}
