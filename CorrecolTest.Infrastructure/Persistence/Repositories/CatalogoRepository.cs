using AutoMapper;
using AutoMapper.QueryableExtensions;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace CorrecolTest.Infrastructure.Persistence.Repositories;

public class CatalogoRepository(CorrecolDbContext db, IMapper mapper) : ICatalogoRepository
{
    public Task<PagedResult<PaisDto>> GetPaisesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        var query = db.Paises.AsNoTracking();
        if (filter.Active.HasValue) query = query.Where(x => x.Active == filter.Active);
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
        if (filter.Active.HasValue) query = query.Where(x => x.Active == filter.Active);
        if (filter.Codigo.HasValue) query = query.Where(x => x.Codigo == filter.Codigo);
        if (filter.DepartamentoCodigo.HasValue) query = query.Where(x => x.Codigo == filter.DepartamentoCodigo);
        if (!string.IsNullOrWhiteSpace(filter.Nombre)) query = query.Where(x => x.Nombre.Contains(filter.Nombre.Trim()));
        if (filter.PaisCodigo.HasValue) query = query.Where(x => x.PaisCodigo == filter.PaisCodigo);
        return PageAsync<Departamento, DepartamentoDto>(query.OrderBy(x => x.Codigo), filter, ct);
    }

    public Task<PagedResult<CiudadDto>> GetCiudadesAsync(CatalogoFilter filter, CancellationToken ct)
    {
        var query = db.Ciudades.AsNoTracking();
        if (filter.Active.HasValue) query = query.Where(x => x.Active == filter.Active);
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
        db.Departamentos.AnyAsync(x => x.PaisCodigo == paisCodigo && x.Active, ct);
    public Task<bool> HasCiudadesAsync(int departamentoCodigo, CancellationToken ct) =>
        db.Ciudades.AnyAsync(x => x.DepartamentoCodigo == departamentoCodigo && x.Active, ct);

    public Task<Pais?> FindPaisAsync(short codigo, CancellationToken ct) =>
        db.Paises.SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<Departamento?> FindDepartamentoAsync(int codigo, CancellationToken ct) =>
        db.Departamentos.SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<Ciudad?> FindCiudadAsync(int codigo, CancellationToken ct) =>
        db.Ciudades.SingleOrDefaultAsync(x => x.Codigo == codigo, ct);
    public Task<bool> PaisNombreExistsAsync(string nombre, short? excludingCodigo, CancellationToken ct) =>
        db.Paises.AnyAsync(x => x.Nombre == nombre && (!excludingCodigo.HasValue || x.Codigo != excludingCodigo), ct);
    public Task<bool> DepartamentoNombreExistsAsync(string nombre, int? excludingCodigo, CancellationToken ct) =>
        db.Departamentos.AnyAsync(x => x.Nombre == nombre && (!excludingCodigo.HasValue || x.Codigo != excludingCodigo), ct);
    public Task<bool> PaisHasClientesAsync(short codigo, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.PaisCodigo == codigo, ct);
    public Task<bool> DepartamentoHasClientesAsync(int codigo, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.DepartamentoCodigo == codigo, ct);
    public Task<bool> CiudadHasClientesAsync(int codigo, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.CiudadCodigo == codigo, ct);

    public void AddPais(Pais pais) => db.Paises.Add(pais);
    public void AddDepartamento(Departamento departamento) => db.Departamentos.Add(departamento);
    public void AddCiudad(Ciudad ciudad) => db.Ciudades.Add(ciudad);

    public async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("El estado del catálogo cambió durante la operación. Consulte los datos e intente nuevamente.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("El código o nombre ya está registrado, incluso si el registro está inactivo.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new ConflictException("La operación no es compatible con las relaciones existentes del catálogo.");
        }
    }
}
