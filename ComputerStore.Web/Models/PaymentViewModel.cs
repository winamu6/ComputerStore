using ComputerStore.Shared.DTOs;

namespace ComputerStore.Web.Models
{
    public class PaymentViewModel
    {
        public OrderDetailsDto Order { get; set; } = null!;
        public PaymentDto Payment { get; set; } = null!;
    }

    public class PaymentSuccessViewModel
    {
        public PaymentDto Payment { get; set; } = null!;
        public OrderDetailsDto Order { get; set; } = null!;
        public string TransactionId { get; set; } = string.Empty;
    }
}
