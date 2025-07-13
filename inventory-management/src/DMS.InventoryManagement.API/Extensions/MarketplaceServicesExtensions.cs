using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DMS.InventoryManagement.Core.Services;
using DMS.InventoryManagement.Infrastructure.Services;
using DMS.InventoryManagement.Infrastructure.Services.Marketplaces;

namespace DMS.InventoryManagement.API.Extensions
{
    public static class MarketplaceServicesExtensions
    {
        public static IServiceCollection AddMarketplaceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register marketplace options
            services.Configure<MarketplaceOptions>(configuration.GetSection("Marketplace"));
            
            // Register the main integration service
            services.AddScoped<IMarketplaceIntegrationService, MarketplaceIntegrationService>();
            
            // Register specific marketplace providers
            services.AddScoped<IMarketplaceProvider, AutoTraderMarketplaceProvider>();
            
            // Add additional marketplace providers as needed
            // services.AddScoped<IMarketplaceProvider, CarsComMarketplaceProvider>();
            // services.AddScoped<IMarketplaceProvider, FacebookMarketplaceProvider>();
            
            return services;
        }
    }
}
