
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<DevsService>();
builder.Services.AddScoped<AdminsService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<ExploreService>();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        options => { options.CommandTimeout(120); }
        );
});
// Caso deseja-se utilizar o banco de dados "InMemory" comentar
// deve-se comentar a configuração acima e descomentar a abaixo!
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseInMemoryDatabase("AppDb"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthentication();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});



var app = builder.Build();
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "User", "Developer", "Administrator" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (roleExists == false)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
}


app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.MapIdentityApi<IdentityUser>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
