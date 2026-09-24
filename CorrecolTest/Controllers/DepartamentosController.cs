using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CorrecolTest.Controllers;

[ApiController]
[Route("api/departamentos")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class DepartamentosController(ICatalogoService service) : ControllerBase
{
    /// <summary>Consulta departamentos con filtros de código, nombre y ubicación; resultado paginado.</summary>
    /// <remarks>PageNumber inicia en 1; PageSize admite 1–100. Orden ascendente por código.
    /// Para cargar un desplegable completo, conservar el filtro de país y recorrer todas las páginas
    /// hasta ceil(totalRecords / pageSize), acumulando items.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<DepartamentoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DepartamentoDto>>> GetAll([FromQuery] CatalogoFilter filter, CancellationToken ct) =>
        Ok(await service.GetDepartamentosAsync(filter, ct));

    /// <summary>Consulta un registro por su código del catálogo.</summary>
    [HttpGet("{codigo:int:min(1)}")]
    [ProducesResponseType<DepartamentoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartamentoDto>> GetById(int codigo, CancellationToken ct) =>
        Ok(await service.GetDepartamentoAsync(codigo, ct));
}
