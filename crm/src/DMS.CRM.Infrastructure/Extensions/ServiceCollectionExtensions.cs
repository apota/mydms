using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DMS.CRM.Core.Repositories;
using DMS.CRM.Core.Services;
using DMS.CRM.Infrastructure.Data;
using DMS.CRM.Infrastructure.Data.Repositories;
using DMS.CRM.Infrastructure.Services;

namespace DMS.CRM.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering CRM Infrastructure services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers CRM Infrastructure services with the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<CrmDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerInteractionRepository, CustomerInteractionRepository>();
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<ICustomerSurveyRepository, CustomerSurveyRepository>();
            services.AddScoped<ICustomerSurveyResponseRepository, CustomerSurveyResponseRepository>();
            
            // Register loyalty repositories
            services.AddScoped<ICustomerLoyaltyRepository, CustomerLoyaltyRepository>();
            services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
            services.AddScoped<ILoyaltyRewardRepository, LoyaltyRewardRepository>();
            services.AddScoped<ILoyaltyRedemptionRepository, LoyaltyRedemptionRepository>();
            services.AddScoped<ILoyaltyTierConfigRepository, LoyaltyTierConfigRepository>();

            // Register services
            services.AddScoped<ILoyaltyService, LoyaltyService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<ICustomerInteractionService, CustomerInteractionService>();
            services.AddScoped<ICustomerJourneyService, CustomerJourneyService>();
            services.AddScoped<ICustomerSegmentService, CustomerSegmentService>();
            services.AddScoped<ICustomerSurveyService, CustomerSurveyService>();
            services.AddScoped<IAIAnalyticsService, AIAnalyticsService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
