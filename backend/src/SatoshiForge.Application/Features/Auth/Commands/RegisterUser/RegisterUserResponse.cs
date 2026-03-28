namespace SatoshiForge.Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string Role);