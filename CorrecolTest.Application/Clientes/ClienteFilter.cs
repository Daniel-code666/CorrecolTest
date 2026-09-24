using System.ComponentModel.DataAnnotations;
using CorrecolTest.Domain.Enums;

namespace CorrecolTest.Application.Clientes;

public class ClienteFilter
{
    [EnumDataType(typeof(TipoIdentificacion))]
    public TipoIdentificacion? TipoIdentificacion { get; set; }
    [StringLength(30)]
    public string? NumeroIdentificacion { get; set; }
    [StringLength(150)]
    public string? RazonSocial { get; set; }
    [Range(1, short.MaxValue)]
    public short? PaisCodigo { get; set; }
    [Range(1, int.MaxValue)]
    public int? DepartamentoCodigo { get; set; }
    [Range(1, int.MaxValue)]
    public int? CiudadCodigo { get; set; }
    /// <summary>Activos por defecto; omitir el valor con active= permite incluir ambos estados.</summary>
    public bool? Active { get; set; } = true;
}

public class ClientePagedFilter : ClienteFilter
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

