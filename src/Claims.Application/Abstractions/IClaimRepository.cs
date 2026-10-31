using Claims.Domain;

namespace Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> ListAsync(CancellationToken cancellationToken);

    Task<Claim?> GetAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(Claim claim, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
