namespace SatoshiForge.Application.Common.Responses;

public sealed class SystemPingResponse
{
    public string Message { get; init; } = default!;
    public string Version { get; init; } = default!;
}