using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Application.Contracts;
using Claims.Domain;
using Xunit;

namespace Claims.IntegrationTests;

internal static class TestApi
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public static async Task<CoverResponse> CreateCoverAsync(this HttpClient client)
    {
        var request = new CreateCoverRequest
        {
            StartDate = Today.AddDays(30),
            EndDate = Today.AddDays(60),
            Type = CoverType.Yacht
        };

        var response = await client.PostAsJsonAsync("/Covers", request, Json, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CoverResponse>(Json, TestContext.Current.CancellationToken))!;
    }

    public static async Task<ClaimResponse> CreateClaimAsync(this HttpClient client)
    {
        var cover = await client.CreateCoverAsync();

        var response = await client.PostAsJsonAsync(
            "/Claims", NewClaim(cover.Id, cover.StartDate), Json, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClaimResponse>(Json, TestContext.Current.CancellationToken))!;
    }

    public static CreateClaimRequest NewClaim(string coverId, DateOnly created) => new()
    {
        CoverId = coverId,
        Created = created,
        Name = "Hull damage",
        Type = ClaimType.Collision,
        DamageCost = 1_000m
    };

    public static async Task AssertValidationErrorAsync(this HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _), $"No error for '{field}'.");
    }
}
