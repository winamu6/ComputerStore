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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetMainCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentId)
        {
            var categories = await _unitOfWork.Categories.GetSubCategoriesAsync(parentId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryWithSubCategoriesDto?> GetCategoryWithSubCategoriesAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(id);
            return category != null ? _mapper.Map<CategoryWithSubCategoriesDto>(category) : null;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var createdCategory = await _unitOfWork.Categories.GetByIdAsync(category.Id);
            return _mapper.Map<CategoryDto>(createdCategory!);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return null;

            _mapper.Map(dto, category);
            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var updatedCategory = await _unitOfWork.Categories.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(updatedCategory!);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return false;

            var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return false;

            await _unitOfWork.Categories.SoftDeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
