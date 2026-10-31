using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Claims.Application.Contracts;
using Claims.Domain;
using Xunit;

namespace Claims.IntegrationTests;

public class CoversControllerTests(ApiFixture fixture)
{
    [Fact]
    public async Task Posting_a_valid_cover_returns_the_calculated_premium()
    {
        var ct = TestContext.Current.CancellationToken;
        var start = TestApi.Today.AddDays(30);
        var request = new CreateCoverRequest { StartDate = start, EndDate = start.AddDays(45), Type = CoverType.Yacht };

        var response = await fixture.Client.PostAsJsonAsync("/Covers", request, TestApi.Json, ct);
        var created = await response.Content.ReadFromJsonAsync<CoverResponse>(TestApi.Json, ct);
        var fetched = await fixture.Client.GetFromJsonAsync<CoverResponse>(response.Headers.Location, TestApi.Json, ct);

        Assert.Equal(60_843.75m, created!.Premium);
        Assert.Equal(60_843.75m, fetched!.Premium);
    }

    [Fact]
    public async Task A_cover_longer_than_one_year_is_rejected()
    {
        var start = TestApi.Today.AddDays(30);
        var request = new CreateCoverRequest
        {
            StartDate = start,
            EndDate = start.AddYears(1).AddDays(1),
            Type = CoverType.Tanker
        };

        var response = await fixture.Client.PostAsJsonAsync(
            "/Covers", request, TestApi.Json, TestContext.Current.CancellationToken);

        await response.AssertValidationErrorAsync("endDate");
    }

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
        var startDate = $"{TestApi.Today.AddDays(30):O}T00:00:00";
        var endDate = TestApi.Today.AddDays(60).ToString("O");

        var response = await fixture.Client.PostAsJsonAsync(
            "/Covers", new { startDate, endDate, type = "Yacht" }, TestContext.Current.CancellationToken);

        await response.AssertValidationErrorAsync("$.startDate");
    }

    [Fact]
    public async Task Enums_are_accepted_by_name_only()
    {
        var ct = TestContext.Current.CancellationToken;
        var startDate = TestApi.Today.AddDays(30).ToString("O");
        var endDate = TestApi.Today.AddDays(60).ToString("O");

        var byNumber = await fixture.Client.PostAsJsonAsync("/Covers", new { startDate, endDate, type = 0 }, ct);
        var byName = await fixture.Client.PostAsJsonAsync("/Covers", new { startDate, endDate, type = "Yacht" }, ct);

        await byNumber.AssertValidationErrorAsync("$.type");
        Assert.Equal(HttpStatusCode.Created, byName.StatusCode);
    }

    [Fact]
    public async Task Computing_a_premium_returns_the_price()
    {
        var ct = TestContext.Current.CancellationToken;
        var start = TestApi.Today.AddDays(30);

        var response = await fixture.Client.PostAsync(
            $"/Covers/compute?startDate={start:O}&endDate={start.AddDays(30):O}&coverType=Yacht", null, ct);

        response.EnsureSuccessStatusCode();
        Assert.Equal(41_250m, await response.Content.ReadFromJsonAsync<decimal>(ct));
    }

    [Fact]
    public async Task Computing_a_premium_for_an_empty_period_is_rejected()
    {
        var day = TestApi.Today.AddDays(30).ToString("O");

        var response = await fixture.Client.PostAsync(
            $"/Covers/compute?startDate={day}&endDate={day}&coverType=Yacht", null, TestContext.Current.CancellationToken);

        await response.AssertValidationErrorAsync("endDate");
    }
}
