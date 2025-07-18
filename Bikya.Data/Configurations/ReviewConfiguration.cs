using Bikya.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>

    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);

            // Required rating
            builder.Property(x => x.Rating)
                   .IsRequired();

            // Optional comment with a max length of 1000 characters
            builder.Property(x => x.Comment)
                   .HasMaxLength(1000);

            // Default value for CreatedAt is the current UTC date/time
            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Review is written *about* the seller
            builder.HasOne(x => x.Seller)
                   .WithMany(u => u.ReviewsReceived)
                   .HasForeignKey(x => x.SellerId)
                   .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

            // Review is written *by* the reviewer (usually the buyer)
            builder.HasOne(x => x.Reviewer)
                   .WithMany(u => u.ReviewsWritten)
                   .HasForeignKey(x => x.ReviewerId)
                   .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

            // Each review is linked to an order
            builder.HasOne(x => x.Order)
                   .WithMany(o => o.Reviews)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade); // Delete review if order is deleted






        }
    }
}
