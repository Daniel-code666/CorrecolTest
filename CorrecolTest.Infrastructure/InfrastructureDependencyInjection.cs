using CorrecolTest.Application.Abstractions;
using CorrecolTest.Infrastructure.Export;
using CorrecolTest.Infrastructure.Persistence;
using CorrecolTest.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CorrecolTest.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CorrecolDbContext>(options => ConfigureDatabase(options, connectionString));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IWriteTransaction, WriteTransaction>();
        services.AddSingleton<IClienteExcelExporter, ClienteExcelExporter>();
        return services;
    }

    public static void ConfigureDatabase(DbContextOptionsBuilder options, string connectionString) =>
        options.UseSqlServer(connectionString)
            .UseSeeding((db, _) => CatalogSeed.Seed((CorrecolDbContext)db))
            .UseAsyncSeeding((db, _, ct) => CatalogSeed.SeedAsync((CorrecolDbContext)db, ct));
}
