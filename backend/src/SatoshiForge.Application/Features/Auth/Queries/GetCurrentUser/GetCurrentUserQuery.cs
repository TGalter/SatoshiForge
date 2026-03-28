using SatoshiForge.Application.Abstractions.CQRS;

namespace SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<GetCurrentUserResponse>;