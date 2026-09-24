using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.IntegrationTests;

public class CatalogoCrudTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private HttpClient Client => fixture.Client;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static PaisCreateDto Country(short code) => new()
    {
        Codigo = code,
        Nombre = "País prueba " + code,
        Capital = "Capital",
        Iso1 = "TST",
        Iso2 = "TS"
    };

    private async Task<T> Create<T>(string route, object dto)
    {
        var response = await Client.PostAsJsonAsync(route, dto, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private async Task CreateHierarchy(short country, int department, int city)
    {
        await Create<PaisDto>("/api/paises", Country(country));
        await Create<DepartamentoDto>("/api/departamentos", new DepartamentoCreateDto
        {
            Codigo = department,
            Nombre = "Departamento prueba " + department,
            PaisCodigo = country
        });
        await Create<CiudadDto>("/api/ciudades", new CiudadCreateDto
        {
            Codigo = city,
            Nombre = "Ciudad prueba " + city,
            DepartamentoCodigo = department
        });
    }

    [Fact]
    public async Task CompleteCrudMaintainsAuditFiltersAndDependencies()
    {
        const short country = 30001;
        const int department = 200000001;
        const int city = 200000001;
        await CreateHierarchy(country, department, city);
        var originalCountry = (await Client.GetFromJsonAsync<PaisDto>($"/api/paises/{country}", Json))!;
        var originalDepartment = (await Client.GetFromJsonAsync<DepartamentoDto>($"/api/departamentos/{department}", Json))!;
        var originalCity = (await Client.GetFromJsonAsync<CiudadDto>($"/api/ciudades/{city}", Json))!;
        Assert.True(originalCountry.Active);
        Assert.Null(originalCountry.UpdatedDate);
        Assert.Null(originalDepartment.UpdatedDate);
        Assert.Null(originalCity.UpdatedDate);

        var countryResponse = await Client.PutAsJsonAsync($"/api/paises/{country}",
            new PaisUpdateDto { Nombre = "País editado", Capital = "Capital editada", Iso1 = "abc", Iso2 = "xy" }, Json);
        Assert.Equal(HttpStatusCode.OK, countryResponse.StatusCode);
        var updatedCountry = (await countryResponse.Content.ReadFromJsonAsync<PaisDto>(Json))!;
        Assert.Equal("ABC", updatedCountry.Iso1);
        Assert.Equal(originalCountry.CreationDate, updatedCountry.CreationDate);
        Assert.NotNull(updatedCountry.UpdatedDate);

        var departmentResponse = await Client.PutAsJsonAsync($"/api/departamentos/{department}",
            new DepartamentoUpdateDto { Nombre = "Departamento editado" }, Json);
        Assert.Equal(HttpStatusCode.OK, departmentResponse.StatusCode);
        var updatedDepartment = (await departmentResponse.Content.ReadFromJsonAsync<DepartamentoDto>(Json))!;
        Assert.Equal(country, updatedDepartment.PaisCodigo);
        Assert.Equal(originalDepartment.CreationDate, updatedDepartment.CreationDate);
        Assert.NotNull(updatedDepartment.UpdatedDate);

        var cityResponse = await Client.PutAsJsonAsync($"/api/ciudades/{city}",
            new CiudadUpdateDto { Nombre = "Ciudad editada" }, Json);
        Assert.Equal(HttpStatusCode.OK, cityResponse.StatusCode);
        var updatedCity = (await cityResponse.Content.ReadFromJsonAsync<CiudadDto>(Json))!;
        Assert.Equal("Ciudad editada", updatedCity.Nombre);
        Assert.Equal(department, updatedCity.DepartamentoCodigo);
        Assert.Equal(country, updatedCity.PaisCodigo);
        Assert.Equal(originalCity.CreationDate, updatedCity.CreationDate);
        Assert.NotNull(updatedCity.UpdatedDate);

        // Las referencias que forman parte de claves no se aceptan en los DTOs de edición.
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync($"/api/departamentos/{department}",
            new { nombre = "Cambio inválido", paisCodigo = 170 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync($"/api/ciudades/{city}",
            new { nombre = "Cambio inválido", departamentoCodigo = 5 })).StatusCode);

        Assert.Equal(HttpStatusCode.Conflict, (await Client.DeleteAsync($"/api/paises/{country}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.DeleteAsync($"/api/departamentos/{department}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/ciudades/{city}")).StatusCode);
        var deletedCity = (await Client.GetFromJsonAsync<CiudadDto>($"/api/ciudades/{city}", Json))!;
        Assert.False(deletedCity.Active);
        Assert.NotNull(deletedCity.UpdatedDate);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/ciudades/{city}")).StatusCode);
        var repeated = (await Client.GetFromJsonAsync<CiudadDto>($"/api/ciudades/{city}", Json))!;
        Assert.Equal(deletedCity.UpdatedDate, repeated.UpdatedDate);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PutAsJsonAsync($"/api/ciudades/{city}",
            new CiudadUpdateDto { Nombre = "No permitido" }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/departamentos/{department}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/paises/{country}")).StatusCode);

        foreach (var route in new[] { $"paises?codigo={country}", $"departamentos?codigo={department}", $"ciudades?codigo={city}" })
        {
            var active = await Client.GetFromJsonAsync<JsonElement>("/api/" + route);
            Assert.Equal(0, active.GetProperty("totalRecords").GetInt32());
            var inactive = await Client.GetFromJsonAsync<JsonElement>("/api/" + route + "&active=false");
            Assert.Equal(1, inactive.GetProperty("totalRecords").GetInt32());
            var all = await Client.GetFromJsonAsync<JsonElement>("/api/" + route + "&active=");
            Assert.Equal(1, all.GetProperty("totalRecords").GetInt32());
        }
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/paises", Country(country), Json)).StatusCode);
    }

    [Fact]
    public async Task SeedNeverReactivatesDeletedOriginalCatalogRows()
    {
        const int city = 5002;
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/ciudades/{city}")).StatusCode);
        var before = (await Client.GetFromJsonAsync<CiudadDto>($"/api/ciudades/{city}", Json))!;
        await using var db = fixture.CreateContext();
        await db.Database.MigrateAsync();
        db.Database.Migrate();
        var after = await db.Ciudades.AsNoTracking().SingleAsync(x => x.Codigo == city);
        Assert.False(after.Active);
        Assert.Equal(before.CreationDate, after.CreationDate);
        Assert.Equal(before.UpdatedDate, after.UpdatedDate);
        Assert.Equal(1, await db.Ciudades.CountAsync(x => x.Codigo == city));
    }

    [Fact]
    public async Task ValidationDuplicatesAndMissingResourcesReturnControlledErrors()
    {
        await Create<PaisDto>("/api/paises", Country(30002));
        var duplicateName = Country(30003); duplicateName.Nombre = Country(30002).Nombre;
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/paises", duplicateName, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/paises", Country(30002), Json)).StatusCode);
        var empty = Country(30003); empty.Nombre = " ";
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/paises", empty, Json)).StatusCode);
        var longIso = Country(30003); longIso.Iso1 = "123456";
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/paises", longIso, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/paises", Country(0), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/departamentos",
            new DepartamentoCreateDto { Codigo = 200000002, Nombre = "Departamento", PaisCodigo = 32767 }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/ciudades",
            new CiudadCreateDto { Codigo = 200000002, Nombre = "Ciudad", DepartamentoCodigo = int.MaxValue }, Json)).StatusCode);
        await Create<DepartamentoDto>("/api/departamentos",
            new DepartamentoCreateDto { Codigo = 200000002, Nombre = "Departamento validación", PaisCodigo = 30002 });
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/departamentos",
            new DepartamentoCreateDto { Codigo = 200000003, Nombre = "Departamento validación", PaisCodigo = 30002 }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PutAsJsonAsync("/api/departamentos/200000002",
            new DepartamentoUpdateDto { Nombre = "ANTIOQUIA" }, Json)).StatusCode);
        await Create<CiudadDto>("/api/ciudades", new CiudadCreateDto
        {
            Codigo = 200000002,
            Nombre = "Ciudad validación",
            DepartamentoCodigo = 200000002
        });
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/ciudades",
            new CiudadCreateDto { Codigo = 200000002, Nombre = "Otra", DepartamentoCodigo = 200000002 }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutAsJsonAsync("/api/ciudades/200000002",
            new CiudadUpdateDto { Nombre = new string('x', 101) }, Json)).StatusCode);
        foreach (var route in new[] { "paises/32767", "departamentos/2147483647", "ciudades/2147483647" })
        {
            var response = await Client.DeleteAsync("/api/" + route);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        }
        Assert.Equal(HttpStatusCode.NotFound, (await Client.PutAsJsonAsync("/api/paises/32767",
            new PaisUpdateDto { Nombre = "Inexistente", Capital = "Capital", Iso1 = "TST", Iso2 = "TS" }, Json)).StatusCode);
    }

    [Fact]
    public async Task ClientsIncludingInactiveOnesPreventCatalogDeletion()
    {
        await CreateHierarchy(30004, 200000004, 200000004);
        var cliente = await Create<ClienteDto>("/api/clientes", new ClienteCreateDto
        {
            TipoIdentificacion = TipoIdentificacion.Nit,
            NumeroIdentificacion = "CATALOGO-CLIENTE",
            RazonSocial = "Cliente catálogo",
            PaisCodigo = 30004,
            DepartamentoCodigo = 200000004,
            CiudadCodigo = 200000004
        });
        foreach (var route in new[] { "paises/30004", "departamentos/200000004", "ciudades/200000004" })
            Assert.Equal(HttpStatusCode.Conflict, (await Client.DeleteAsync("/api/" + route)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/clientes/{cliente.Id}")).StatusCode);
        foreach (var route in new[] { "paises/30004", "departamentos/200000004", "ciudades/200000004" })
            Assert.Equal(HttpStatusCode.Conflict, (await Client.DeleteAsync("/api/" + route)).StatusCode);
    }

    [Fact]
    public async Task InactiveLocationsCannotReceiveChildrenOrClients()
    {
        await CreateHierarchy(30005, 200000005, 200000005);
        var cliente = new ClienteCreateDto
        {
            TipoIdentificacion = TipoIdentificacion.Nit,
            NumeroIdentificacion = "INACTIVO",
            RazonSocial = "Cliente",
            PaisCodigo = 30005,
            DepartamentoCodigo = 200000005,
            CiudadCodigo = 200000005
        };
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync("/api/ciudades/200000005")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/clientes", cliente, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync("/api/departamentos/200000005")).StatusCode);
        cliente.CiudadCodigo = null;
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/clientes", cliente, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/ciudades",
            new CiudadCreateDto { Codigo = 200000006, Nombre = "Ciudad", DepartamentoCodigo = 200000005 }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync("/api/paises/30005")).StatusCode);
        cliente.DepartamentoCodigo = null;
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/clientes", cliente, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PostAsJsonAsync("/api/departamentos",
            new DepartamentoCreateDto { Codigo = 200000006, Nombre = "Departamento", PaisCodigo = 30005 }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PutAsJsonAsync("/api/paises/30005",
            new PaisUpdateDto { Nombre = "No editar", Capital = "Capital", Iso1 = "TST", Iso2 = "TS" }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PutAsJsonAsync("/api/departamentos/200000005",
            new DepartamentoUpdateDto { Nombre = "No editar" }, Json)).StatusCode);
    }

    [Fact]
    public async Task ConcurrentCountryCreationReturnsOneSuccessAndOneConflict()
    {
        var dto = Country(30006);
        var responses = await Task.WhenAll(
            Client.PostAsJsonAsync("/api/paises", dto, Json), Client.PostAsJsonAsync("/api/paises", dto, Json));
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConcurrentParentDeletionAndChildCreationCannotLeaveActiveOrphans()
    {
        await Create<PaisDto>("/api/paises", Country(30007));
        var results = await Task.WhenAll(
            Client.DeleteAsync("/api/paises/30007"),
            Client.PostAsJsonAsync("/api/departamentos",
                new DepartamentoCreateDto { Codigo = 200000007, Nombre = "Departamento concurrente", PaisCodigo = 30007 }, Json));
        Assert.All(results, r => Assert.True((int)r.StatusCode < 500));
        await using var db = fixture.CreateContext();
        var parent = await db.Paises.AsNoTracking().SingleAsync(x => x.Codigo == 30007);
        var child = await db.Departamentos.AsNoTracking().SingleOrDefaultAsync(x => x.Codigo == 200000007);
        Assert.False(!parent.Active && child is { Active: true });
    }

    [Fact]
    public async Task SwaggerDocumentsCatalogWrites()
    {
        var document = await Client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        foreach (var route in new[] { "paises", "departamentos", "ciudades" })
        {
            Assert.True(document.GetProperty("paths").GetProperty("/api/" + route).TryGetProperty("post", out _));
            var detail = document.GetProperty("paths").GetProperty("/api/" + route + "/{codigo}");
            Assert.True(detail.TryGetProperty("put", out _));
            Assert.True(detail.TryGetProperty("delete", out _));
        }
    }
}

