using CorrecolTest.Application.Common;

namespace CorrecolTest.Application.Catalogos;

public interface ICatalogoService
{
    Task<PaisDto> CreatePaisAsync(PaisCreateDto dto, CancellationToken ct);
    Task<PaisDto> UpdatePaisAsync(short codigo, PaisUpdateDto dto, CancellationToken ct);
    Task DeletePaisAsync(short codigo, CancellationToken ct);
    Task<DepartamentoDto> CreateDepartamentoAsync(DepartamentoCreateDto dto, CancellationToken ct);
    Task<DepartamentoDto> UpdateDepartamentoAsync(int codigo, DepartamentoUpdateDto dto, CancellationToken ct);
    Task DeleteDepartamentoAsync(int codigo, CancellationToken ct);
    Task<CiudadDto> CreateCiudadAsync(CiudadCreateDto dto, CancellationToken ct);
    Task<CiudadDto> UpdateCiudadAsync(int codigo, CiudadUpdateDto dto, CancellationToken ct);
    Task DeleteCiudadAsync(int codigo, CancellationToken ct);
    Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PaisDto> GetPaisAsync(short codigo, CancellationToken ct);
    Task<DepartamentoDto> GetDepartamentoAsync(int codigo, CancellationToken ct);
    Task<CiudadDto> GetCiudadAsync(int codigo, CancellationToken ct);
}
