using SatoshiForge.Application.Abstractions.CQRS;

namespace SatoshiForge.Application.Features.Auth.Commands.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<LoginUserResponse>;