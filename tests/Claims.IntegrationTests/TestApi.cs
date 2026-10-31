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
}
