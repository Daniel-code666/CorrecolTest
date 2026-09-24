using AutoMapper;
using AutoMapper.QueryableExtensions;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.Infrastructure.Persistence.Repositories;

public class CatalogoRepository(CorrecolDbContext db, IMapper mapper) : ICatalogoRepository
{
    public Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        var query = db.Paises.AsNoTracking();
        if (filter.Codigo.HasValue) query = query.Where(x => x.Codigo == filter.Codigo);
        if (!string.IsNullOrWhiteSpace(filter.Nombre)) query = query.Where(x => x.Nombre.Contains(filter.Nombre.Trim()));
        if (filter.PaisCodigo.HasValue) query = query.Where(x => x.Codigo == filter.PaisCodigo);
        if (filter.DepartamentoCodigo.HasValue)
            query = query.Where(x => db.Departamentos.Any(d => d.Codigo == filter.DepartamentoCodigo && d.PaisCodigo == x.Codigo));
        return PageAsync<Pais, PaisDto>(query.OrderBy(x => x.Codigo), filter, ct);
    }

    public Task<PagedResult<DepartamentoDto>> GetDepartamentosAsync(CatalogoFilter filter, CancellationToken ct)
    {
        var query = db.Departamentos.AsNoTracking();
        if (filter.Codigo.HasValue) query = query.Where(x => x.Codigo == filter.Codigo);
        if (filter.DepartamentoCodigo.HasValue) query = query.Where(x => x.Codigo == filter.DepartamentoCodigo);
        if (!string.IsNullOrWhiteSpace(filter.Nombre)) query = query.Where(x => x.Nombre.Contains(filter.Nombre.Trim()));
        if (filter.PaisCodigo.HasValue) query = query.Where(x => x.PaisCodigo == filter.PaisCodigo);
        return PageAsync<Departamento, DepartamentoDto>(query.OrderBy(x => x.Codigo), filter, ct);
    }

    public Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        var query = db.Ciudades.AsNoTracking();
        if (filter.Codigo.HasValue) query = query.Where(x => x.Codigo == filter.Codigo);
        if (!string.IsNullOrWhiteSpace(filter.Nombre)) query = query.Where(x => x.Nombre.Contains(filter.Nombre.Trim()));
        if (filter.PaisCodigo.HasValue) query = query.Where(x => x.Departamento.PaisCodigo == filter.PaisCodigo);
        if (filter.DepartamentoCodigo.HasValue) query = query.Where(x => x.DepartamentoCodigo == filter.DepartamentoCodigo);
        return PageAsync<Ciudad, CiudadDto>(query.OrderBy(x => x.Codigo), filter, ct);
    }

    private async Task<PagedResult<TDto>> PageAsync<TEntity, TDto>(IQueryable<TEntity> query, CatalogoFilter filter, CancellationToken ct)
    {
        var count = await query.CountAsync(ct);
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ProjectTo<TDto>(mapper.ConfigurationProvider).ToListAsync(ct);
        return new PagedResult<TDto>
        {
            Items = items,
            TotalRecords = count,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public Task<Pais?> GetPaisAsync(short codigo, CancellationToken ct) =>
        db.Paises.AsNoTracking().SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<Departamento?> GetDepartamentoAsync(int codigo, CancellationToken ct) =>
        db.Departamentos.AsNoTracking().SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<Ciudad?> GetCiudadAsync(int codigo, CancellationToken ct) =>
        db.Ciudades.AsNoTracking().Include(x => x.Departamento).SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<bool> HasDepartamentosAsync(short paisCodigo, CancellationToken ct) =>
        db.Departamentos.AnyAsync(x => x.PaisCodigo == paisCodigo, ct);
    public Task<bool> HasCiudadesAsync(int departamentoCodigo, CancellationToken ct) =>
        db.Ciudades.AnyAsync(x => x.DepartamentoCodigo == departamentoCodigo, ct);
}
