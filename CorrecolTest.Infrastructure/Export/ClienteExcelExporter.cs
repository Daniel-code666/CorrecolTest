using ClosedXML.Excel;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Clientes;

namespace CorrecolTest.Infrastructure.Export;

public class ClienteExcelExporter : IClienteExcelExporter
{
    public byte[] Export(IReadOnlyList<ClienteDto> clientes)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Clientes");
        string[] headers = ["ID", "Tipo identificación", "Identificación", "Razón social", "País",
            "Departamento", "Ciudad", "Activo", "Creación UTC", "Actualización UTC"];
        for (var column = 0; column < headers.Length; column++) sheet.Cell(1, column + 1).Value = headers[column];
        var row = 2;
        foreach (var cliente in clientes)
        {
            sheet.Cell(row, 1).Value = cliente.Id;
            sheet.Cell(row, 2).Value = cliente.TipoIdentificacion.ToString();
            // Asignación de texto explícita: conserva ceros y no interpreta fórmulas.
            sheet.Cell(row, 3).Value = cliente.NumeroIdentificacion;
            sheet.Cell(row, 3).Style.NumberFormat.Format = "@";
            sheet.Cell(row, 4).Value = cliente.RazonSocial;
            sheet.Cell(row, 5).Value = cliente.PaisNombre;
            sheet.Cell(row, 6).Value = cliente.DepartamentoNombre ?? "";
            sheet.Cell(row, 7).Value = cliente.CiudadNombre ?? "";
            sheet.Cell(row, 8).Value = cliente.Active ? "Sí" : "No";
            sheet.Cell(row, 9).Value = cliente.CreationDate;
            if (cliente.UpdatedDate.HasValue) sheet.Cell(row, 10).Value = cliente.UpdatedDate.Value;
            row++;
        }
        sheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
        sheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        sheet.Columns(9, 10).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
        sheet.Columns().Width = 24;
        sheet.Column(4).Width = 45;
        sheet.SheetView.FreezeRows(1);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

