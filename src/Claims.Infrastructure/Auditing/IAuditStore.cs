using Claims.Application.Abstractions;

namespace Claims.Infrastructure.Auditing;

public interface IAuditStore
{
    Task SaveAsync(AuditEvent auditEvent);
}
