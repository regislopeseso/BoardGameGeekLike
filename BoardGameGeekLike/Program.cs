
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();  
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
// Caso deseja-se utilizar o banco de dados "InMemory"
// deve-se comentar a configuração acima e descomentar a abaixo!
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseInMemoryDatabase("AppDb"));


// Configuração do CORS para permitir requisições de qualquer domínio
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(name: "AllowAll",
//        policy => policy.WithOrigins("http://127.0.0.1:5500")        
//                        .AllowAnyHeader()
//                        .AllowAnyMethod()
//                        .AllowCredentials());
//});

// Configuração do CORS para permitir requisições de um domínio específico
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "JPROnly",
        policy => policy.WithOrigins("http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});


builder.Services.AddIdentity<User, IdentityRole>(options => 
{
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 30;
    options.Lockout.AllowedForNewUsers = true;

    // Optional: enforce strong passwords
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
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

    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // optional timeout
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
    options.Cookie.Expiration = null; // <-- this ensures it's a session cookie
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


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.MapIdentityApi<IdentityUser>();


app.UseHttpsRedirection(); // Redirect HTTP to HTTPS (first for security)

app.UseRouting(); // Sets up routing for endpoints

app.UseCors("JPROnly");  // <-- Use your named policy here

app.UseAuthentication(); // Handles authentication (if enabled)
 
app.UseAuthorization(); // Handles authorization (must follow authentication)

app.MapControllers(); // Maps your API controllers

app.Run(); // Starts the application
