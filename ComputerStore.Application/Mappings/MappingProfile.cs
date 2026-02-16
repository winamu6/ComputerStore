using AutoMapper;
using ComputerStore.Domain.Entities;
using ComputerStore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Specifications.OrderBy(s => s.DisplayOrder)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder)))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt)));

            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            CreateMap<ProductSpecification, ProductSpecificationDto>();
            CreateMap<ProductImage, ProductImageDto>();

            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => p.IsAvailable)));

            CreateMap<Category, CategoryWithSubCategoriesDto>()
                .ForMember(dest => dest.TotalProducts, opt => opt.MapFrom(src => src.Products.Count(p => p.IsAvailable)));

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.Product.DiscountPrice))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Product.StockQuantity))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Product.IsAvailable));

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)));

            CreateMap<Order, OrderDetailsDto>();

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

            CreateMap<CreateOrderDto, Order>();

            CreateMap<Customer, CustomerDto>();
            CreateMap<UpdateCustomerDto, Customer>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));

            CreateMap<CreateReviewDto, Review>();

            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.OrderNumber,
                    opt => opt.MapFrom(src => src.Order.OrderNumber));

            CreateMap<CreatePaymentDto, Payment>();
        }
    }
}
