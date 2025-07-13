using Microsoft.Extensions.DependencyInjection;

namespace DMS.Entrypoint.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add core services here
        services.AddHttpClient("SettingsService", client =>
        {
            client.BaseAddress = new Uri("http://settings-service:8090");
            client.DefaultRequestHeaders.Add("User-Agent", "DMS-Entrypoint-API/1.0");
        });
        
        return services;
    }
}
