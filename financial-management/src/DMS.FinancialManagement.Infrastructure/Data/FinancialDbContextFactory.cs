using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DMS.FinancialManagement.Infrastructure.Data
{
    public class FinancialDbContextFactory : IDesignTimeDbContextFactory<FinancialDbContext>
    {
        public FinancialDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("FinancialDb");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<FinancialDbContext>();
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });

            return new FinancialDbContext(optionsBuilder.Options);
        }
    }
}
