using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Abstractions
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetCustomerByUserIdAsync(string userId);
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> CreateCustomerAsync(string userId, UpdateCustomerDto dto);
        Task<CustomerDto?> UpdateCustomerAsync(string userId, UpdateCustomerDto dto);
    }
}
