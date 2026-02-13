using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class ProfileViewModel
    {
        public CustomerDto Customer { get; set; } = null!;
        public IEnumerable<OrderDto> RecentOrders { get; set; } = new List<OrderDto>();
    }
}
