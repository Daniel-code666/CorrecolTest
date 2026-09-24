using CorrecolTest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CorrecolTest.Health;

public class DatabaseHealthCheck(CorrecolDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                return HealthCheckResult.Unhealthy("Hay migraciones pendientes.");
            // Comprobar que la tabla es consultable; no requiere que existan clientes.
            _ = await db.Clientes.AnyAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("Base de datos no disponible.", ex); }
    }
}
