using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SatoshiForge.Application.Abstractions.Auth;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;
using SatoshiForge.Application.Features.Auth.Commands.LoginUser;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.UnitTests.Features.Auth.Commands.LoginUser;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly ILogger<LoginUserCommandHandler> _logger = Substitute.For<ILogger<LoginUserCommandHandler>>();

    [Fact]
    public async Task HandleAsync_Should_Return_Token_When_Credentials_Are_Valid()
    {
        var command = new LoginUserCommand("user@test.com", "123456");
        var user = User.Create("user@test.com", "hashed-password");
        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _logger, _jwtTokenGenerator);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(command.Password, user.PasswordHash)
            .Returns(true);

        _jwtTokenGenerator.Generate(user)
            .Returns("jwt-token");

        var response = await handler.HandleAsync(command);

        response.AccessToken.Should().Be("jwt-token");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_UnauthorizedException_When_User_Does_Not_Exist()
    {
        var command = new LoginUserCommand("user@test.com", "123456");
        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _logger, _jwtTokenGenerator);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await handler.HandleAsync(command);

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_UnauthorizedException_When_Password_Is_Invalid()
    {
        var command = new LoginUserCommand("user@test.com", "123456");
        var user = User.Create("user@test.com", "hashed-password");
        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _logger, _jwtTokenGenerator);

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(command.Password, user.PasswordHash)
            .Returns(false);

        var act = async () => await handler.HandleAsync(command);

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }
}