using SatoshiForge.Application.Abstractions.CQRS;

namespace SatoshiForge.Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password) : ICommand<RegisterUserResponse>;