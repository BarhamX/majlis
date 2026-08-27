using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class RevisionDialectConfiguration : IEntityTypeConfiguration<RevisionDialect>
{
    public void Configure(EntityTypeBuilder<RevisionDialect> builder)
    {
        builder.ToTable("RevisionDialects");
        builder.HasKey(dialect => new { dialect.RevisionId, dialect.DialectCode });
        builder.Property(dialect => dialect.DialectCode).HasColumnType("text");
        builder.HasOne<DailyMajlisRevision>()
            .WithMany(revision => revision.Dialects)
            .HasForeignKey(dialect => dialect.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
