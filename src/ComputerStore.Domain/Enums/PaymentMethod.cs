using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Enums
{
    public enum PaymentMethod
    {
        CreditCard = 0,     // Кредитная карта
        DebitCard = 1,      // Дебетовая карта
        PayPal = 2,         // PayPal
        BankTransfer = 3,   // Банковский перевод
        Cash = 4            // Наличные при получении
    }
}
