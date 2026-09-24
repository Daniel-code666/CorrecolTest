using AutoMapper;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;

namespace CorrecolTest.Application.Clientes;

public class ClienteService(IClienteRepository repository, ICatalogoRepository catalogos, IClienteExcelExporter exporter, IMapper mapper, IWriteTransaction transaction) : IClienteService
{
    public Task<PagedResult<ClienteDto>> GetAllAsync(ClientePagedFilter filter, CancellationToken ct)
    {
        new Pagination { PageNumber = filter.PageNumber, PageSize = filter.PageSize }.Validate();
        return repository.GetAllAsync(filter, ct);
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct) ?? throw new NotFoundException("El cliente no existe.");

    public Task<ClienteDto> CreateAsync(ClienteCreateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
    {
        await ValidateAsync(dto, null, ct);
        var cliente = mapper.Map<Cliente>(dto);
        await repository.AddAsync(cliente, ct);
        await repository.SaveAsync(ct);
        return await GetByIdAsync(cliente.Id, ct);
    }, ct);

    public Task<ClienteDto> UpdateAsync(int id, ClienteUpdateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
    {
        var cliente = await repository.FindAsync(id, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (!cliente.Active) throw new ConflictException("No se puede editar un cliente inactivo.");
        await ValidateAsync(dto, id, ct);
        mapper.Map(dto, cliente);
        await repository.SaveAsync(ct);
        return await GetByIdAsync(id, ct);
    }, ct);

    public Task DeleteAsync(int id, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
    {
        var cliente = await repository.FindAsync(id, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (!cliente.Active) return;
        cliente.Active = false;
        await repository.SaveAsync(ct);
    }, ct);

    public async Task<byte[]> ExportAsync(ClienteFilter filter, CancellationToken ct) =>
        exporter.Export(await repository.GetExportAsync(filter, ct));

    private async Task ValidateAsync(ClienteWriteDto dto, int? id, CancellationToken ct)
    {
        dto.NumeroIdentificacion = dto.NumeroIdentificacion?.Trim() ?? "";
        dto.RazonSocial = dto.RazonSocial?.Trim() ?? "";
        if (!Enum.IsDefined(dto.TipoIdentificacion))
            throw new ValidationException("El tipo de identificación no es válido.");
        if (string.IsNullOrWhiteSpace(dto.NumeroIdentificacion) || dto.NumeroIdentificacion.Length > 30)
            throw new ValidationException("La identificación es obligatoria y admite hasta 30 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.RazonSocial) || dto.RazonSocial.Length > 150)
            throw new ValidationException("La razón social es obligatoria y admite hasta 150 caracteres.");
        var pais = await catalogos.GetPaisAsync(dto.PaisCodigo, ct);
        if (pais is null || !pais.Active)
            throw new ValidationException("El país no existe o está inactivo.");

        if (dto.DepartamentoCodigo is { } departamentoCodigo)
        {
            var departamento = await catalogos.GetDepartamentoAsync(departamentoCodigo, ct);
            if (departamento is null || !departamento.Active || departamento.PaisCodigo != dto.PaisCodigo)
                throw new ValidationException("El departamento está inactivo o no pertenece al país seleccionado.");
        }
        else if (await catalogos.HasDepartamentosAsync(dto.PaisCodigo, ct))
            throw new ValidationException("Debe seleccionar un departamento para este país.");

        if (dto.CiudadCodigo is { } ciudadCodigo)
        {
            var ciudad = await catalogos.GetCiudadAsync(ciudadCodigo, ct);
            if (dto.DepartamentoCodigo is null || ciudad is null || !ciudad.Active || ciudad.DepartamentoCodigo != dto.DepartamentoCodigo)
                throw new ValidationException("La ciudad está inactiva o no pertenece al departamento seleccionado.");
        }
        else if (dto.DepartamentoCodigo is { } codigo && await catalogos.HasCiudadesAsync(codigo, ct))
            throw new ValidationException("Debe seleccionar una ciudad para este departamento.");

        if (await repository.IdentificationExistsAsync(dto.TipoIdentificacion, dto.NumeroIdentificacion, id, ct))
            throw new ConflictException("Ya existe un cliente con ese tipo y número de identificación.");
    }
}
