using ComputerStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
    }
}
