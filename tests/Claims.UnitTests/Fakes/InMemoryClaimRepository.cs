using Claims.Application.Abstractions;
using Claims.Domain;

namespace Claims.UnitTests.Fakes;

internal sealed class InMemoryClaimRepository : IClaimRepository
{
    private readonly Dictionary<string, Claim> _claims = [];

    public Task<IReadOnlyList<Claim>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Claim>>([.. _claims.Values]);

    public Task<Claim?> GetAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_claims.GetValueOrDefault(id));

    public Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        _claims.Add(claim.Id, claim);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_claims.Remove(id));
}
