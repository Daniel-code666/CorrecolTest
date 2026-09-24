namespace CorrecolTest.Domain.Entities;

public class Ciudad : AuditTable
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public int DepartamentoCodigo { get; set; }
    public Departamento Departamento { get; set; } = null!;
}

