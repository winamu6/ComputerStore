using ComputerStore.Domain.Interfaces;
using ComputerStore.Domain.Interfaces.Repositories;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IProductRepository? _products;
        private ICategoryRepository? _categories;
        private IOrderRepository? _orders;
        private ICustomerRepository? _customers;
        private ICartRepository? _cartItems;
        private IReviewRepository? _reviews;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products
        {
            get
            {
                _products ??= new ProductRepository(_context);
                return _products;
            }
        }

        public ICategoryRepository Categories
        {
            get
            {
                _categories ??= new CategoryRepository(_context);
                return _categories;
            }
        }

        public IOrderRepository Orders
        {
            get
            {
                _orders ??= new OrderRepository(_context);
                return _orders;
            }
        }

        public ICustomerRepository Customers
        {
            get
            {
                _customers ??= new CustomerRepository(_context);
                return _customers;
            }
        }

        public ICartRepository CartItems
        {
            get
            {
                _cartItems ??= new CartRepository(_context);
                return _cartItems;
            }
        }

        public IReviewRepository Reviews
        {
            get
            {
                _reviews ??= new ReviewRepository(_context);
                return _reviews;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await action();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }
    }
}
