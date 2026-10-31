using Claims.Application.Contracts;
using Claims.Application.Services;
using Claims.Application.Validation;
using Claims.Domain;
using Claims.UnitTests.Fakes;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Claims.UnitTests;

public class CoversServiceTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
    private readonly InMemoryCoverRepository _covers = new();
    private readonly RecordingAuditPublisher _audit = new();
    private readonly CoversService _service;

    public CoversServiceTests()
    {
        _service = new CoversService(_covers, new CreateCoverRequestValidator(_clock), _audit, _clock);
    }

    [Fact]
    public async Task A_new_cover_is_priced_stored_and_audited()
    {
        var ct = TestContext.Current.CancellationToken;

        var cover = await _service.CreateAsync(Request(new(2026, 11, 1), new(2026, 12, 1)), ct);

        Assert.Equal(41_250m, cover.Premium);
        Assert.NotNull(await _covers.GetAsync(cover.Id, ct));
        Assert.Single(_audit.Events);
    }

    [Fact]
    public async Task An_invalid_cover_is_neither_stored_nor_audited()
    {
        var ct = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(Request(new(2026, 12, 1), new(2026, 11, 1)), ct));

        Assert.Empty(await _covers.ListAsync(ct));
        Assert.Empty(_audit.Events);
    }

    [Fact]
    public void A_quote_is_validated_like_a_new_cover() =>
        Assert.Throws<ValidationException>(() => _service.Quote(Request(new(2026, 10, 29), new(2026, 11, 29))));

    private static CreateCoverRequest Request(DateOnly startDate, DateOnly endDate) =>
        new() { StartDate = startDate, EndDate = endDate, Type = CoverType.Yacht };
}
