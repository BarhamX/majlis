using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence;

public sealed class MajlisDbContext(
    DbContextOptions<MajlisDbContext> options,
    TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<DailyMajlisEntity> DailyMajlis => Set<DailyMajlisEntity>();

    public DbSet<Challenge> Challenges => Set<Challenge>();

    public DbSet<ChallengeOption> ChallengeOptions => Set<ChallengeOption>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MajlisDbContext).Assembly);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<DailyMajlisEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property<DateTimeOffset>("CreatedAt").CurrentValue = utcNow;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property<DateTimeOffset>("UpdatedAt").CurrentValue = utcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Challenge>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property<DateTimeOffset>("CreatedAt").CurrentValue = utcNow;
            }
        }
    }
}
