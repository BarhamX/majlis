using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence;

public sealed class MajlisDbContext(
    DbContextOptions<MajlisDbContext> options,
    TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<DailyMajlisEntity> DailyMajlis => Set<DailyMajlisEntity>();

    public DbSet<DailyMajlisPublication> DailyMajlisPublications => Set<DailyMajlisPublication>();

    public DbSet<Challenge> Challenges => Set<Challenge>();

    public DbSet<ChallengeOption> ChallengeOptions => Set<ChallengeOption>();

    public DbSet<DailyMajlisRevision> DailyMajlisRevisions => Set<DailyMajlisRevision>();

    public DbSet<DailyMajlisTranslation> DailyMajlisTranslations => Set<DailyMajlisTranslation>();

    public DbSet<ChallengeOptionTranslation> ChallengeOptionTranslations => Set<ChallengeOptionTranslation>();

    public DbSet<RevisionRegion> RevisionRegions => Set<RevisionRegion>();

    public DbSet<RevisionDialect> RevisionDialects => Set<RevisionDialect>();

    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>();

    public DbSet<UserProfile> Profiles => Set<UserProfile>();

    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    public DbSet<UserConsent> UserConsents => Set<UserConsent>();

    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();

    public DbSet<AccountDeletionRequest> AccountDeletionRequests => Set<AccountDeletionRequest>();

    public DbSet<UserAttempt> UserAttempts => Set<UserAttempt>();

    public DbSet<XpLedgerEntry> XpLedger => Set<XpLedgerEntry>();

    public DbSet<UserProgress> UserProgress => Set<UserProgress>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

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

    }
}
