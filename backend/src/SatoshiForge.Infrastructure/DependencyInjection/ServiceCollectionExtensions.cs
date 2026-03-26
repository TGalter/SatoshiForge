using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatoshiForge.Infrastructure.Persistence;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Infrastructure.Dispatching;
using FluentValidation;

namespace SatoshiForge.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddValidatorsFromAssembly(
            typeof(SatoshiForge.Application.Abstractions.CQRS.ICommand<>).Assembly);


        return services;
    }
}