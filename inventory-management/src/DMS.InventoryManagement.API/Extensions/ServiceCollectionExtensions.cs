using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data.Repositories;
using DMS.Shared.Core.Data;
using Microsoft.Extensions.Configuration;
using DMS.InventoryManagement.Core.Services;
using DMS.InventoryManagement.Infrastructure.Services;

namespace DMS.InventoryManagement.API.Extensions
{
    /// <summary>
    /// Extension methods for configuring application services
    /// </summary>
    public static class ServiceCollectionExtensions
    {        /// <summary>
        /// Adds all application services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            
            // Register unit of work
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IVehicleRepository>() as IUnitOfWork);
            
            // Register core services
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleImageService, VehicleImageService>();
            services.AddScoped<IVehicleDocumentService, VehicleDocumentService>();
            services.AddScoped<IInventoryAnalyticsService, InventoryAnalyticsService>();
            services.AddScoped<IExternalMarketDataService, ExternalMarketDataService>();
            services.AddScoped<IFinancialIntegrationService, FinancialIntegrationService>();
            services.AddScoped<IServiceIntegrationService, ServiceIntegrationService>();
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
              // Register workflow services
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IAgingManagementService, AgingManagementService>();
            services.AddScoped<IReconditioningService, ReconditioningService>();
            services.AddScoped<IAcquisitionService, AcquisitionService>();
            
            return services;
        }
        
        /// <summary>
        /// Adds all marketplace integration services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddMarketplaceServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddMarketplaceServices(configuration);
        }
    }
}
