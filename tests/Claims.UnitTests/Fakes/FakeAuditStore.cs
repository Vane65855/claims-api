using System.Threading.Channels;
using Claims.Application.Abstractions;
using Claims.Infrastructure.Auditing;

namespace Claims.UnitTests.Fakes;

internal sealed class FakeAuditStore(Guid? failFor = null) : IAuditStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource _called = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Channel<AuditEvent> _saved = Channel.CreateUnbounded<AuditEvent>();

    public Task WaitForFirstCallAsync(CancellationToken cancellationToken) =>
        _called.Task.WaitAsync(Timeout, cancellationToken);

    public void Release() => _released.TrySetResult();

    public async Task SaveAsync(AuditEvent auditEvent)
    {
        _called.TrySetResult();
        await _released.Task;

        if (auditEvent.EventId == failFor)
        {
            throw new InvalidOperationException("The store is down.");
        }

        _saved.Writer.TryWrite(auditEvent);
    }

    public async Task<List<AuditEvent>> SavedAsync(int count, CancellationToken cancellationToken)
    {
        var saved = new List<AuditEvent>();
        while (saved.Count < count)
        {
            saved.Add(await _saved.Reader.ReadAsync(cancellationToken).AsTask().WaitAsync(Timeout, cancellationToken));
        }

        return saved;
    }
}
