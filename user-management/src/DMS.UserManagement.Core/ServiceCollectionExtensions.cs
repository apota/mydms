using DMS.UserManagement.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.UserManagement.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        
        return services;
    }
}
