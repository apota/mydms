using Amazon.S3;
using DMS.SalesManagement.Core.Repositories;
using DMS.SalesManagement.Core.Services;
using DMS.SalesManagement.Infrastructure.Data;
using DMS.SalesManagement.Infrastructure.Data.Repositories;
using DMS.SalesManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DMS.SalesManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            ConfigureMiddleware(app, app.Environment);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add controllers
            services.AddControllers();

            // Add API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Add database context
            services.AddDbContext<SalesDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("SalesDatabase")));            // Add repositories
            services.AddScoped<ILeadRepository, LeadRepository>();
            services.AddScoped<IDealRepository, DealRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
              // Add AWS services
            services.AddAWSService<IAmazonS3>();
            
            // Add integration services
            services.AddHttpClient();
            services.AddScoped<IIntegrationService, IntegrationService>();
        }

        private static void ConfigureMiddleware(WebApplication app, IHostEnvironment env)
        {
            // Development-specific middleware
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
