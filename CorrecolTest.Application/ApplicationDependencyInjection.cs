using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace CorrecolTest.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        services.AddAutoMapper(config =>
        {
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey)) config.LicenseKey = autoMapperLicenseKey;
            config.AddProfile<ApplicationProfile>();
        });
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        return services;
    }
}

