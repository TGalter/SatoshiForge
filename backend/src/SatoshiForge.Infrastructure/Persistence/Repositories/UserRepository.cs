using Microsoft.EntityFrameworkCore;
using SatoshiForge.Application.Abstractions.Persistence;
using SatoshiForge.Domain.Entities;
using SatoshiForge.Infrastructure.Persistence;

namespace SatoshiForge.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }
}