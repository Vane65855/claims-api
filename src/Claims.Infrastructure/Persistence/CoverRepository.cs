using Claims.Application.Abstractions;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public sealed class CoverRepository(ClaimsDbContext db) : ICoverRepository
{
    public async Task<IReadOnlyList<Cover>> ListAsync(CancellationToken cancellationToken) =>
        await db.Covers.ToListAsync(cancellationToken);

    public Task<Cover?> GetAsync(string id, CancellationToken cancellationToken) =>
        db.Covers.SingleOrDefaultAsync(cover => cover.Id == id, cancellationToken);

    public async Task AddAsync(Cover cover, CancellationToken cancellationToken)
    {
        db.Covers.Add(cover);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await GetAsync(id, cancellationToken);
        if (cover is null)
        {
            return false;
        }

        db.Covers.Remove(cover);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
