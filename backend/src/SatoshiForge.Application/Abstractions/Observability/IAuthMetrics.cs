namespace SatoshiForge.Application.Abstractions.Observability;

public interface IAuthMetrics
{
    void UserRegistered(string role);
    void LoginAttempt(bool succeeded);
}