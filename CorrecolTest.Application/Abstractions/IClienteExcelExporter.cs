using CorrecolTest.Application.Clientes;

namespace CorrecolTest.Application.Abstractions;

public interface IClienteExcelExporter
{
    byte[] Export(IReadOnlyList<ClienteDto> clientes);
}

