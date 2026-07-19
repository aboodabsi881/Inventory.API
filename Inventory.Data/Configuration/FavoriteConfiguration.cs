using Inventory.Core.Entities.Favorites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Data.Configuration
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn();
            builder.Property(f => f.IsFavorite).IsRequired();


            //Relationships
            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Favorites)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("Fk_Favorites_Products");
            builder.ToTable("Favorites");
        }
    }

}
