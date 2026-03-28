using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;
using SatoshiForge.Application.Features.Auth.Commands.RegisterUser;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.UnitTests.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<RegisterUserCommandHandler> _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();

    [Fact]
    public async Task HandleAsync_Should_Create_User_When_Email_Does_Not_Exist()
    {
        var command = new RegisterUserCommand("user@test.com", "123456");
        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _logger, _unitOfWork);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.Hash(command.Password)
            .Returns("hashed-password");

        var response = await handler.HandleAsync(command);

        response.Email.Should().Be("user@test.com");
        response.Role.Should().Be("Buyer");

        await _userRepository.Received(1)
            .AddAsync(
                Arg.Is<User>(u =>
                    u.Email == "user@test.com" &&
                    u.PasswordHash == "hashed-password"),
                Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Email_Already_Exists()
    {
        var command = new RegisterUserCommand("user@test.com", "123456");
        var existingUser = User.Create("user@test.com", "hash");
        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _logger, _unitOfWork);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var act = async () => await handler.HandleAsync(command);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("A user with this email already exists.");

        await _userRepository.DidNotReceive()
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}