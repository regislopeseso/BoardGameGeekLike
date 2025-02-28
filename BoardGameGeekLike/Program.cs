
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Properties;
using BoardGameGeekLike.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#region Code to force Json accept DateOnly in the DD/MM/YYYY format
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });
#endregion

builder.Services.AddOpenApi();

builder.Services.AddScoped<DevsService>();
builder.Services.AddScoped<AdminsService>();
builder.Services.AddScoped<UsersService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        options => { options.CommandTimeout(120); }
        );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
