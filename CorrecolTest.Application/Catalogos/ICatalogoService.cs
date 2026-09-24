using CorrecolTest.Application.Common;

namespace CorrecolTest.Application.Catalogos;

public interface ICatalogoService
{
    Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PaisDto> GetPaisAsync(short codigo, CancellationToken ct);
    Task<DepartamentoDto> GetDepartamentoAsync(int codigo, CancellationToken ct);
    Task<CiudadDto> GetCiudadAsync(int codigo, CancellationToken ct);
}

