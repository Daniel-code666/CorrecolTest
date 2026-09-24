using AutoMapper;
using CorrecolTest.Application.Abstractions;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using CorrecolTest.Application.Mapping;
using CorrecolTest.Domain.Entities;
using CorrecolTest.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CorrecolTest.UnitTests;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> repository = new();
    private readonly Mock<ICatalogoRepository> catalogos = new();
    private readonly Mock<IClienteExcelExporter> exporter = new();
    private readonly IMapper mapper = new MapperConfiguration(c => c.AddProfile<ApplicationProfile>(), NullLoggerFactory.Instance).CreateMapper();
    private ClienteService Service => new(repository.Object, catalogos.Object, exporter.Object, mapper);
    private static ClienteCreateDto Valid() => new()
    {
        TipoIdentificacion = TipoIdentificacion.Nit,
        NumeroIdentificacion = "00123",
        RazonSocial = "Prueba",
        PaisCodigo = 170,
        DepartamentoCodigo = 5,
        CiudadCodigo = 5001
    };

    public ClienteServiceTests()
    {
        catalogos.Setup(x => x.GetPaisAsync(170, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Codigo = 170 });
        catalogos.Setup(x => x.HasDepartamentosAsync(170, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        catalogos.Setup(x => x.GetDepartamentoAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Departamento { Codigo = 5, PaisCodigo = 170 });
        catalogos.Setup(x => x.HasCiudadesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        catalogos.Setup(x => x.GetCiudadAsync(5001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ciudad { Codigo = 5001, DepartamentoCodigo = 5 });
        repository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ClienteDto());
    }

    [Fact]
    public void AutoMapperConfigurationIsValid() => mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Theory]
    [InlineData("", "Cliente")]
    [InlineData("   ", "Cliente")]
    [InlineData("123", " ")]
    public async Task RejectsBlankRequiredText(string number, string name)
    {
        var dto = Valid(); dto.NumeroIdentificacion = number; dto.RazonSocial = name;
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto, default));
        repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(31, 10)]
    [InlineData(10, 151)]
    public async Task RejectsExcessLength(int numberLength, int nameLength)
    {
        var dto = Valid(); dto.NumeroIdentificacion = new('1', numberLength); dto.RazonSocial = new('a', nameLength);
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto, default));
    }

    [Fact]
    public async Task RequiresDepartmentWhenCountryHasOptions()
    {
        var dto = Valid(); dto.DepartamentoCodigo = null; dto.CiudadCodigo = null;
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto, default));
    }

    [Fact]
    public async Task RequiresCityWhenDepartmentHasOptions()
    {
        var dto = Valid(); dto.CiudadCodigo = null;
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto, default));
    }

    [Fact]
    public async Task AllowsMissingLocationWhenNoOptionsExist()
    {
        catalogos.Setup(x => x.GetPaisAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Codigo = 4 });
        var dto = Valid(); dto.PaisCodigo = 4; dto.DepartamentoCodigo = null; dto.CiudadCodigo = null;
        await Service.CreateAsync(dto, default);
        repository.Verify(x => x.AddAsync(It.Is<Cliente>(c => c.PaisCodigo == 4 && c.DepartamentoCodigo == null && c.CiudadCodigo == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AllowsDepartmentWithoutCities()
    {
        catalogos.Setup(x => x.HasCiudadesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var dto = Valid(); dto.CiudadCodigo = null;
        await Service.CreateAsync(dto, default);
        repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectsMismatchedCity()
    {
        catalogos.Setup(x => x.GetCiudadAsync(5001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ciudad { Codigo = 5001, DepartamentoCodigo = 8 });
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(Valid(), default));
    }

    [Fact]
    public async Task RejectsCityWithoutDepartment()
    {
        catalogos.Setup(x => x.GetPaisAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Codigo = 4 });
        var dto = Valid(); dto.PaisCodigo = 4; dto.DepartamentoCodigo = null;
        await Assert.ThrowsAsync<ValidationException>(() => Service.CreateAsync(dto, default));
    }

    [Fact]
    public async Task DuplicateIsRejectedAfterTrimming()
    {
        repository.Setup(x => x.IdentificationExistsAsync(TipoIdentificacion.Nit, "00123", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = Valid(); dto.NumeroIdentificacion = " 00123 ";
        await Assert.ThrowsAsync<ConflictException>(() => Service.CreateAsync(dto, default));
    }

    [Fact]
    public async Task RepeatedDeleteDoesNotSaveAgain()
    {
        repository.Setup(x => x.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1, Active = false });
        await Service.DeleteAsync(1, default);
        repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CannotEditInactiveClient()
    {
        repository.Setup(x => x.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1, Active = false });
        await Assert.ThrowsAsync<ConflictException>(() => Service.UpdateAsync(1, new ClienteUpdateDto(), default));
    }
}

