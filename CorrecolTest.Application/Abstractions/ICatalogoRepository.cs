using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;

namespace CorrecolTest.Application.Abstractions;

public interface ICatalogoRepository
{
    Task<Pais?> FindPaisAsync(short codigo, CancellationToken ct);
    Task<Departamento?> FindDepartamentoAsync(int codigo, CancellationToken ct);
    Task<Ciudad?> FindCiudadAsync(int codigo, CancellationToken ct);
    Task<bool> PaisNombreExistsAsync(string nombre, short? excludingCodigo, CancellationToken ct);
    Task<bool> DepartamentoNombreExistsAsync(string nombre, int? excludingCodigo, CancellationToken ct);
    Task<bool> PaisHasClientesAsync(short codigo, CancellationToken ct);
    Task<bool> DepartamentoHasClientesAsync(int codigo, CancellationToken ct);
    Task<bool> CiudadHasClientesAsync(int codigo, CancellationToken ct);
    void AddPais(Pais pais);
    void AddDepartamento(Departamento departamento);
    void AddCiudad(Ciudad ciudad);
    Task SaveAsync(CancellationToken ct);
    Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct);
    Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct);
    Task<Pais?> GetPaisAsync(short codigo, CancellationToken ct);
    Task<Departamento?> GetDepartamentoAsync(int codigo, CancellationToken ct);
    Task<Ciudad?> GetCiudadAsync(int codigo, CancellationToken ct);
    Task<bool> HasDepartamentosAsync(short paisCodigo, CancellationToken ct);
    Task<bool> HasCiudadesAsync(int departamentoCodigo, CancellationToken ct);
}
