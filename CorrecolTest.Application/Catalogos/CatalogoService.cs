using AutoMapper;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Common;

namespace CorrecolTest.Application.Catalogos;

public class CatalogoService(ICatalogoRepository repository, IMapper mapper) : ICatalogoService
{
    public Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        filter.Validate();
        return repository.GetPaisesAsync(filter, ct);
    }

    public Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct)
    {
        filter.Validate();
        return repository.GetDepartamentosAsync(filter, ct);
    }
    
    public Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        filter.Validate();
        return repository.GetCiudadesAsync(filter, ct);
    }
    
    public async Task<PaisDto> GetPaisAsync(short codigo, CancellationToken ct) =>
        mapper.Map<PaisDto>(await repository.GetPaisAsync(codigo, ct) ?? throw new NotFoundException("El país no existe."));
    
    public async Task<DepartamentoDto> GetDepartamentoAsync(int codigo, CancellationToken ct) =>
        mapper.Map<DepartamentoDto>(await repository.GetDepartamentoAsync(codigo, ct) ?? throw new NotFoundException("El departamento no existe."));
    
    public async Task<CiudadDto> GetCiudadAsync(int codigo, CancellationToken ct) =>
        mapper.Map<CiudadDto>(await repository.GetCiudadAsync(codigo, ct) ?? throw new NotFoundException("La ciudad no existe."));
}

