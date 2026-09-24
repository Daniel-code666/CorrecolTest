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
