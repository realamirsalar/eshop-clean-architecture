using E_Shop.Application.Common.Interfaces;
using E_Shop.Infrastructure.Data;
using E_Shop.Infrastructure.Persistence;
using E_Shop.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace E_Shop.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddDatabaseConfigurations(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            return services;
        }

        public static IServiceCollection AddInfrastructureDIs(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }

    }
}
