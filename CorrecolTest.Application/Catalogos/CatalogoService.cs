using AutoMapper;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;

namespace CorrecolTest.Application.Catalogos;

public class CatalogoService(ICatalogoRepository repository, IMapper mapper, IWriteTransaction transaction) : ICatalogoService
{
    public Task<PaisDto> CreatePaisAsync(PaisCreateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            if (dto.Codigo <= 0) throw new ValidationException("El código del país debe ser positivo.");
            ValidatePais(dto);
            if (await repository.GetPaisAsync(dto.Codigo, ct) is not null)
                throw new ConflictException("El código del país ya está registrado.");
            await ValidatePaisNombreAsync(dto.Nombre, null, ct);
            repository.AddPais(mapper.Map<Pais>(dto));
            await repository.SaveAsync(ct);
            return await GetPaisAsync(dto.Codigo, ct);
        }, ct);

    public Task<PaisDto> UpdatePaisAsync(short codigo, PaisUpdateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var pais = await repository.FindPaisAsync(codigo, ct) ?? throw new NotFoundException("El país no existe.");
            RequireActive(pais.Active);
            ValidatePais(dto);
            await ValidatePaisNombreAsync(dto.Nombre, codigo, ct);
            mapper.Map(dto, pais);
            await repository.SaveAsync(ct);
            return await GetPaisAsync(codigo, ct);
        }, ct);

    public Task DeletePaisAsync(short codigo, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var pais = await repository.FindPaisAsync(codigo, ct) ?? throw new NotFoundException("El país no existe.");
            if (!pais.Active) return;
            if (await repository.HasDepartamentosAsync(codigo, ct) || await repository.PaisHasClientesAsync(codigo, ct))
                throw new ConflictException("No se puede desactivar un país con departamentos activos o clientes asociados.");
            pais.Active = false;
            await repository.SaveAsync(ct);
        }, ct);

    public Task<DepartamentoDto> CreateDepartamentoAsync(DepartamentoCreateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            if (dto.Codigo <= 0) throw new ValidationException("El código del departamento debe ser positivo.");
            dto.Nombre = RequiredText(dto.Nombre, 100, "nombre");
            if (await repository.GetDepartamentoAsync(dto.Codigo, ct) is not null)
                throw new ConflictException("El código del departamento ya está registrado.");
            var pais = await repository.GetPaisAsync(dto.PaisCodigo, ct);
            if (pais is null || !pais.Active) throw new ValidationException("El país no existe o está inactivo.");
            await ValidateDepartamentoNombreAsync(dto.Nombre, null, ct);
            repository.AddDepartamento(mapper.Map<Departamento>(dto));
            await repository.SaveAsync(ct);
            return await GetDepartamentoAsync(dto.Codigo, ct);
        }, ct);

    public Task<DepartamentoDto> UpdateDepartamentoAsync(int codigo, DepartamentoUpdateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var departamento = await repository.FindDepartamentoAsync(codigo, ct) ?? throw new NotFoundException("El departamento no existe.");
            RequireActive(departamento.Active);
            dto.Nombre = RequiredText(dto.Nombre, 100, "nombre");
            await ValidateDepartamentoNombreAsync(dto.Nombre, codigo, ct);
            mapper.Map(dto, departamento);
            await repository.SaveAsync(ct);
            return await GetDepartamentoAsync(codigo, ct);
        }, ct);

    public Task DeleteDepartamentoAsync(int codigo, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var departamento = await repository.FindDepartamentoAsync(codigo, ct) ?? throw new NotFoundException("El departamento no existe.");
            if (!departamento.Active) return;
            if (await repository.HasCiudadesAsync(codigo, ct) || await repository.DepartamentoHasClientesAsync(codigo, ct))
                throw new ConflictException("No se puede desactivar un departamento con ciudades activas o clientes asociados.");
            departamento.Active = false;
            await repository.SaveAsync(ct);
        }, ct);

    public Task<CiudadDto> CreateCiudadAsync(CiudadCreateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            if (dto.Codigo <= 0) throw new ValidationException("El código de la ciudad debe ser positivo.");
            dto.Nombre = RequiredText(dto.Nombre, 100, "nombre");
            if (await repository.GetCiudadAsync(dto.Codigo, ct) is not null)
                throw new ConflictException("El código de la ciudad ya está registrado.");
            var departamento = await repository.GetDepartamentoAsync(dto.DepartamentoCodigo, ct);
            if (departamento is null || !departamento.Active)
                throw new ValidationException("El departamento no existe o está inactivo.");
            var pais = await repository.GetPaisAsync(departamento.PaisCodigo, ct);
            if (pais is null || !pais.Active) throw new ValidationException("El país del departamento está inactivo.");
            repository.AddCiudad(mapper.Map<Ciudad>(dto));
            await repository.SaveAsync(ct);
            return await GetCiudadAsync(dto.Codigo, ct);
        }, ct);

    public Task<CiudadDto> UpdateCiudadAsync(int codigo, CiudadUpdateDto dto, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var ciudad = await repository.FindCiudadAsync(codigo, ct) ?? throw new NotFoundException("La ciudad no existe.");
            RequireActive(ciudad.Active);
            dto.Nombre = RequiredText(dto.Nombre, 100, "nombre");
            mapper.Map(dto, ciudad);
            await repository.SaveAsync(ct);
            return await GetCiudadAsync(codigo, ct);
        }, ct);

    public Task DeleteCiudadAsync(int codigo, CancellationToken ct) =>
        transaction.ExecuteAsync(async () =>
        {
            var ciudad = await repository.FindCiudadAsync(codigo, ct) ?? throw new NotFoundException("La ciudad no existe.");
            if (!ciudad.Active) return;
            if (await repository.CiudadHasClientesAsync(codigo, ct))
                throw new ConflictException("No se puede desactivar una ciudad con clientes asociados.");
            ciudad.Active = false;
            await repository.SaveAsync(ct);
        }, ct);

    private static string RequiredText(string? value, int maxLength, string field)
    {
        var normalized = value?.Trim() ?? "";
        if (normalized.Length == 0 || normalized.Length > maxLength)
            throw new ValidationException($"El campo {field} es obligatorio y admite hasta {maxLength} caracteres.");
        return normalized;
    }

    private static void ValidatePais(PaisUpdateDto dto)
    {
        dto.Nombre = RequiredText(dto.Nombre, 100, "nombre");
        dto.Capital = RequiredText(dto.Capital, 100, "capital");
        dto.Iso1 = RequiredText(dto.Iso1, 5, "ISO 1").ToUpperInvariant();
        dto.Iso2 = RequiredText(dto.Iso2, 3, "ISO 2").ToUpperInvariant();
    }

    private static void RequireActive(bool active)
    {
        if (!active) throw new ConflictException("No se puede editar un registro inactivo.");
    }

    private async Task ValidatePaisNombreAsync(string nombre, short? codigo, CancellationToken ct)
    {
        if (await repository.PaisNombreExistsAsync(nombre, codigo, ct))
            throw new ConflictException("El nombre del país ya está registrado.");
    }

    private async Task ValidateDepartamentoNombreAsync(string nombre, int? codigo, CancellationToken ct)
    {
        if (await repository.DepartamentoNombreExistsAsync(nombre, codigo, ct))
            throw new ConflictException("El nombre del departamento ya está registrado.");
    }

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
