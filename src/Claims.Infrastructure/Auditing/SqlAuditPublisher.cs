using Claims.Application.Abstractions;

namespace Claims.Infrastructure.Auditing;

public sealed class SqlAuditPublisher(AuditContext db) : IAuditPublisher
{
    public async ValueTask PublishAsync(AuditEvent auditEvent)
    {
        db.Record(auditEvent);
        await db.SaveChangesAsync();
    }
}
