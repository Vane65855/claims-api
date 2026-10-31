using Claims.IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Xunit;

[assembly: AssemblyFixture(typeof(ApiFixture))]

namespace Claims.IntegrationTests;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04")
        .WithDatabase("ClaimsAudit")
        .Build();

    private readonly MongoDbContainer _mongoDb = new MongoDbBuilder("mongo:9.0.4").Build();

    private ApiFactory? _factory;

    public HttpClient Client { get; private set; } = null!;

    public IServiceProvider Services => _factory!.Services;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_sqlServer.StartAsync(), _mongoDb.StartAsync());

        _factory = new ApiFactory(_sqlServer.GetConnectionString(), _mongoDb.GetConnectionString());
        _factory.StartServer();
        Client = _factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _mongoDb.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }

    private sealed class ApiFactory(string sqlServer, string mongoDb) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:SqlServer", sqlServer);
            builder.UseSetting("ConnectionStrings:MongoDb", mongoDb);
            builder.UseSetting("MongoDb:DatabaseName", $"claims_{Guid.NewGuid():N}");
        }
    }
}
