using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ComputerStore.Domain.Entities;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(p => p.DetailedDescription)
                .HasMaxLength(5000);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.DiscountPrice)
                .HasPrecision(18, 2);

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            builder.Property(p => p.Manufacturer)
                .HasMaxLength(100);

            builder.Property(p => p.Model)
                .HasMaxLength(100);

            builder.Property(p => p.SKU)
                .HasMaxLength(50);

            builder.Property(p => p.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ViewCount)
                .HasDefaultValue(0);

            builder.Property(p => p.Rating)
                .HasDefaultValue(0.0);

            builder.Property(p => p.ReviewCount)
                .HasDefaultValue(0);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.SKU).IsUnique();
            builder.HasIndex(p => p.IsAvailable);
            builder.HasIndex(p => p.IsFeatured);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Specifications)
                .WithOne(ps => ps.Product)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
