using Claims.Application.Abstractions;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public sealed class ClaimRepository(ClaimsDbContext db) : IClaimRepository
{
    public async Task<IReadOnlyList<Claim>> ListAsync(CancellationToken cancellationToken) =>
        await db.Claims.ToListAsync(cancellationToken);

    public Task<Claim?> GetAsync(string id, CancellationToken cancellationToken) =>
        db.Claims.SingleOrDefaultAsync(claim => claim.Id == id, cancellationToken);

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        db.Claims.Add(claim);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await GetAsync(id, cancellationToken);
        if (claim is null)
        {
            return false;
        }

        db.Claims.Remove(claim);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
