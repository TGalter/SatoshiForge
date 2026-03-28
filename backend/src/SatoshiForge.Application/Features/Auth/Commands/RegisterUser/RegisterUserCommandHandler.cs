using Microsoft.Extensions.Logging;
using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUserCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Register user requested for email: {Email}", command.Email);

        var existingUser = await userRepository.GetByEmailAsync(
            command.Email,
            cancellationToken);

        if (existingUser is not null)
        {
            logger.LogWarning("Register user conflict for email: {Email}", command.Email);
            throw new ConflictException("A user with this email already exists.");
        }

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Create(command.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {userId} successfully registered with role {Role}", user.Id, user.Role);

        return new RegisterUserResponse(
            user.Id,
            user.Email,
            user.Role.ToString());
    }
}