using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pi => pi.AltText)
                .HasMaxLength(200);

            builder.Property(pi => pi.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pi => pi.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.CreatedAt)
                .IsRequired();

            builder.Property(pi => pi.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(pi => pi.ProductId);
            builder.HasIndex(pi => pi.IsMain);
            builder.HasIndex(pi => pi.DisplayOrder);

            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
