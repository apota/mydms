using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DMS.PartsManagement.Infrastructure.Data
{
    public class PartsDbContextFactory : IDesignTimeDbContextFactory<PartsDbContext>
    {
        public PartsDbContext CreateDbContext(string[] args)
        {
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("PartsDb");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<PartsDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Create the context
            return new PartsDbContext(optionsBuilder.Options);
        }
    }
}
