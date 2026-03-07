using AutoMapper;
using ComputerStore.Application.Abstractions;
using ComputerStore.Domain.Entities;
using ComputerStore.Domain.Interfaces;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CustomerDto?> GetCustomerByUserIdAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto?> CreateCustomerAsync(string userId, UpdateCustomerDto dto)
        {
            var customer = _mapper.Map<Customer>(dto);
            customer.UserId = userId;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            var createdCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            return _mapper.Map<CustomerDto>(createdCustomer);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(string userId, UpdateCustomerDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
                return null;

            _mapper.Map(dto, customer);
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            var updatedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            return _mapper.Map<CustomerDto>(updatedCustomer);
        }
    }
}
