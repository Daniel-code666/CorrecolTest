using CorrecolTest.Application.Common;

namespace CorrecolTest.Application.Clientes;

public interface IClienteService
{
    Task<PagedResult<ClienteDto>> GetAllAsync(ClientePagedFilter filter, CancellationToken ct);
    Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct);
    Task<ClienteDto> CreateAsync(ClienteCreateDto dto, CancellationToken ct);
    Task<ClienteDto> UpdateAsync(int id, ClienteUpdateDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<byte[]> ExportAsync(ClienteFilter filter, CancellationToken ct);
}

