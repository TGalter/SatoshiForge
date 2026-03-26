using Microsoft.OpenApi;

namespace SatoshiForge.Api.Extensions;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SatoshiForge API",
                Version = "v1",
                Description = "API do projeto SatoshiForge"
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SatoshiForge API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}