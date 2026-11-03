using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace E_Shop.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddDatabaseConfigurations(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<Data.ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            return services;
        }
    }
}
