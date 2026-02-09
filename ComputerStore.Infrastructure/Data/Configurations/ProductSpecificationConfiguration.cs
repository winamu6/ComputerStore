using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
    {
        public void Configure(EntityTypeBuilder<ProductSpecification> builder)
        {
            builder.ToTable("ProductSpecifications");

            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ps => ps.Value)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ps => ps.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(ps => ps.CreatedAt)
                .IsRequired();

            builder.Property(ps => ps.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(ps => ps.ProductId);
            builder.HasIndex(ps => ps.DisplayOrder);

            builder.HasOne(ps => ps.Product)
                .WithMany(p => p.Specifications)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
