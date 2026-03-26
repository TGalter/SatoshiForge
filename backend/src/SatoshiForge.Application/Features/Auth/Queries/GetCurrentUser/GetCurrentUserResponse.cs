namespace SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string Role);