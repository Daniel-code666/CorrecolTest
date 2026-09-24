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
