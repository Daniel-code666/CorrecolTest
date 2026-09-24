using CorrecolTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.Infrastructure.Persistence;

public class CorrecolDbContext(DbContextOptions<CorrecolDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Ciudad> Ciudades => Set<Ciudad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CorrecolDbContext).Assembly);

    private void ApplyAudit()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditTable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreationDate = now;
                entry.Entity.UpdatedDate = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreationDate).IsModified = false;
                entry.Entity.UpdatedDate = now;
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

