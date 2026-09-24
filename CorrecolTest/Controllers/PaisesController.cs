using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CorrecolTest.Controllers;

[ApiController]
[Route("api/paises")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class PaisesController(ICatalogoService service) : ControllerBase
{
    /// <summary>Crea un registro activo en el catálogo.</summary>
    /// <remarks>Los códigos se asignan al crear y no se modifican. El nombre debe ser único, incluyendo países inactivos. Active y las fechas de auditoría se asignan automáticamente.</remarks>
    [HttpPost]
    [ProducesResponseType<PaisDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaisDto>> Create(PaisCreateDto dto, CancellationToken ct)
    {
        var created = await service.CreatePaisAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { codigo = created.Codigo }, created);
    }

    /// <summary>Actualiza los datos descriptivos de un registro activo.</summary>
    /// <remarks>Los códigos se asignan al crear y no se modifican. El nombre debe ser único, incluyendo países inactivos. No permite reactivar registros.</remarks>
    [HttpPut("{codigo:int:min(1)}")]
    [ProducesResponseType<PaisDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaisDto>> Update(short codigo, PaisUpdateDto dto, CancellationToken ct) =>
        Ok(await service.UpdatePaisAsync(codigo, dto, ct));

    /// <summary>Desactiva un registro sin eliminarlo físicamente.</summary>
    /// <remarks>Devuelve 409 si tiene clientes asociados (incluso inactivos) o hijos activos.
    /// Un registro ya inactivo devuelve 204 sin cambiar su auditoría. El seed no lo reactiva.</remarks>
    [HttpDelete("{codigo:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(short codigo, CancellationToken ct)
    {
        await service.DeletePaisAsync(codigo, ct);
        return NoContent();
    }

    /// <summary>Consulta paises con filtros de código, nombre y ubicación; resultado paginado.</summary>
    /// <remarks>PageNumber inicia en 1; PageSize admite 1–100. Orden ascendente por código.
    /// Para cargar un desplegable completo, conservar los filtros y recorrer todas las páginas
    /// hasta ceil(totalRecords / pageSize), acumulando items. Los 246 países requieren tres páginas con pageSize=100.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<PaisDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaisDto>>> GetAll([FromQuery] CatalogoFilter filter, CancellationToken ct) =>
        Ok(await service.GetPaisesAsync(filter, ct));

    /// <summary>Consulta un registro por su código del catálogo.</summary>
    [HttpGet("{codigo:int:min(1)}")]
    [ProducesResponseType<PaisDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaisDto>> GetById(short codigo, CancellationToken ct) =>
        Ok(await service.GetPaisAsync(codigo, ct));
}
