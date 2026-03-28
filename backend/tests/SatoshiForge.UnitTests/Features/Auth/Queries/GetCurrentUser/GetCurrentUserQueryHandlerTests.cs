using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SatoshiForge.Application.Abstractions.Identity;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;
using SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.UnitTests.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ILogger<GetCurrentUserQueryHandler> _logger = Substitute.For<ILogger<GetCurrentUserQueryHandler>>();

    [Fact]
    public async Task HandleAsync_Should_Return_Current_User_When_Authenticated()
    {
        var query = new GetCurrentUserQuery();
        var user = User.Create("user@test.com", "hash");
        var handler = new GetCurrentUserQueryHandler(_currentUser, _logger, _userRepository);

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(user.Id);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var response = await handler.HandleAsync(query);

        response.Id.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.Role.Should().Be("Buyer");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_UnauthorizedException_When_User_Is_Not_Authenticated()
    {
        var query = new GetCurrentUserQuery();
        var handler = new GetCurrentUserQueryHandler(_currentUser, _logger, _userRepository);

        _currentUser.IsAuthenticated.Returns(false);
        _currentUser.UserId.Returns((Guid?)null);

        var act = async () => await handler.HandleAsync(query);

        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_User_Does_Not_Exist()
    {
        var query = new GetCurrentUserQuery();
        var userId = Guid.NewGuid();
        var handler = new GetCurrentUserQueryHandler(_currentUser, _logger, _userRepository);

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = async () => await handler.HandleAsync(query);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Authenticated user was not found.");
    }
}