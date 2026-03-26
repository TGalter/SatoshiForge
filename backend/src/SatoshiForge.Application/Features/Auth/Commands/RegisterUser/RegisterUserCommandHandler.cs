using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(
            command.Email,
            cancellationToken);

        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Create(command.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(
            user.Id,
            user.Email,
            user.Role.ToString());
    }
}