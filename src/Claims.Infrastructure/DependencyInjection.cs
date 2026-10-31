using System.Threading.Channels;
using Claims.Application.Abstractions;
using Claims.Infrastructure.Auditing;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(provider => new MongoClient(ConnectionString(provider, "MongoDb")));

        services.AddDbContext<ClaimsDbContext>((provider, options) => options.UseMongoDB(
            provider.GetRequiredService<IMongoClient>(),
            provider.GetRequiredService<IConfiguration>()["MongoDb:DatabaseName"] ?? "Claims"));

        services.AddDbContextFactory<AuditContext>((provider, options) => options.UseSqlServer(
            ConnectionString(provider, "SqlServer"),
            sql => sql.EnableRetryOnFailure().MigrationsHistoryTable(HistoryRepository.DefaultTableName, "audit")));

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ICoverRepository, CoverRepository>();

        services.AddSingleton(Channel.CreateBounded<AuditEvent>(
            new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true }));
        services.AddSingleton<IAuditPublisher, ChannelAuditPublisher>();
        services.AddSingleton<IAuditStore, SqlAuditStore>();
        services.AddHostedService<AuditWorker>();
        return services;
    }

    private static string ConnectionString(IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString(name)
        ?? throw new InvalidOperationException($"ConnectionStrings:{name} is not configured.");
}
