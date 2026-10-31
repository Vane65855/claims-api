using Claims.Application.Abstractions;

namespace Claims.Infrastructure.Auditing;

public sealed class ClaimAudit
{
    public long Id { get; init; }

    public Guid EventId { get; init; }

    public required string ClaimId { get; init; }

    public AuditOperation Operation { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}
