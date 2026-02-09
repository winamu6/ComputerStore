using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,        // Ожидает обработки
        Processing = 1,     // В обработке
        Shipped = 2,        // Отправлен
        Delivered = 3,      // Доставлен
        Cancelled = 4,      // Отменён
        Refunded = 5        // Возврат средств
    }
}
