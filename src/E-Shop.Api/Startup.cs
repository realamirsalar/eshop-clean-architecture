using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

namespace E_Shop.Api
{
    public static class Startup
    {
        public static IServiceCollection AddApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            var serviceProvider = services.BuildServiceProvider();
            var apiVersionDescriptionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                services.AddOpenApi(description.GroupName, options =>
                {
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Info.Title = "E-Shop Api With clean architecture and modern practices made by Amirsalar Kheiry ; my email: amirsslar@gamil.com;";
                        document.Info.Version = description.ApiVersion.ToString();

                        if (description.IsDeprecated)
                        {
                            document.Info.Description = "This API version has been deprecated.";
                        }

                        return Task.CompletedTask;
                    });
                });
            }

            return services;
        }
    }
}
