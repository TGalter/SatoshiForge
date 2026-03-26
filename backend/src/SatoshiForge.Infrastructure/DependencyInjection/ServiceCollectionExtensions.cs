using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatoshiForge.Infrastructure.Persistence;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Infrastructure.Dispatching;
using FluentValidation;
using SatoshiForge.Infrastructure.Persistence.Repositories;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Features.Auth.Commands.RegisterUser;
using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Infrastructure.Auth;
using SatoshiForge.Infrastructure.Auth.Options;
using SatoshiForge.Application.Features.Auth.Commands.LoginUser;
using SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

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
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<ICommandHandler<LoginUserCommand, LoginUserResponse>, LoginUserCommandHandler>();

        services.AddScoped<IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>, GetCurrentUserQueryHandler>();

        services.AddValidatorsFromAssembly(
            typeof(SatoshiForge.Application.Abstractions.CQRS.ICommand<>).Assembly);


        return services;
    }
}