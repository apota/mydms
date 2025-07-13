using DMS.UserManagement.Core.Services;
using DMS.UserManagement.Infrastructure.Data;
using DMS.UserManagement.Infrastructure.Services;
using DMS.UserManagement.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.UserManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
               $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
               $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "dms_users"};" +
               $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "dms_user"};" +
               $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "dms_password"}";

        services.AddDbContext<UserManagementDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add AutoMapper
        services.AddAutoMapper(typeof(UserMappingProfile));

        // Add services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}
