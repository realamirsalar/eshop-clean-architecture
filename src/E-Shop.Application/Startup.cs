
using E_Shop.Application.PipelineBehaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace E_Shop.Application
{
    public static class Startup
    {
        public static IServiceCollection AddApplicationDIs(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationPipelineBehaviors<,>));
                cfg.AddOpenBehavior(typeof(CachePipelineBehavior<,>));
                cfg.AddOpenBehavior(typeof(CacheInvalidationPipelineBehavior<,>));
            });
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Startup).Assembly));
            services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
            return services;
        }
    }
}
