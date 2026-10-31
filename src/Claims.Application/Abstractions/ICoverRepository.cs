using Claims.Domain;

namespace Claims.Application.Abstractions;

public interface ICoverRepository
{
    Task<IReadOnlyList<Cover>> ListAsync(CancellationToken cancellationToken);

    Task<Cover?> GetAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(Cover cover, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
