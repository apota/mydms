using DMS.LoginManagement.Core.Services;
using DMS.LoginManagement.Infrastructure.Data;
using DMS.LoginManagement.Infrastructure.Mapping;
using DMS.LoginManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.LoginManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
               $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
               $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "dms_login"};" +
               $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "dms_user"};" +
               $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "dms_password"}";

        services.AddDbContext<LoginManagementDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add AutoMapper
        services.AddAutoMapper(typeof(AuthMappingProfile));

        // Add services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}
