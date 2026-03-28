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
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SatoshiForge.Infrastructure.Observability.Metrics;
using SatoshiForge.Application.Abstractions.Observability;
using SatoshiForge.Infrastructure.Observability.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace SatoshiForge.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment? environment)
    {
        if (environment?.IsEnvironment("Testing") != true)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services
    .AddOptions<ObservabilityOptions>()
    .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => Uri.TryCreate(options.Otlp.Endpoint, UriKind.Absolute, out _),
        "Observability:Otlp:Endpoint deve ser uma URI absoluta válida.")
    .Validate(
        options => options.Otlp.Protocol is "grpc" or "http/protobuf",
        "Observability:Otlp:Protocol deve ser 'grpc' ou 'http/protobuf'.")
    .ValidateOnStart();

        var observabilityOptions = configuration
            .GetRequiredSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>()
            ?? throw new OptionsValidationException(
                ObservabilityOptions.SectionName,
                typeof(ObservabilityOptions),
                ["A seção Observability é obrigatória."]);

        var serviceVersion =
            Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
            ?? Assembly.GetEntryAssembly()?
                .GetName()
                .Version?
                .ToString()
            ?? "unknown";

        var openTelemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: observabilityOptions.ServiceName,
                    serviceVersion: serviceVersion);

                if (observabilityOptions.ResourceAttributes.Count > 0)
                {
                    resource.AddAttributes(
                        observabilityOptions.ResourceAttributes
                            .Select(x => new KeyValuePair<string, object>(x.Key, x.Value)));
                }
            });

        if (observabilityOptions.EnableMetrics)
        {
            openTelemetry.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(observabilityOptions.ServiceName)
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(observabilityOptions.Otlp.Endpoint);
                        exporter.Protocol = observabilityOptions.Otlp.Protocol switch
                        {
                            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
                            _ => OtlpExportProtocol.Grpc
                        };
                    });
            });
        }

        if (observabilityOptions.EnableMetrics)
        {
            openTelemetry.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(observabilityOptions.ServiceName)
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(observabilityOptions.Otlp.Endpoint);
                        exporter.Protocol = observabilityOptions.Otlp.Protocol switch
                        {
                            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
                            _ => OtlpExportProtocol.Grpc
                        };
                    });
            });
        }

        services.AddSingleton<IAuthMetrics, AuthMetrics>();

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

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