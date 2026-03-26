using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Persistence;

namespace SatoshiForge.Application.Features.Auth.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Invalid credentials.");

        var passwordIsValid = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!passwordIsValid)
            throw new InvalidOperationException("Invalid credentials.");

        var accessToken = jwtTokenGenerator.Generate(user);

        return new LoginUserResponse(accessToken);
    }
}