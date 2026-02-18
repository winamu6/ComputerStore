using ComputerStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerStore.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.IsVerifiedPurchase)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.IsApproved)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(r => r.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.HelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.NotHelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.CustomerId);
            builder.HasIndex(r => r.IsApproved);
            builder.HasIndex(r => r.Rating);
            builder.HasIndex(r => r.IsDeleted);

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");
        }
    }
}
