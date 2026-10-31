using Claims.Application.Abstractions;
using Claims.Application.Contracts;
using Claims.Domain;

namespace Claims.Application.Services;

public sealed class CoversService(ICoverRepository covers, IAuditPublisher audit, TimeProvider clock)
{
    public async Task<IReadOnlyList<CoverResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await covers.ListAsync(cancellationToken)).Select(cover => cover.ToResponse()).ToList();

    public async Task<CoverResponse?> GetAsync(string id, CancellationToken cancellationToken) =>
        (await covers.GetAsync(id, cancellationToken))?.ToResponse();

    public async Task<CoverResponse> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            Premium = Quote(request)
        };

        await covers.AddAsync(cover, cancellationToken);
        await PublishAsync(cover.Id, AuditOperation.Created);
        return cover.ToResponse();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!await covers.DeleteAsync(id, cancellationToken))
        {
            return false;
        }

        await PublishAsync(id, AuditOperation.Deleted);
        return true;
    }

    public decimal Quote(CreateCoverRequest request) =>
        PremiumCalculator.Calculate(request.Type, new InsurancePeriod(request.StartDate, request.EndDate).Days);

    private ValueTask PublishAsync(string coverId, AuditOperation operation) =>
        audit.PublishAsync(new AuditEvent(
            Guid.NewGuid(), AuditEntityType.Cover, coverId, operation, clock.GetUtcNow().UtcDateTime));
}
