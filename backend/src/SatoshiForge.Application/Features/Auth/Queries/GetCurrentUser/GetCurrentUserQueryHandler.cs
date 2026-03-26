using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Identity;
using SatoshiForge.Application.Abstractions.Persistence;

namespace SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository)
    : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    public async Task<GetCurrentUserResponse> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new InvalidOperationException("User is not authenticated.");

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Authenticated user was not found.");

        return new GetCurrentUserResponse(
            user.Id,
            user.Email,
            user.Role.ToString());
    }
}