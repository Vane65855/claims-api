using Claims.Application.Abstractions;
using Claims.Domain;

namespace Claims.UnitTests.Fakes;

internal sealed class InMemoryCoverRepository(params Cover[] seed) : ICoverRepository
{
    private readonly Dictionary<string, Cover> _covers = seed.ToDictionary(cover => cover.Id);

    public Task<IReadOnlyList<Cover>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Cover>>([.. _covers.Values]);

    public Task<Cover?> GetAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_covers.GetValueOrDefault(id));

    public Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        _covers.Add(cover.Id, cover);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_covers.Remove(id));
}
