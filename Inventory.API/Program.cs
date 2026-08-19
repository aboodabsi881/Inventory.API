using Inventory.Core.AutoMapperProfiles;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Inventory.Core.Services;
using Inventory.Data.Data;
using Inventory.Data.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


namespace Inventory.API
{
    public class Program
    {
        public static async Task Main(string[] args) // Entry point of the application
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConfigurationSitting")));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>() // Add Identity services for user and role management
                .AddEntityFrameworkStores<AppDbContext>();  // Add Entity Framework stores for Identity

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); //AddScoped : Registers a service with a scoped lifetime, meaning a new instance is created for each HTTP request.
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddAutoMapper(typeof(AccountsAutoMapperProfile).Assembly);

            builder.Services.AddEndpointsApiExplorer(); // Add API explorer for endpoint discovery
            builder.Services.AddSwaggerGen(); // Add Swagger generator for API documentation

            builder.Services.AddControllers() 
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Add JSON string enum converter
                });


            var app = builder.Build();

            // Configure Pipeline
            if (app.Environment.IsDevelopment()) 
            {
                app.UseSwagger(); // Enable Swagger middleware for API documentation
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection(); // Enable HTTPS redirection

            app.UseStaticFiles(); // Enable serving static files

            app.UseAuthentication(); // Enable authentication middleware

            app.UseAuthorization(); // Enable authorization middleware

            app.MapControllers(); // Map controller routes

            using (var scope = app.Services.CreateScope()) 
            {
                var services = scope.ServiceProvider; 
                await SeedService.SeedDatabaseAsync(services); 
            }

            app.Run(); 
        }
    }
}