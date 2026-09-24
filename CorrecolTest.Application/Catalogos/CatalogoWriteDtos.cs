using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CorrecolTest.Application.Catalogos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class PaisUpdateDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";
    [Required, StringLength(5)]
    public string Iso1 { get; set; } = "";
    [Required, StringLength(3)]
    public string Iso2 { get; set; } = "";
    [Required, StringLength(100)]
    public string Capital { get; set; } = "";
}

public class PaisCreateDto : PaisUpdateDto
{
    [Range(1, short.MaxValue)]
    public short Codigo { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class DepartamentoUpdateDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";
}

public class DepartamentoCreateDto : DepartamentoUpdateDto
{
    [Range(1, int.MaxValue)]
    public int Codigo { get; set; }
    [Range(1, short.MaxValue)]
    public short PaisCodigo { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CiudadUpdateDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";
}

public class CiudadCreateDto : CiudadUpdateDto
{
    [Range(1, int.MaxValue)]
    public int Codigo { get; set; }
    [Range(1, int.MaxValue)]
    public int DepartamentoCodigo { get; set; }
}
