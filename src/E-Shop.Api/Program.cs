using E_Shop.Api;
using E_Shop.Api.ExceptionHandlers;
using E_Shop.Application;
using E_Shop.Application.Common.CacheOptions;
using E_Shop.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();

builder.Services.AddDatabaseConfigurations(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddInfrastructureDIs();
builder.Services.AddApplicationDIs();


builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));


builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SectionName));

builder.Services.ConfigureCache(builder.Configuration);


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddApiVersioning();
builder.Services.AddVersioning();
builder.Services.AddOpenApi();



builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


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
