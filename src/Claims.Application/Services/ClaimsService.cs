using Claims.Application.Abstractions;
using Claims.Application.Contracts;
using Claims.Domain;
using FluentValidation;
using FluentValidation.Results;

namespace Claims.Application.Services;

public sealed class ClaimsService(
    IClaimRepository claims,
    ICoverRepository covers,
    IValidator<CreateClaimRequest> validator,
    IAuditPublisher audit,
    TimeProvider clock)
{
    public async Task<IReadOnlyList<ClaimResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await claims.ListAsync(cancellationToken)).Select(claim => claim.ToResponse()).ToList();

    public async Task<ClaimResponse?> GetAsync(string id, CancellationToken cancellationToken) =>
        (await claims.GetAsync(id, cancellationToken))?.ToResponse();

    public async Task<ClaimResponse> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);
        await EnsureCoverIncludesAsync(request, cancellationToken);

        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        await claims.AddAsync(claim, cancellationToken);
        await PublishAsync(claim.Id, AuditOperation.Created);
        return claim.ToResponse();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!await claims.DeleteAsync(id, cancellationToken))
        {
            return false;
        }

        await PublishAsync(id, AuditOperation.Deleted);
        return true;
    }

    private async Task EnsureCoverIncludesAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var cover = await covers.GetAsync(request.CoverId, cancellationToken)
            ?? throw Invalid(nameof(request.CoverId), "Cover does not exist.");

        if (!cover.Period.Contains(request.Created))
        {
            throw Invalid(nameof(request.Created), "Created date must be within the cover period.");
        }
    }

    private static ValidationException Invalid(string propertyName, string message) =>
        new([new ValidationFailure(propertyName, message)]);

    private ValueTask PublishAsync(string claimId, AuditOperation operation) =>
        audit.PublishAsync(new AuditEvent(
            Guid.NewGuid(), AuditEntityType.Claim, claimId, operation, clock.GetUtcNow().UtcDateTime));
}
