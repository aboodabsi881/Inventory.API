using Inventory.Core.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Data.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(c => c.Img).HasMaxLength(500);

            builder.HasMany(x => x.Products) 
                   .WithOne(x => x.Category)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("Fk_Category_Product");

            builder.ToTable("Categories");
        }
    }
}
