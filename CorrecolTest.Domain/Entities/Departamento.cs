namespace CorrecolTest.Domain.Entities;

public class Departamento : AuditTable
{
    public bool Active { get; set; } = true;
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public short PaisCodigo { get; set; }
    public Pais Pais { get; set; } = null!;
}
