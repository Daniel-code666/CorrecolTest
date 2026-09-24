namespace CorrecolTest.Domain.Entities;

public class Pais : AuditTable
{
    public bool Active { get; set; } = true;
    public short Codigo { get; set; }
    public string Iso1 { get; set; } = "";
    public string Iso2 { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Capital { get; set; } = "";
}
