namespace CorrecolTest.Application.Catalogos;

public class PaisDto
{
    public short Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public string Iso1 { get; set; } = "";
    public string Iso2 { get; set; } = "";
    public string Capital { get; set; } = "";
}

public class DepartamentoDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public short PaisCodigo { get; set; }
}

public class CiudadDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public int DepartamentoCodigo { get; set; }
    public short PaisCodigo { get; set; }
}
