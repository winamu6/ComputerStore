using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class HomeViewModel
    {
        public IEnumerable<ProductDto> FeaturedProducts { get; set; } = new List<ProductDto>();
        public IEnumerable<ProductDto> TopRatedProducts { get; set; } = new List<ProductDto>();
        public IEnumerable<CategoryDto> MainCategories { get; set; } = new List<CategoryDto>();
    }
}
