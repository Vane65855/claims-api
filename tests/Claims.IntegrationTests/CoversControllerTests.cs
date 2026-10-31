using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Claims.IntegrationTests;

public class CoversControllerTests(ApiFixture fixture)
{
    [Fact]
    public async Task Deleting_a_cover_returns_204_and_then_404()
    {
        var ct = TestContext.Current.CancellationToken;
        var cover = await fixture.Client.CreateCoverAsync();

        var first = await fixture.Client.DeleteAsync($"/Covers/{cover.Id}", ct);
        var second = await fixture.Client.DeleteAsync($"/Covers/{cover.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    [Fact]
    public async Task A_posted_date_reads_back_unchanged()
    {
        var ct = TestContext.Current.CancellationToken;
        var startDate = TestApi.Today.AddDays(30).ToString("O");
        var endDate = TestApi.Today.AddDays(60).ToString("O");

        var response = await fixture.Client.PostAsJsonAsync("/Covers", new { startDate, endDate, type = "Yacht" }, ct);
        response.EnsureSuccessStatusCode();
        var cover = await fixture.Client.GetFromJsonAsync<JsonElement>(response.Headers.Location, ct);

        Assert.Equal(startDate, cover.GetProperty("startDate").GetString());
        Assert.Equal(endDate, cover.GetProperty("endDate").GetString());
    }

    [Fact]
    public async Task A_timestamp_for_a_date_field_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var startDate = $"{TestApi.Today.AddDays(30):O}T00:00:00";
        var endDate = TestApi.Today.AddDays(60).ToString("O");

        var response = await fixture.Client.PostAsJsonAsync("/Covers", new { startDate, endDate, type = "Yacht" }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.True(problem.GetProperty("errors").TryGetProperty("$.startDate", out _));
    }
}
