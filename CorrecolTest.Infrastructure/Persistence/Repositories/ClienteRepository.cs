using AutoMapper;
using AutoMapper.QueryableExtensions;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;
using CorrecolTest.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.Infrastructure.Persistence.Repositories;

public class ClienteRepository(CorrecolDbContext db, IMapper mapper) : IClienteRepository
{
    private IQueryable<Cliente> Filter(ClienteFilter filter)
    {
        var query = db.Clientes.AsNoTracking();
        if (filter.Active.HasValue) query = query.Where(x => x.Active == filter.Active);
        if (filter.TipoIdentificacion.HasValue) query = query.Where(x => x.TipoIdentificacion == filter.TipoIdentificacion);
        if (!string.IsNullOrWhiteSpace(filter.NumeroIdentificacion))
            query = query.Where(x => x.NumeroIdentificacion.Contains(filter.NumeroIdentificacion.Trim()));
        if (!string.IsNullOrWhiteSpace(filter.RazonSocial))
            query = query.Where(x => x.RazonSocial.Contains(filter.RazonSocial.Trim()));
        if (filter.PaisCodigo.HasValue) query = query.Where(x => x.PaisCodigo == filter.PaisCodigo);
        if (filter.DepartamentoCodigo.HasValue) query = query.Where(x => x.DepartamentoCodigo == filter.DepartamentoCodigo);
        if (filter.CiudadCodigo.HasValue) query = query.Where(x => x.CiudadCodigo == filter.CiudadCodigo);
        return query;
    }

    public async Task<PagedResult<ClienteDto>> GetAllAsync(ClientePagedFilter filter, CancellationToken ct)
    {
        var query = Filter(filter);
        var count = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Id).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize)
            .ProjectTo<ClienteDto>(mapper.ConfigurationProvider).ToListAsync(ct);

        return new PagedResult<ClienteDto>
        {
            Items = items,
            TotalRecords = count,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<IReadOnlyList<ClienteDto>> GetExportAsync(ClienteFilter filter, CancellationToken ct) =>
        await Filter(filter).OrderBy(x => x.Id).ProjectTo<ClienteDto>(mapper.ConfigurationProvider).ToListAsync(ct);

    public Task<ClienteDto?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Clientes.AsNoTracking().Where(x => x.Id == id).ProjectTo<ClienteDto>(mapper.ConfigurationProvider).SingleOrDefaultAsync(ct);

    public Task<Cliente?> FindAsync(int id, CancellationToken ct) => db.Clientes.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> IdentificationExistsAsync(TipoIdentificacion type, string number, int? excludingId, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.TipoIdentificacion == type && x.NumeroIdentificacion == number
            && (!excludingId.HasValue || x.Id != excludingId), ct);

    public Task AddAsync(Cliente cliente, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        db.Clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("Ya existe un cliente con ese tipo y número de identificación.");
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("El cliente cambió durante la operación. Consulte sus datos e intente de nuevo.");
        }
    }
}
