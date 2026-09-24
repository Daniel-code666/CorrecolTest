using System.Data;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.Infrastructure.Persistence;

public class WriteTransaction(CorrecolDbContext db) : IWriteTransaction
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct)
    {
        // Mantiene válidas las comprobaciones de relaciones hasta guardar:
        // no permite desactivar un catálogo mientras se le asigna un cliente o un hijo.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var result = await operation();
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception ex) when (IsDeadlock(ex))
        {
            throw new ConflictException("Otra operación modificó los datos relacionados. Consulte los datos e intente nuevamente.");
        }
    }

    private static bool IsDeadlock(Exception exception)
    {
        // El proveedor puede envolver SqlException dentro de DbUpdateException e InvalidOperationException.
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException { Number: 1205 }) return true;
        }
        return false;
    }

    public Task ExecuteAsync(Func<Task> operation, CancellationToken ct) =>
        ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, ct);
}
