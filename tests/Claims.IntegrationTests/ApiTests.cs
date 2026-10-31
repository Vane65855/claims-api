using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Claims.IntegrationTests;

public class ApiTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/Claims")]
    [InlineData("/Covers")]
    public async Task Unknown_ids_return_404_problem_json_on_get_and_delete(string resource)
    {
        var ct = TestContext.Current.CancellationToken;
        var url = $"{resource}/{Guid.NewGuid()}";

        AssertNotFoundProblem(await fixture.Client.GetAsync(url, ct));
        AssertNotFoundProblem(await fixture.Client.DeleteAsync(url, ct));
    }

    [Fact]
    public async Task Swagger_document_is_served()
    {
        var ct = TestContext.Current.CancellationToken;

        var document = await fixture.Client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json", ct);

        Assert.True(document.GetProperty("paths").TryGetProperty("/Claims/{id}", out _));
        Assert.True(document.GetProperty("paths").TryGetProperty("/Covers/compute", out _));
    }

    private static void AssertNotFoundProblem(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
