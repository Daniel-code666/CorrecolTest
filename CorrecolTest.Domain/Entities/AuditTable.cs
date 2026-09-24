namespace CorrecolTest.Domain.Entities;

public abstract class AuditTable
{
    public DateTime CreationDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

