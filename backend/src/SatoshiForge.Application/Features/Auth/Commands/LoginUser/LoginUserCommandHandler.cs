using Microsoft.Extensions.Logging;
using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;

namespace SatoshiForge.Application.Features.Auth.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<LoginUserCommandHandler> logger,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Login requested for email: {Email}", command.Email);
        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Login failed for email {Email}: invalid credentials or inactive user.", command.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var passwordIsValid = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            logger.LogWarning("Login failed for email {Email}: invalid credentials.", command.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var accessToken = jwtTokenGenerator.Generate(user);
        logger.LogInformation("User {userId} successfully logged in.", user.Id);
        return new LoginUserResponse(accessToken);
    }
}