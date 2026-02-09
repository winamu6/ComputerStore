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

    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetTopRatedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetTopRatedProductsAsync(count);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDetailsDto?> GetProductDetailsAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id);
            return product != null ? _mapper.Map<ProductDetailsDto>(product) : null;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _unitOfWork.Products.GetByIdAsync(product.Id);
            return _mapper.Map<ProductDto>(createdProduct!);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return null;

            _mapper.Map(dto, product);
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var updatedProduct = await _unitOfWork.Products.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(updatedProduct!);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return false;

            await _unitOfWork.Products.SoftDeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                product.ViewCount++;
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }

}
