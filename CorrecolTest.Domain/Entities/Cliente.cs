using CorrecolTest.Domain.Enums;

namespace CorrecolTest.Domain.Entities;

public class Cliente : AuditTable
{
    public int Id { get; set; }
    public TipoIdentificacion TipoIdentificacion { get; set; }
    public string NumeroIdentificacion { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public short PaisCodigo { get; set; }
    public int? DepartamentoCodigo { get; set; }
    public int? CiudadCodigo { get; set; }
    public bool Active { get; set; } = true;
    public Pais Pais { get; set; } = null!;
    public Departamento? Departamento { get; set; }
    public Ciudad? Ciudad { get; set; }
}

