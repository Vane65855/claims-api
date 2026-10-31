using System.Threading.Channels;
using Claims.Application.Abstractions;

namespace Claims.Infrastructure.Auditing;

public sealed class ChannelAuditPublisher(Channel<AuditEvent> queue) : IAuditPublisher
{
    public ValueTask PublishAsync(AuditEvent auditEvent) => queue.Writer.WriteAsync(auditEvent);
}
