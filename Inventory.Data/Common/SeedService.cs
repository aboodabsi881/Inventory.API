using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Data.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Data.Common
{
    public class SeedService
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Ensure migrations are fully executed on startup safely instead of just EnsureCreated
                logger.LogInformation("Applying missing pending database migrations...");
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                // Seeding System Roles
                logger.LogInformation("Seeding default identity system roles.");
                await AddRoleAsync(roleManager, "SuperAdmin");
                await AddRoleAsync(roleManager, "Admin");
                await AddRoleAsync(roleManager, "User");

                // Seeding Super Admin Configuration Profile
                var superAdmins = new[]
                {
                    new
                    {
                        Email = "abood2004absi@gmail.com",
                        Username = "aboodAbsi",
                        NameAr = "عبدالرحمن العباسي",
                        NameEn = "Abdalrahman Alabsi"
                    }
                };

                foreach (var admin in superAdmins)
                {
                    if (await userManager.FindByEmailAsync(admin.Email) == null)
                    {
                        logger.LogInformation("Super Admin identity record missing. Creating seed instance...");

                        var superAdminUser = new ApplicationUser
                        {
                            Email = admin.Email,
                            UserName = admin.Username,
                            NameAr = admin.NameAr,
                            NameEn = admin.NameEn,
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            PasswordByte = Encoding.UTF8.GetBytes("Super@123") // Preserves your custom byte storage logic
                        };

                        var result = await userManager.CreateAsync(superAdminUser, "Super@123");

                        if (result.Succeeded)
                        {
                            logger.LogInformation("Assigning 'SuperAdmin' role credentials to user.");
                            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            logger.LogError("Failed to create user sequence during deployment: {Errors}", errors);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A critical underlying exception occurred while seeding system entities.");
            }
        }

        private static async Task AddRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to populate infrastructure system role '{roleName}': {errors}");
                }
            }
        }
    }
}