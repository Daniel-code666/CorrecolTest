namespace CorrecolTest.Application.Abstractions;

public interface IWriteTransaction
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct);
    Task ExecuteAsync(Func<Task> operation, CancellationToken ct);
}

