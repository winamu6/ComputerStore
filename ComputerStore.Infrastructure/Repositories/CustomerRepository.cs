using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Customer?> GetCustomerWithOrdersAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }
    }
}
