using System.ComponentModel.DataAnnotations;
using CorrecolTest.Application.Common;

namespace CorrecolTest.Application.Catalogos;

public class CatalogoFilter : Pagination
{
    [Range(1, int.MaxValue)]
    public int? Codigo { get; set; }
    [StringLength(100)]
    public string? Nombre { get; set; }
    [Range(1, short.MaxValue)]
    public short? PaisCodigo { get; set; }
    [Range(1, int.MaxValue)]
    public int? DepartamentoCodigo { get; set; }
}

