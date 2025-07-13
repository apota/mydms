using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using DMS.CRM.Core.Repositories;
using DMS.CRM.Core.Services;
using DMS.CRM.Infrastructure.Data.Repositories;
using DMS.CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DMS.CRM.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure AWS credentials and region
            var awsOptions = configuration.GetAWSOptions();
            
            if (string.IsNullOrEmpty(awsOptions.Region.SystemName))
            {
                awsOptions.Region = RegionEndpoint.USEast1; // Default region if not specified
            }

            // Use environment variables or secrets manager for credentials in production
            if (string.IsNullOrEmpty(awsOptions.Credentials?.GetCredentials().AccessKey))
            {
                var accessKey = configuration["AWS:AccessKey"];
                var secretKey = configuration["AWS:SecretKey"];

                if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                {
                    awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
                }
            }

            services.AddDefaultAWSOptions(awsOptions);
            
            // Add DynamoDB service
            services.AddAWSService<IAmazonDynamoDB>();

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Auth:Authority"];
                options.Audience = configuration["Auth:Audience"];
                
                // For development environments
                if (configuration["Auth:UseDevSettings"]?.ToLower() == "true")
                {
                    options.RequireHttpsMetadata = false;
                    
                    // For local testing with a symmetric key
                    var key = configuration["Auth:SigningKey"];
                    if (!string.IsNullOrEmpty(key))
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                            ValidateIssuer = true,
                            ValidIssuer = configuration["Auth:ValidIssuer"],
                            ValidateAudience = true,
                            ValidAudience = configuration["Auth:Audience"]
                        };
                    }
                }
            });

            return services;
        }

        public static IServiceCollection AddCrmServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerInteractionRepository, CustomerInteractionRepository>();
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<ICustomerSurveyRepository, CustomerSurveyRepository>();
            
            // Additional DynamoDB repository - TODO: Implement when needed
            // services.AddScoped<IDynamoDbCustomerInteractionRepository, DynamoDbCustomerInteractionRepository>();
            
            // Register services - All services are now implemented
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerInteractionService, CustomerInteractionService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<ICustomerSegmentService, CustomerSegmentService>();
            services.AddScoped<ICustomerJourneyService, CustomerJourneyService>();
            services.AddScoped<IAIAnalyticsService, AIAnalyticsService>();
            services.AddScoped<ICustomerSurveyService, CustomerSurveyService>();

            return services;
        }
    }
}
