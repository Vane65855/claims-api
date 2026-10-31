using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
}
