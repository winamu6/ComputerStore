using ComputerStore.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        ICustomerRepository Customers { get; }
        ICartRepository CartItems { get; }
        IReviewRepository Reviews { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);

    }
}
