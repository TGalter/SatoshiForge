using System.ComponentModel.DataAnnotations;

namespace SatoshiForge.Infrastructure.Observability.Configuration;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    [Required]
    public string ServiceName { get; init; } = string.Empty;

    public bool EnableMetrics { get; init; } = true;

    public bool EnableTracing { get; init; } = true;
    
    public bool EnableLogs { get; init; } = true;

    public bool EnableStructuredLogging { get; init; } = true;

    [Required]
    public OtlpOptions Otlp { get; init; } = new();

    public Dictionary<string, string> ResourceAttributes { get; init; } = new();
}

public sealed class OtlpOptions
{
    [Required]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    public string Protocol { get; init; } = "grpc";
}