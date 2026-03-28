using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using Serilog;
using SatoshiForge.Infrastructure.Observability.Configuration;

namespace SatoshiForge.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructureLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        var observabilityOptions = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>()
            ?? throw new OptionsValidationException(
                ObservabilityOptions.SectionName,
                typeof(ObservabilityOptions),
                ["A seção Observability é obrigatória."]);

        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();

            if (!observabilityOptions.EnableStructuredLogging)
            {
                return;
            }

            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = observabilityOptions.Otlp.Endpoint;
                options.Protocol = observabilityOptions.Otlp.Protocol switch
                {
                    "http/protobuf" => Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf,
                    _ => Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc
                };
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = observabilityOptions.ServiceName
                };
            });
        });

        return hostBuilder;
    }
}