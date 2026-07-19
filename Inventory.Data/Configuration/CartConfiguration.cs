using Inventory.Core.Entities.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Data.Configuration
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();
            builder.Property(c => c.Quantity).IsRequired();
            builder.Property(c => c.TotalPrice).HasColumnType("decimal(18,2)");


            //Relationships
            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Cart)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("Fk_Carts_Products");

            // Table name
            builder.ToTable("Carts");
        }

    }
}
