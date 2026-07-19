using Inventory.Core.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Data.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(p => p.Img).HasMaxLength(500);

            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

            builder.Property(p => p.PowerType).HasConversion<string>().HasMaxLength(50);



            //Relationships
            builder.HasMany(x => x.Favorites)
                   .WithOne(x => x.Product)
                   .HasForeignKey(x => x.ProductId);

            builder.HasMany(x => x.Cart)
                   .WithOne(x => x.Product)
                   .HasForeignKey(x => x.ProductId);

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("Fk_Products_Category");

            builder.ToTable("Products");
        }
    }
}
