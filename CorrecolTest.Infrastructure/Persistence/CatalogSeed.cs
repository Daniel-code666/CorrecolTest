using System.Text.Json;
using CorrecolTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.Infrastructure.Persistence;

public static class CatalogSeed
{
    // EF ejecuta estos callbacks bajo su bloqueo de migración.
    public static void Seed(CorrecolDbContext db)
    {
        var ownsTransaction = db.Database.CurrentTransaction is null;
        using var transaction = ownsTransaction ? db.Database.BeginTransaction() : null;
        AddMissing(db.Paises, Read<Pais>("paises"), x => x.Codigo);
        AddMissing(db.Departamentos, Read<Departamento>("departamentos"), x => x.Codigo);
        AddMissing(db.Ciudades, Read<Ciudad>("ciudades"), x => x.Codigo);
        db.SaveChanges();
        transaction?.Commit();
    }

    public static async Task SeedAsync(CorrecolDbContext db, CancellationToken ct)
    {
        var ownsTransaction = db.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction ? await db.Database.BeginTransactionAsync(ct) : null;
        await AddMissingAsync(db.Paises, Read<Pais>("paises"), x => x.Codigo, ct);
        await AddMissingAsync(db.Departamentos, Read<Departamento>("departamentos"), x => x.Codigo, ct);
        await AddMissingAsync(db.Ciudades, Read<Ciudad>("ciudades"), x => x.Codigo, ct);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
    }

    private static List<T> Read<T>(string name)
    {
        using var stream = typeof(CatalogSeed).Assembly.GetManifestResourceStream(
            $"CorrecolTest.Infrastructure.Persistence.SeedData.{name}.json")
            ?? throw new InvalidOperationException($"No se encontró el catálogo {name}.");
        return JsonSerializer.Deserialize<List<T>>(stream)
            ?? throw new InvalidOperationException($"El catálogo {name} no es válido.");
    }

    private static void AddMissing<T, TKey>(DbSet<T> set, IEnumerable<T> seed, Func<T, TKey> key)
        where T : class where TKey : notnull
    {
        var existing = set.AsNoTracking().ToList().Select(key).ToHashSet();
        set.AddRange(seed.Where(x => !existing.Contains(key(x))));
    }

    private static async Task AddMissingAsync<T, TKey>(DbSet<T> set, IEnumerable<T> seed, Func<T, TKey> key, CancellationToken ct)
        where T : class where TKey : notnull
    {
        var existing = (await set.AsNoTracking().ToListAsync(ct)).Select(key).ToHashSet();
        set.AddRange(seed.Where(x => !existing.Contains(key(x))));
    }
}

