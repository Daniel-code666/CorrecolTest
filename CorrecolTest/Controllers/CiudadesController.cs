using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CorrecolTest.Controllers;

[ApiController]
[Route("api/ciudades")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class CiudadesController(ICatalogoService service) : ControllerBase
{
    /// <summary>Crea un registro activo en el catálogo.</summary>
    /// <remarks>El código y el departamento se asignan al crear y no se modifican. La edición cambia el nombre. Active y las fechas de auditoría se asignan automáticamente.</remarks>
    [HttpPost]
    [ProducesResponseType<CiudadDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CiudadDto>> Create(CiudadCreateDto dto, CancellationToken ct)
    {
        var created = await service.CreateCiudadAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { codigo = created.Codigo }, created);
    }

    /// <summary>Actualiza los datos descriptivos de un registro activo.</summary>
    /// <remarks>El código y el departamento se asignan al crear y no se modifican. La edición cambia el nombre. No permite reactivar registros.</remarks>
    [HttpPut("{codigo:int:min(1)}")]
    [ProducesResponseType<CiudadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CiudadDto>> Update(int codigo, CiudadUpdateDto dto, CancellationToken ct) =>
        Ok(await service.UpdateCiudadAsync(codigo, dto, ct));

    /// <summary>Desactiva un registro sin eliminarlo físicamente.</summary>
    /// <remarks>Devuelve 409 si tiene clientes asociados (incluso inactivos) o hijos activos.
    /// Un registro ya inactivo devuelve 204 sin cambiar su auditoría. El seed no lo reactiva.</remarks>
    [HttpDelete("{codigo:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int codigo, CancellationToken ct)
    {
        await service.DeleteCiudadAsync(codigo, ct);
        return NoContent();
    }

    /// <summary>Consulta ciudades con filtros de código, nombre y ubicación; resultado paginado.</summary>
    /// <remarks>PageNumber inicia en 1; PageSize admite 1–100. Orden ascendente por código.
    /// Para cargar un desplegable completo, conservar los filtros y recorrer desde la página 1 hasta
    /// ceil(totalRecords / pageSize), acumulando items. No interpretar una página como el catálogo completo.
    /// Ejemplo: departamentoCodigo=5 y pageSize=100 devuelve los 125 municipios de Antioquia en dos páginas (100 y 25).</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<CiudadDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CiudadDto>>> GetAll([FromQuery] CatalogoFilter filter, CancellationToken ct) =>
        Ok(await service.GetCiudadesAsync(filter, ct));

    /// <summary>Consulta un registro por su código del catálogo.</summary>
    [HttpGet("{codigo:int:min(1)}")]
    [ProducesResponseType<CiudadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CiudadDto>> GetById(int codigo, CancellationToken ct) =>
        Ok(await service.GetCiudadAsync(codigo, ct));
}
