using System.Threading.Channels;
using Claims.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Auditing;

public sealed class AuditWorker(Channel<AuditEvent> queue, IAuditStore store, ILogger<AuditWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken _)
    {
        await foreach (var auditEvent in queue.Reader.ReadAllAsync())
        {
            try
            {
                await store.SaveAsync(auditEvent);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not store audit event {EventId}.", auditEvent.EventId);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Writer.TryComplete();
        return base.StopAsync(cancellationToken);
    }
}
