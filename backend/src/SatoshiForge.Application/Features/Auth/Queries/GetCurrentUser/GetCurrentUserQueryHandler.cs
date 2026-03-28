using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Identity;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    ILogger<GetCurrentUserQueryHandler> logger,
    IUserRepository userRepository)
    : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    public async Task<GetCurrentUserResponse> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            logger.LogWarning("Get current user failed: user is not authenticated.");
            throw new UnauthorizedException("User is not authenticated.");
        }

        logger.LogInformation("Get current user requested for userId: {UserId}", currentUser.UserId);
        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Get current user failed for userId {UserId}: user not found or inactive.", currentUser.UserId);
            throw new NotFoundException("Authenticated user was not found.");
        }

        logger.LogInformation("Current user retrieved successfully for userId: {UserId}", currentUser.UserId);

        return new GetCurrentUserResponse(
            user.Id,
            user.Email,
            user.Role.ToString());
    }
}