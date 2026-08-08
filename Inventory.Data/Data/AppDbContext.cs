using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Categories;
using Inventory.Core.Entities.Favorites;
using Inventory.Core.Entities.Products;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Inventory.Data.Configuration;

namespace Inventory.Data.Data
{
        public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
            {
            }

            public DbSet<Category> Categories { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<Cart> Carts { get; set; }
            public DbSet<Favorite> Favorites { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
                modelBuilder.ApplyConfiguration(new CategoryConfiguration());
                modelBuilder.ApplyConfiguration(new ProductConfiguration());
                modelBuilder.ApplyConfiguration(new CartConfiguration());
                modelBuilder.ApplyConfiguration(new FavoriteConfiguration());
            }

        }
    }
