using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Claims.Application.Contracts;
using Claims.Domain;
using Xunit;

namespace Claims.IntegrationTests;

public class ClaimsControllerTests(ApiFixture fixture)
{
    [Fact]
    public async Task Listing_claims_returns_a_json_array()
    {
        var response = await fixture.Client.GetAsync("/Claims", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
    }

    [Fact]
    public async Task Thirty_requests_in_a_row_all_succeed()
    {
        for (var i = 0; i < 30; i++)
        {
            var response = await fixture.Client.GetAsync("/Claims", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Posting_a_claim_returns_201_with_a_location_that_resolves()
    {
        var ct = TestContext.Current.CancellationToken;
        var cover = await fixture.Client.CreateCoverAsync();

        var response = await fixture.Client.PostAsJsonAsync("/Claims", NewClaim(cover.Id, cover.StartDate), TestApi.Json, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ClaimResponse>(TestApi.Json, ct);
        var fetched = await fixture.Client.GetFromJsonAsync<ClaimResponse>(response.Headers.Location, TestApi.Json, ct);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Invalid_request_returns_camel_case_problem_details()
    {
        var ct = TestContext.Current.CancellationToken;
        var cover = await fixture.Client.CreateCoverAsync();
        var request = NewClaim(cover.Id, cover.StartDate) with { DamageCost = 100_000.01m };

        var response = await fixture.Client.PostAsJsonAsync("/Claims", request, TestApi.Json, ct);

        await response.AssertValidationErrorAsync("damageCost");
    }

    [Fact]
    public async Task A_claim_for_an_unknown_cover_is_rejected_on_cover_id()
    {
        var request = NewClaim(Guid.NewGuid().ToString(), TestApi.Today.AddDays(30));

        var response = await fixture.Client.PostAsJsonAsync(
            "/Claims", request, TestApi.Json, TestContext.Current.CancellationToken);

        await response.AssertValidationErrorAsync("coverId");
    }

    private static CreateClaimRequest NewClaim(string coverId, DateOnly created) => new()
    {
        CoverId = coverId,
        Created = created,
        Name = "Hull damage",
        Type = ClaimType.Collision,
        DamageCost = 1_000m
    };
}
