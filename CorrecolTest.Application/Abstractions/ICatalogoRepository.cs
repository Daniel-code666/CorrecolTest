using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;

namespace CorrecolTest.Application.Abstractions;

public interface ICatalogoRepository
{
    Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<Pais?> GetPaisAsync(short codigo, CancellationToken ct);
    Task<Departamento?> GetDepartamentoAsync(int codigo, CancellationToken ct);
    Task<Ciudad?> GetCiudadAsync(int codigo, CancellationToken ct);
    Task<bool> HasDepartamentosAsync(short paisCodigo, CancellationToken ct);
    Task<bool> HasCiudadesAsync(int departamentoCodigo, CancellationToken ct);
}

