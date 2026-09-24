using CorrecolTest.Application.Abstractions;

namespace CorrecolTest.UnitTests;

// Las transacciones SQL se prueban en integración; aquí se aíslan las reglas de negocio.
public class DirectWriteTransaction : IWriteTransaction
{
    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct) => operation();
    public Task ExecuteAsync(Func<Task> operation, CancellationToken ct) => operation();
}
