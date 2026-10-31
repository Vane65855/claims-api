using Claims.Application.Abstractions;
using Claims.Application.Contracts;
using Claims.Application.Services;
using Claims.Application.Validation;
using Claims.Domain;
using Claims.UnitTests.Fakes;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Claims.UnitTests;

public class ClaimsServiceTests
{
    private static readonly Cover Cover = new()
    {
        Id = "cover-1",
        StartDate = new DateOnly(2026, 11, 1),
        EndDate = new DateOnly(2026, 12, 1),
        Type = CoverType.Yacht,
        Premium = 41_250m
    };

    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
    private readonly InMemoryClaimRepository _claims = new();
    private readonly RecordingAuditPublisher _audit = new();
    private readonly ClaimsService _service;

    public ClaimsServiceTests()
    {
        _service = new ClaimsService(
            _claims, new InMemoryCoverRepository(Cover), new CreateClaimRequestValidator(), _audit, _clock);
    }

    [Fact]
    public async Task A_claim_on_the_cover_start_date_is_stored_and_audited()
    {
        var ct = TestContext.Current.CancellationToken;

        var claim = await _service.CreateAsync(Request(Cover.StartDate), ct);

        Assert.NotNull(await _claims.GetAsync(claim.Id, ct));
        var auditEvent = Assert.Single(_audit.Events);
        Assert.Equal(
            new AuditEvent(Guid.Empty, AuditEntityType.Claim, claim.Id, AuditOperation.Created, _clock.GetUtcNow().UtcDateTime),
            auditEvent with { EventId = Guid.Empty });
    }

    [Fact]
    public async Task A_claim_on_the_cover_end_date_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Request(Cover.EndDate), ct));

        Assert.Equal(nameof(CreateClaimRequest.Created), Assert.Single(exception.Errors).PropertyName);
        Assert.Empty(await _claims.ListAsync(ct));
        Assert.Empty(_audit.Events);
    }

    [Fact]
    public async Task A_claim_for_an_unknown_cover_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = Request(Cover.StartDate) with { CoverId = "missing" };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, ct));

        Assert.Equal(nameof(CreateClaimRequest.CoverId), Assert.Single(exception.Errors).PropertyName);
        Assert.Empty(await _claims.ListAsync(ct));
    }

    [Fact]
    public async Task Deleting_an_unknown_claim_publishes_no_audit_event()
    {
        var deleted = await _service.DeleteAsync("missing", TestContext.Current.CancellationToken);

        Assert.False(deleted);
        Assert.Empty(_audit.Events);
    }

    private static CreateClaimRequest Request(DateOnly created) => new()
    {
        CoverId = Cover.Id,
        Created = created,
        Name = "Hull damage",
        Type = ClaimType.Collision,
        DamageCost = 1_000m
    };
}
