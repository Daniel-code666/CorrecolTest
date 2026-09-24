using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClosedXML.Excel;
using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Common;
using CorrecolTest.Domain.Entities;
using CorrecolTest.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CorrecolTest.IntegrationTests;

public class ApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private HttpClient Client => fixture.Client;
    private static ClienteCreateDto Valid(string? number = null) => new()
    {
        TipoIdentificacion = TipoIdentificacion.Nit,
        NumeroIdentificacion = number ?? Guid.NewGuid().ToString("N")[..25],
        RazonSocial = "Cliente de prueba",
        PaisCodigo = 170,
        DepartamentoCodigo = 5,
        CiudadCodigo = 5001
    };
    private async Task<ClienteDto> Create(ClienteCreateDto? dto = null)
    {
        var response = await Client.PostAsJsonAsync("/api/clientes", dto ?? Valid(), Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return (await response.Content.ReadFromJsonAsync<ClienteDto>(Json))!;
    }

    [Fact]
    public async Task FreshDatabaseHasCompleteSeedAndMigration()
    {
        await using var db = fixture.CreateContext();
        Assert.Equal(246, await db.Paises.CountAsync());
        Assert.Equal(33, await db.Departamentos.CountAsync());
        Assert.Equal(1118, await db.Ciudades.CountAsync());
        Assert.Single(await db.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.All(await db.Paises.ToListAsync(), p =>
        {
            Assert.True(p.CreationDate > new DateTime(2020, 1, 1));
            Assert.Equal(DateTimeKind.Utc, p.CreationDate.Kind);
        });
    }

    [Fact]
    public async Task SeedRepairsPartialCatalogWithoutOverwritingExistingRows()
    {
        await using var db = fixture.CreateContext();
        var country = await db.Paises.SingleAsync(x => x.Codigo == 4);
        var original = country.Capital;
        country.Capital = "CAPITAL MODIFICADA";
        await db.SaveChangesAsync();
        var creationDate = country.CreationDate;
        var city = await db.Ciudades.OrderByDescending(x => x.Codigo).FirstAsync();
        var code = city.Codigo;
        db.Ciudades.Remove(city);
        await db.SaveChangesAsync();
        await db.Database.MigrateAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(1118, await db.Ciudades.CountAsync());
        Assert.True(await db.Ciudades.AnyAsync(x => x.Codigo == code));
        country = await db.Paises.SingleAsync(x => x.Codigo == 4);
        Assert.Equal("CAPITAL MODIFICADA", country.Capital);
        Assert.Equal(creationDate, country.CreationDate);
        country.Capital = original;
        await db.SaveChangesAsync();
        // El callback síncrono, utilizado por las herramientas EF, también es idempotente.
        db.Database.Migrate();
        Assert.Equal(246, await db.Paises.CountAsync());
    }

    [Fact]
    public async Task CatalogsSupportFiltersPaginationAndDetails()
    {
        var countries = await Client.GetFromJsonAsync<PagedResult<PaisDto>>("/api/paises?nombre=colombia&pageSize=1", Json);
        Assert.Equal(1, countries!.TotalRecords);
        Assert.Equal((short)170, Assert.Single(countries.Items).Codigo);
        var departments = await Client.GetFromJsonAsync<PagedResult<DepartamentoDto>>("/api/departamentos?paisCodigo=170&pageSize=10&pageNumber=2", Json);
        Assert.Equal(33, departments!.TotalRecords);
        Assert.Equal(10, departments.Items.Count);
        var cities = await Client.GetFromJsonAsync<PagedResult<CiudadDto>>("/api/ciudades?departamentoCodigo=5&paisCodigo=170&codigo=5001", Json);
        var city = Assert.Single(cities!.Items);
        Assert.Equal("MEDELLIN", city.Nombre);
        var detail = await Client.GetFromJsonAsync<CiudadDto>("/api/ciudades/5001", Json);
        Assert.Equal((short)170, detail!.PaisCodigo);
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/api/paises/170")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/api/departamentos/5")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/ciudades/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/paises?pageSize=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/ciudades?pageNumber=2147483647&pageSize=100")).StatusCode);
    }

    [Fact]
    public async Task AntioquiaCatalogCanBeReadCompletelyAcrossPages()
    {
        var first = (await Client.GetFromJsonAsync<PagedResult<CiudadDto>>(
            "/api/ciudades?departamentoCodigo=5&pageNumber=1&pageSize=100", Json))!;
        var second = (await Client.GetFromJsonAsync<PagedResult<CiudadDto>>(
            "/api/ciudades?departamentoCodigo=5&pageNumber=2&pageSize=100", Json))!;
        var third = (await Client.GetFromJsonAsync<PagedResult<CiudadDto>>(
            "/api/ciudades?departamentoCodigo=5&pageNumber=3&pageSize=100", Json))!;
        Assert.Equal(125, first.TotalRecords);
        Assert.Equal(first.TotalRecords, second.TotalRecords);
        Assert.Equal(first.TotalRecords, third.TotalRecords);
        Assert.Equal(100, first.Items.Count);
        Assert.Equal(25, second.Items.Count);
        Assert.Empty(third.Items);
        var codes = first.Items.Concat(second.Items).Select(x => x.Codigo).ToArray();
        Assert.Equal(125, codes.Distinct().Count());
        Assert.Equal(codes.OrderBy(x => x), codes);
        await using var db = fixture.CreateContext();
        var expected = await db.Ciudades.Where(x => x.DepartamentoCodigo == 5)
            .OrderBy(x => x.Codigo).Select(x => x.Codigo).ToArrayAsync();
        Assert.Equal(expected, codes);
    }

    [Fact]
    public async Task ClientLifecyclePreservesAuditAndSoftDeletes()
    {
        var dto = Valid("00" + Guid.NewGuid().ToString("N")[..20]);
        var created = await Create(dto);
        Assert.True(created.Active);
        Assert.Equal(dto.NumeroIdentificacion, created.NumeroIdentificacion);
        Assert.Equal("COLOMBIA", created.PaisNombre);
        Assert.Equal("ANTIOQUIA", created.DepartamentoNombre);
        Assert.Equal("MEDELLIN", created.CiudadNombre);
        Assert.Null(created.UpdatedDate);
        Assert.Equal(DateTimeKind.Utc, created.CreationDate.Kind);

        var update = new ClienteUpdateDto
        {
            TipoIdentificacion = dto.TipoIdentificacion,
            NumeroIdentificacion = dto.NumeroIdentificacion,
            RazonSocial = "Cliente actualizado",
            PaisCodigo = 4
        };
        var response = await Client.PutAsJsonAsync($"/api/clientes/{created.Id}", update, Json);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<ClienteDto>(Json))!;
        Assert.Equal(created.CreationDate, updated.CreationDate);
        Assert.NotNull(updated.UpdatedDate);
        Assert.Null(updated.DepartamentoCodigo);
        Assert.Null(updated.CiudadNombre);

        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/clientes/{created.Id}")).StatusCode);
        var inactive = (await Client.GetFromJsonAsync<ClienteDto>($"/api/clientes/{created.Id}", Json))!;
        Assert.False(inactive.Active);
        Assert.NotNull(inactive.UpdatedDate);
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/clientes/{created.Id}")).StatusCode);
        var repeated = (await Client.GetFromJsonAsync<ClienteDto>($"/api/clientes/{created.Id}", Json))!;
        Assert.Equal(inactive.UpdatedDate, repeated.UpdatedDate);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PutAsJsonAsync($"/api/clientes/{created.Id}", update, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Client.PostAsJsonAsync("/api/clientes", dto, Json)).StatusCode);

        var active = await Client.GetFromJsonAsync<PagedResult<ClienteDto>>($"/api/clientes?numeroIdentificacion={dto.NumeroIdentificacion}", Json);
        Assert.Empty(active!.Items);
        var all = await Client.GetFromJsonAsync<PagedResult<ClienteDto>>($"/api/clientes?active=&numeroIdentificacion={dto.NumeroIdentificacion}", Json);
        Assert.Single(all!.Items);
        var onlyInactive = await Client.GetFromJsonAsync<PagedResult<ClienteDto>>($"/api/clientes?active=false&numeroIdentificacion={dto.NumeroIdentificacion}", Json);
        Assert.Single(onlyInactive!.Items);
    }

    [Fact]
    public async Task ValidationAndErrorsUseProblemDetails()
    {
        var dto = Valid(); dto.CiudadCodigo = 8001;
        await AssertProblem(await Client.PostAsJsonAsync("/api/clientes", dto, Json), HttpStatusCode.BadRequest);
        dto = Valid(); dto.DepartamentoCodigo = null; dto.CiudadCodigo = null;
        await AssertProblem(await Client.PostAsJsonAsync("/api/clientes", dto, Json), HttpStatusCode.BadRequest);
        dto = Valid(); dto.NumeroIdentificacion = new string('1', 31);
        await AssertProblem(await Client.PostAsJsonAsync("/api/clientes", dto, Json), HttpStatusCode.BadRequest);
        dto = Valid(); dto.TipoIdentificacion = (TipoIdentificacion)99;
        await AssertProblem(await Client.PostAsJsonAsync("/api/clientes", dto, Json), HttpStatusCode.BadRequest);
        dto = Valid(); dto.PaisCodigo = 32767;
        await AssertProblem(await Client.PostAsJsonAsync("/api/clientes", dto, Json), HttpStatusCode.BadRequest);
        await AssertProblem(await Client.GetAsync("/api/clientes/2147483647"), HttpStatusCode.NotFound);
        await AssertProblem(await Client.PostAsync("/api/clientes", new StringContent("{bad", System.Text.Encoding.UTF8, "application/json")), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConcurrentDuplicatesProduceOneCreatedAndOneConflict()
    {
        var dto = Valid();
        var responses = await Task.WhenAll(Client.PostAsJsonAsync("/api/clientes", dto, Json), Client.PostAsJsonAsync("/api/clientes", dto, Json));
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ExportIncludesEveryMatchingRowAndPreservesText()
    {
        var tag = "Export-" + Guid.NewGuid().ToString("N")[..10];
        for (var i = 0; i < 3; i++)
        {
            var dto = Valid("00" + Guid.NewGuid().ToString("N")[..20]);
            dto.RazonSocial = i == 0 ? "=" + tag : tag;
            await Create(dto);
        }
        var page = await Client.GetFromJsonAsync<PagedResult<ClienteDto>>($"/api/clientes?razonSocial={tag}&pageSize=1", Json);
        Assert.Single(page!.Items);
        Assert.Equal(3, page.TotalRecords);
        var response = await Client.GetAsync($"/api/clientes/exportacion?razonSocial={tag}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType!.MediaType);
        using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
        using var book = new XLWorkbook(stream);
        var sheet = book.Worksheet("Clientes");
        Assert.Equal(4, sheet.LastRowUsed()!.RowNumber());
        Assert.StartsWith("00", sheet.Cell(2, 3).GetString());
        Assert.False(sheet.Cell(2, 4).HasFormula);
        Assert.Equal("COLOMBIA", sheet.Cell(2, 5).GetString());
        var empty = await Client.GetAsync("/api/clientes/exportacion?razonSocial=no-existe-" + tag);
        using var emptyBook = new XLWorkbook(new MemoryStream(await empty.Content.ReadAsByteArrayAsync()));
        Assert.Equal(1, emptyBook.Worksheet(1).LastRowUsed()!.RowNumber());
    }

    [Fact]
    public async Task DatabaseEnforcesUniquenessAndGeographicRelations()
    {
        var dto = Valid();
        await Create(dto);
        await using (var db = fixture.CreateContext())
        {
            db.Clientes.Add(new Cliente
            {
                TipoIdentificacion = dto.TipoIdentificacion,
                NumeroIdentificacion = dto.NumeroIdentificacion,
                RazonSocial = "Duplicado",
                PaisCodigo = 4
            });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        await using (var db = fixture.CreateContext())
        {
            db.Clientes.Add(new Cliente
            {
                TipoIdentificacion = TipoIdentificacion.Nit,
                NumeroIdentificacion = Guid.NewGuid().ToString("N")[..25],
                RazonSocial = "Ubicación inválida",
                PaisCodigo = 4,
                DepartamentoCodigo = 5,
                CiudadCodigo = 5001
            });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task SwaggerAndHealthAreAvailable()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/clientes/exportacion", out _));
        Assert.True(paths.TryGetProperty("/api/clientes/{id}", out var clientPath));
        Assert.True(clientPath.TryGetProperty("delete", out _));
        Assert.True(paths.TryGetProperty("/api/ciudades/{codigo}", out _));
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/health")).StatusCode);
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        Assert.False(body.TryGetProperty("stackTrace", out _));
    }
}
