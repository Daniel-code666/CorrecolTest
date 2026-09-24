using System.ComponentModel.DataAnnotations;
using CorrecolTest.Domain.Enums;

namespace CorrecolTest.Application.Clientes;

public abstract class ClienteWriteDto
{
    [EnumDataType(typeof(TipoIdentificacion))]
    public TipoIdentificacion TipoIdentificacion { get; set; }
    [Required, StringLength(30)]
    public string NumeroIdentificacion { get; set; } = "";
    [Required, StringLength(150)]
    public string RazonSocial { get; set; } = "";
    [Range(1, short.MaxValue)]
    public short PaisCodigo { get; set; }
    [Range(1, int.MaxValue)]
    public int? DepartamentoCodigo { get; set; }
    [Range(1, int.MaxValue)]
    public int? CiudadCodigo { get; set; }
}

public class ClienteCreateDto : ClienteWriteDto;
public class ClienteUpdateDto : ClienteWriteDto;

public class ClienteDto
{
    public int Id { get; set; }
    public TipoIdentificacion TipoIdentificacion { get; set; }
    public string NumeroIdentificacion { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public short PaisCodigo { get; set; }
    public string PaisNombre { get; set; } = "";
    public int? DepartamentoCodigo { get; set; }
    public string? DepartamentoNombre { get; set; }
    public int? CiudadCodigo { get; set; }
    public string? CiudadNombre { get; set; }
    public bool Active { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

