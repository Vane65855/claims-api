using Claims.Auditing;
using Claims.Controllers;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AuditContext>((services, options) =>
    options.UseSqlServer(services.GetRequiredService<IConfiguration>().GetConnectionString("SqlServer")));

builder.Services.AddSingleton<IMongoClient>(services =>
    new MongoClient(services.GetRequiredService<IConfiguration>().GetConnectionString("MongoDb")));

builder.Services.AddDbContext<ClaimsContext>((services, options) =>
    options.UseMongoDB(
        services.GetRequiredService<IMongoClient>(),
        services.GetRequiredService<IConfiguration>()["MongoDb:DatabaseName"]!));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}

app.Run();

public partial class Program { }
