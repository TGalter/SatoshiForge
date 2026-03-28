using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using SatoshiForge.Application.Abstractions.Observability;
using SatoshiForge.Infrastructure.Observability.Configuration;

namespace SatoshiForge.Infrastructure.Observability.Metrics;

internal sealed class AuthMetrics : IAuthMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _usersRegistered;
    private readonly Counter<long> _loginAttempts;

    public AuthMetrics(
        IMeterFactory meterFactory,
        IOptions<ObservabilityOptions> options)
    {
        var serviceName = options.Value.ServiceName;

        _meter = meterFactory.Create(serviceName);

        _usersRegistered = _meter.CreateCounter<long>(
            MetricNames.UsersRegistered,
            unit: "{user}",
            description: "Total de usuários registrados.");

        _loginAttempts = _meter.CreateCounter<long>(
            MetricNames.LoginAttempts,
            unit: "{attempt}",
            description: "Total de tentativas de login.");
    }

    public void UserRegistered(string role)
    {
        _usersRegistered.Add(1,
            new KeyValuePair<string, object?>(TagNames.UserRole, role));
    }

    public void LoginAttempt(bool succeeded)
    {
        _loginAttempts.Add(1,
            new KeyValuePair<string, object?>(
                TagNames.AuthResult,
                succeeded ? "success" : "failure"));
    }
}