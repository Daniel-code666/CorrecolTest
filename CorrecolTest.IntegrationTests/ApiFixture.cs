using CorrecolTest.Infrastructure;
using CorrecolTest.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorrecolTest.IntegrationTests;

public class ApiFixture : IAsyncLifetime
{
    private readonly string databaseName = "CorrecolTestTests_" + Guid.NewGuid().ToString("N");
    public string ConnectionString { get; private set; } = "";
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseConnection = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
            ?? throw new InvalidOperationException("Ejecute scripts/test.ps1 o configure TEST_SQLSERVER_CONNECTION para SQL Server.");
        var connection = new SqlConnectionStringBuilder(baseConnection) { InitialCatalog = databaseName };
        ConnectionString = connection.ConnectionString;
        // Dos inicializadores sobre una base vacía: el bloqueo de EF debe serializarlos.
        await using (var first = CreateContext())
        await using (var second = CreateContext())
        {
            // Crear la base primero evita la carrera de CREATE DATABASE; luego competir por el esquema/seed.
            await using var master = new SqlConnection(new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" }.ConnectionString);
            await master.OpenAsync();
            await using var create = master.CreateCommand();
            create.CommandText = $"CREATE DATABASE [{databaseName}]";
            await create.ExecuteNonQueryAsync();
            await Task.WhenAll(first.Database.MigrateAsync(), second.Database.MigrateAsync());
        }
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Database:InitializeOnStartup"] = "false"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<CorrecolDbContext>();
                services.RemoveAll<DbContextOptions<CorrecolDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<CorrecolDbContext>>();
                services.AddDbContext<CorrecolDbContext>(options =>
                {
                    InfrastructureDependencyInjection.ConfigureDatabase(options, ConnectionString);
                    options.ConfigureWarnings(warnings => warnings.Throw(
                        Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.RowLimitingOperationWithoutOrderByWarning));
                });
            });
        });
        Client = Factory.CreateClient();
    }

    public CorrecolDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CorrecolDbContext>();
        InfrastructureDependencyInjection.ConfigureDatabase(options, ConnectionString);
        return new CorrecolDbContext(options.Options);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null) await Factory.DisposeAsync();
        if (!string.IsNullOrEmpty(ConnectionString))
        {
            var catalog = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
            if (catalog != databaseName || !catalog.StartsWith("CorrecolTestTests_", StringComparison.Ordinal))
                throw new InvalidOperationException("La limpieza solo admite la base aislada de esta ejecución.");
            await using var db = CreateContext();
            await db.Database.EnsureDeletedAsync();
        }
    }
}
