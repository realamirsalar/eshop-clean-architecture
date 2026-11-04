using E_Shop.Application;
using E_Shop.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDatabaseConfigurations(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddInfrastructureDIs();
builder.Services.AddApplicationDIs();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "E-Shop.Api";
        options.Theme = ScalarTheme.DeepSpace;
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });
}


app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
