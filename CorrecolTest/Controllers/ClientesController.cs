using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CorrecolTest.Controllers;

[ApiController]
[Route("api/clientes")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class ClientesController(IClienteService service) : ControllerBase
{
    /// <summary>Lista clientes con filtros y paginación, ordenados por ID.</summary>
    /// <remarks>PageNumber inicia en 1; PageSize admite 1–100. Active=true por defecto, false para inactivos y active= para ambos estados.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClienteDto>>> GetAll([FromQuery] ClientePagedFilter filter, CancellationToken ct) =>
        Ok(await service.GetAllAsync(filter, ct));

    /// <summary>Consulta un cliente por ID, incluyendo clientes inactivos.</summary>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    /// <summary>Crea un cliente con identificación única y ubicación coherente.</summary>
    /// <remarks>Departamento y ciudad pueden omitirse cuando no existen opciones en el catálogo correspondiente.
    /// Tipos: CedulaCiudadania, Nit, CedulaExtranjeria, Pasaporte, TarjetaIdentidad.
    /// Las fechas y Active se asignan automáticamente.</remarks>
    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Actualiza los datos de un cliente activo.</summary>
    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Update(int id, ClienteUpdateDto dto, CancellationToken ct) =>
        Ok(await service.UpdateAsync(id, dto, ct));

    /// <summary>Desactiva un cliente sin eliminar sus datos.</summary>
    /// <remarks>Repetir la operación sobre un cliente inactivo devuelve 204 sin cambiar sus fechas.</remarks>
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Exporta todos los clientes que cumplen los filtros a Excel, sin paginación.</summary>
    /// <remarks>Exporta activos por defecto. El reporte contiene nombres de catálogos y fechas UTC.</remarks>
    [HttpGet("exportacion")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Export([FromQuery] ClienteFilter filter, CancellationToken ct) =>
        File(await service.ExportAsync(filter, ct), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "clientes.xlsx");
}

