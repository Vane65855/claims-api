namespace Claims.Application.Abstractions;

public interface IAuditPublisher
{
    ValueTask PublishAsync(AuditEvent auditEvent);
}
