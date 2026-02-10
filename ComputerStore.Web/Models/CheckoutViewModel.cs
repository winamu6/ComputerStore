using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class CheckoutViewModel
    {
        public CartDto Cart { get; set; } = null!;
        public CustomerDto? Customer { get; set; }
        public CreateOrderDto Order { get; set; } = null!;
    }
}
