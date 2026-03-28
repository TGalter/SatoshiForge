using SatoshiForge.Domain.Enums;

namespace SatoshiForge.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
    }

    private User(
        Guid id,
        string email,
        string passwordHash,
        UserRole role,
        bool isActive,
        DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            UserRole.Buyer,
            true,
            DateTime.UtcNow);
    }
}