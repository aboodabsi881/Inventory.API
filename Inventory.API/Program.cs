using Inventory.Core.AutoMapperProfiles;
using Inventory.Core.Entities;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Data.Data;
using Inventory.Data.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConfigurationSitting")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>();


builder.Services.AddAutoMapper(typeof(AccountsAutoMapperProfile));
builder.Services.AddAutoMapper(typeof(ProductsAutoMapperProfile));
builder.Services.AddAutoMapper(typeof(CategoriesAutoMapperProfile));
builder.Services.AddAutoMapper(typeof(CartsAutoMapperProfile));
builder.Services.AddAutoMapper(typeof(FavoritesAutoMapperProfile));
builder.Services.AddAutoMapper(typeof(RolesAutoMapperProfile));


builder.Services.AddControllers();



var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles(); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedService.SeedDatabaseAsync(services);
}

app.Run();