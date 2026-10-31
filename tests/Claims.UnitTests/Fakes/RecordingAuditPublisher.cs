using Claims.Application.Abstractions;

namespace Claims.UnitTests.Fakes;

internal sealed class RecordingAuditPublisher : IAuditPublisher
{
    public List<AuditEvent> Events { get; } = [];

    public ValueTask PublishAsync(AuditEvent auditEvent)
    {
        Events.Add(auditEvent);
        return ValueTask.CompletedTask;
    }
}
