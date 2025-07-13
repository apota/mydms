using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.FinancialManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAWSServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DynamoDB if used
            services.AddAWSService<IAmazonDynamoDB>(configuration.GetAWSOptions());

            return services;
        }
    }
}
