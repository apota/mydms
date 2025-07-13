using DMS.SalesManagement.Core.Repositories;
using DMS.SalesManagement.Infrastructure.Data;
using DMS.SalesManagement.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.SalesManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all sales management services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddSalesManagementServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database context
            services.AddDbContext<SalesDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("SalesDatabase"),
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly("DMS.SalesManagement.Infrastructure")));

            // Add repositories
            services.AddScoped<ILeadRepository, LeadRepository>();
            services.AddScoped<IDealRepository, DealRepository>();

            return services;
        }
    }
}
