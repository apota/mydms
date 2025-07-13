using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using DMS.PartsManagement.Infrastructure.Data;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Infrastructure.Repositories;
using DMS.PartsManagement.Infrastructure.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add database context
builder.Services.AddDbContext<PartsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IPartInventoryRepository, PartInventoryRepository>();
builder.Services.AddScoped<IPartOrderRepository, PartOrderRepository>();
builder.Services.AddScoped<IPartTransactionRepository, PartTransactionRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPartCoreTrackingRepository, PartCoreTrackingRepository>();

// Register services
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICoreTrackingService, CoreTrackingService>();

// Register Integration Service and configure HttpClient
builder.Services.AddHttpClient<IIntegrationService, IntegrationService>();
builder.Services.Configure<IntegrationSettings>(builder.Configuration.GetSection("Integration"));
builder.Services.AddScoped<IIntegrationService, IntegrationService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Frontend application URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "DMS Parts Management API", 
        Version = "v1",
        Description = "API for managing automotive dealership parts inventory, ordering, and tracking",
        Contact = new OpenApiContact
        {
            Name = "DMS Support",
            Email = "support@dms.example.com"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DMS.PartsManagement.API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and migrations are applied during development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<PartsDbContext>();
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

app.Run();
