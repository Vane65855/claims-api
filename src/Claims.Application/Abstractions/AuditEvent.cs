namespace Claims.Application.Abstractions;

public sealed record AuditEvent(
    Guid EventId,
    AuditEntityType EntityType,
    string EntityId,
    AuditOperation Operation,
    DateTime OccurredAtUtc);

public enum AuditEntityType
{
    Claim,
    Cover
}

public enum AuditOperation
{
    Created,
    Deleted
}
