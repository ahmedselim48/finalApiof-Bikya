﻿using Bikya.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            
            builder.HasKey(c => c.Id);

           
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Description)
                   .HasMaxLength(300);

            builder.Property(c => c.IconUrl)
                   .HasMaxLength(300);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");




            builder.HasMany(c => c.Products)
                   .WithOne(p => p.Category)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
