using System.Reflection;
using System.Text.Json.Serialization;
using CorrecolTest.Application;
using CorrecolTest.Health;
using CorrecolTest.Infrastructure;
using CorrecolTest.Infrastructure.Persistence;
using CorrecolTest.Middleware;
using Microsoft.EntityFrameworkCore;

if (args.Contains("--healthcheck"))
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    try
    {
        var response = await client.GetAsync("http://localhost:8080/health");
        Environment.ExitCode = response.IsSuccessStatusCode ? 0 : 1;
    }
    catch { Environment.ExitCode = 1; }
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CorrecolTest API",
        Version = "v1",
        Description = "Clientes y catálogos geográficos. Fechas UTC. Identificaciones únicas incluso para clientes inactivos."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});
builder.Services.AddApplication(builder.Configuration["AutoMapper:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection."));
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();
if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    app.Logger.LogInformation("Inicializando la base de datos: migraciones y catálogos.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CorrecolDbContext>();
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Base de datos lista. Iniciando la API.");
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "CorrecolTest API v1"));
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
await app.RunAsync();
