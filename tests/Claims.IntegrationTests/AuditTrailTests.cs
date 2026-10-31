using System.Net;
using Claims.Application.Abstractions;
using Claims.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Claims.IntegrationTests;

public class AuditTrailTests(ApiFixture fixture)
{
    [Fact]
    public async Task A_created_claim_is_audited_once()
    {
        var claim = await fixture.Client.CreateClaimAsync();

        var rows = await ClaimAuditsAsync(claim.Id, count: 1);

        Assert.Equal(AuditOperation.Created, Assert.Single(rows).Operation);
    }

    [Fact]
    public async Task Deleting_a_claim_is_audited_once_and_deleting_it_again_is_not()
    {
        var ct = TestContext.Current.CancellationToken;
        var claim = await fixture.Client.CreateClaimAsync();

        var first = await fixture.Client.DeleteAsync($"/Claims/{claim.Id}", ct);
        var second = await fixture.Client.DeleteAsync($"/Claims/{claim.Id}", ct);
        var later = await fixture.Client.CreateClaimAsync();
        await ClaimAuditsAsync(later.Id, count: 1);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        var rows = await ClaimAuditsAsync(claim.Id, count: 2);
        Assert.Equal([AuditOperation.Created, AuditOperation.Deleted], rows.Select(row => row.Operation));
    }

    private async Task<List<ClaimAudit>> ClaimAuditsAsync(string claimId, int count)
    {
        var ct = TestContext.Current.CancellationToken;
        var contexts = fixture.Services.GetRequiredService<IDbContextFactory<AuditContext>>();
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (true)
        {
            await using var db = await contexts.CreateDbContextAsync(ct);
            var rows = await db.ClaimAudits.Where(row => row.ClaimId == claimId).OrderBy(row => row.Id).ToListAsync(ct);
            if (rows.Count >= count || DateTime.UtcNow > deadline)
            {
                return rows;
            }

            await Task.Delay(100, ct);
        }
    }
}
