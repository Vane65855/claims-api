using System.Threading.Channels;
using Claims.Application.Abstractions;
using Claims.Infrastructure.Auditing;
using Claims.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.UnitTests;

public class AuditWorkerTests
{
    private readonly Channel<AuditEvent> _queue = Channel.CreateBounded<AuditEvent>(10);

    [Fact]
    public async Task Worker_writes_every_published_event()
    {
        var ct = TestContext.Current.CancellationToken;
        var store = new FakeAuditStore();
        store.Release();
        var worker = Worker(store);
        AuditEvent[] events = [NewEvent(), NewEvent(), NewEvent()];

        await worker.StartAsync(ct);
        foreach (var auditEvent in events)
        {
            await _queue.Writer.WriteAsync(auditEvent, ct);
        }

        Assert.Equal(events, await store.SavedAsync(3, ct));
        await worker.StopAsync(ct);
    }

    [Fact]
    public async Task Worker_drains_the_queue_when_it_is_stopped()
    {
        var ct = TestContext.Current.CancellationToken;
        var store = new FakeAuditStore();
        var worker = Worker(store);
        AuditEvent[] events = [NewEvent(), NewEvent(), NewEvent()];

        await worker.StartAsync(ct);
        foreach (var auditEvent in events)
        {
            await _queue.Writer.WriteAsync(auditEvent, ct);
        }

        await store.WaitForFirstCallAsync(ct);
        var stopping = worker.StopAsync(ct);
        store.Release();
        await stopping;

        Assert.Equal(events, await store.SavedAsync(3, ct));
    }

    [Fact]
    public async Task Worker_keeps_running_after_a_store_failure()
    {
        var ct = TestContext.Current.CancellationToken;
        var failing = NewEvent();
        var next = NewEvent();
        var store = new FakeAuditStore(failFor: failing.EventId);
        store.Release();
        var worker = Worker(store);

        await worker.StartAsync(ct);
        await _queue.Writer.WriteAsync(failing, ct);
        await _queue.Writer.WriteAsync(next, ct);

        Assert.Equal(next, Assert.Single(await store.SavedAsync(1, ct)));
        await worker.StopAsync(ct);
    }

    [Fact]
    public async Task Publishing_an_audit_event_puts_it_on_the_queue()
    {
        var auditEvent = NewEvent();

        await new ChannelAuditPublisher(_queue).PublishAsync(auditEvent);

        Assert.True(_queue.Reader.TryRead(out var queued));
        Assert.Equal(auditEvent, queued);
    }

    private AuditWorker Worker(FakeAuditStore store) => new(_queue, store, NullLogger<AuditWorker>.Instance);

    private static AuditEvent NewEvent() =>
        new(Guid.NewGuid(), AuditEntityType.Claim, Guid.NewGuid().ToString(), AuditOperation.Created, DateTime.UtcNow);
}
