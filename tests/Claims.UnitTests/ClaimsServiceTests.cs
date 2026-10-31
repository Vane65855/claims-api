using Claims.Application.Services;
using Claims.UnitTests.Fakes;
using Xunit;

namespace Claims.UnitTests;

public class ClaimsServiceTests
{
    private readonly RecordingAuditPublisher _audit = new();
    private readonly ClaimsService _service;

    public ClaimsServiceTests()
    {
        _service = new ClaimsService(new InMemoryClaimRepository(), _audit, TimeProvider.System);
    }

    [Fact]
    public async Task Deleting_an_unknown_claim_publishes_no_audit_event()
    {
        var deleted = await _service.DeleteAsync("missing", TestContext.Current.CancellationToken);

        Assert.False(deleted);
        Assert.Empty(_audit.Events);
    }
}
