using DMS.SettingsManagement.Core.Services;
using DMS.SettingsManagement.Infrastructure.Data;
using DMS.SettingsManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
       $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
       $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "dms_settings"};" +
       $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "dms_user"};" +
       $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "dms_password"}";

builder.Services.AddDbContext<SettingsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add application services
builder.Services.AddScoped<ISettingsService, SettingsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the database.");
    }
}

app.Run();
