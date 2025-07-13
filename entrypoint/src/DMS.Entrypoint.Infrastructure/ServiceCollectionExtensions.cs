using Microsoft.Extensions.DependencyInjection;

namespace DMS.Entrypoint.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Add infrastructure services here (database, external APIs, etc.)
        return services;
    }
}
