namespace SatoshiForge.Domain.Entities;

public sealed class SystemInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
}