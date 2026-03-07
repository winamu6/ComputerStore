using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.OrderDate)
                .IsRequired();

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(o => o.IsPaid)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.ShippingCountry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.SubTotal)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.ShippingCost)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            builder.Property(o => o.TrackingNumber)
                .HasMaxLength(100);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.OrderDate);
            builder.HasIndex(o => o.Status);

            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
