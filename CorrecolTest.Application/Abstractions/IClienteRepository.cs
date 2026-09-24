using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;
using CorrecolTest.Domain.Enums;

namespace CorrecolTest.Application.Abstractions;

public interface IClienteRepository
{
    Task<PagedResult<ClienteDto>> GetAllAsync(ClientePagedFilter filter, CancellationToken ct);
    Task<IReadOnlyList<ClienteDto>> GetExportAsync(ClienteFilter filter, CancellationToken ct);
    Task<ClienteDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<Cliente?> FindAsync(int id, CancellationToken ct);
    Task<bool> IdentificationExistsAsync(TipoIdentificacion type, string number, int? excludingId, CancellationToken ct);
    Task AddAsync(Cliente cliente, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}

