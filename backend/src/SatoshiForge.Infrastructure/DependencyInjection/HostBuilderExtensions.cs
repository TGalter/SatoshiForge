using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using SatoshiForge.Infrastructure.Observability.Configuration;

namespace SatoshiForge.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructureLogging(
        this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<ObservabilityOptions>()
            .Bind(builder.Configuration.GetSection(ObservabilityOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(options.Otlp.Endpoint, UriKind.Absolute, out _),
                "Observability:Otlp:Endpoint deve ser uma URI absoluta válida.")
            .Validate(
                options => options.Otlp.Protocol is "grpc" or "http/protobuf",
                "Observability:Otlp:Protocol deve ser 'grpc' ou 'http/protobuf'.")
            .ValidateOnStart();

        var options = builder.Configuration
            .GetRequiredSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>()
            ?? throw new InvalidOperationException("A seção Observability é obrigatória.");

        var serviceVersion =
            Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
            ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            ?? "unknown";

        if (!options.EnableLogs)
        {
            return builder;
        }

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;

            logging.SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(options.ServiceName, serviceVersion: serviceVersion)
                    .AddAttributes(
                        options.ResourceAttributes.Select(x =>
                            new KeyValuePair<string, object>(x.Key, x.Value))));

            logging.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(options.Otlp.Endpoint);
                exporter.Protocol = options.Otlp.Protocol switch
                {
                    "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
                    _ => OtlpExportProtocol.Grpc
                };
            });
        });

        return builder;
    }
}