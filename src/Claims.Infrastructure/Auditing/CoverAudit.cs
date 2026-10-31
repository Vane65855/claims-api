using Claims.Application.Abstractions;

namespace Claims.Infrastructure.Auditing;

public sealed class CoverAudit
{
    public long Id { get; init; }

    public Guid EventId { get; init; }

    public required string CoverId { get; init; }

    public AuditOperation Operation { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}
