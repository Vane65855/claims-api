using Claims.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Auditing;

public sealed class AuditContext(DbContextOptions<AuditContext> options) : DbContext(options)
{
    public DbSet<ClaimAudit> ClaimAudits => Set<ClaimAudit>();

    public DbSet<CoverAudit> CoverAudits => Set<CoverAudit>();

    public void Record(AuditEvent auditEvent)
    {
        object row = auditEvent.EntityType switch
        {
            AuditEntityType.Claim => new ClaimAudit
            {
                EventId = auditEvent.EventId,
                ClaimId = auditEvent.EntityId,
                Operation = auditEvent.Operation,
                OccurredAtUtc = auditEvent.OccurredAtUtc
            },
            AuditEntityType.Cover => new CoverAudit
            {
                EventId = auditEvent.EventId,
                CoverId = auditEvent.EntityId,
                Operation = auditEvent.Operation,
                OccurredAtUtc = auditEvent.OccurredAtUtc
            },
            _ => throw new ArgumentOutOfRangeException(nameof(auditEvent))
        };

        Add(row);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.Entity<ClaimAudit>(audit =>
        {
            audit.Property(a => a.ClaimId).HasMaxLength(36);
            audit.Property(a => a.Operation).HasConversion<string>().HasMaxLength(16);
            audit.HasIndex(a => a.EventId).IsUnique();
            audit.HasIndex(a => new { a.ClaimId, a.OccurredAtUtc });
        });

        modelBuilder.Entity<CoverAudit>(audit =>
        {
            audit.Property(a => a.CoverId).HasMaxLength(36);
            audit.Property(a => a.Operation).HasConversion<string>().HasMaxLength(16);
            audit.HasIndex(a => a.EventId).IsUnique();
            audit.HasIndex(a => new { a.CoverId, a.OccurredAtUtc });
        });
    }
}
