using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ci => ci.Quantity)
                .IsRequired();

            builder.Property(ci => ci.AddedDate)
                .IsRequired();

            builder.Property(ci => ci.CreatedAt)
                .IsRequired();

            builder.Property(ci => ci.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(ci => ci.UserId);
            builder.HasIndex(ci => ci.ProductId);
            builder.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();

            builder.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
