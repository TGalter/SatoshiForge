using Microsoft.EntityFrameworkCore;
using SatoshiForge.Domain.Entities;

namespace SatoshiForge.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SystemInfo> SystemInfos => Set<SystemInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemInfo>(entity =>
        {
            entity.ToTable("system_infos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}