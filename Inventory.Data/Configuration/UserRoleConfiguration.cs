using Inventory.Core.Entities.Users.ApplicationUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Data.Configuration
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<int>> builder)
        {
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        }
    }
}