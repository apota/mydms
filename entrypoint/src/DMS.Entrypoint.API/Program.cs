using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using DMS.Entrypoint.Core;
using DMS.Entrypoint.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Core and Infrastructure services
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices();

// Configure service endpoints
var serviceUrls = new ServiceUrls();
builder.Configuration.GetSection("ServiceUrls").Bind(serviceUrls);
builder.Services.AddSingleton(serviceUrls);

// Add HttpClients for service communication
builder.Services.AddHttpClient("CrmService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.CRM);
});

builder.Services.AddHttpClient("InventoryService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.Inventory);
});

builder.Services.AddHttpClient("SalesService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.Sales);
});

builder.Services.AddHttpClient("ServiceService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.Service);
});

builder.Services.AddHttpClient("PartsService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.Parts);
});

builder.Services.AddHttpClient("ReportingService", client =>
{
    client.BaseAddress = new Uri(serviceUrls.Reporting);
});

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DMS Entrypoint API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DMS Entrypoint API v1"));
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

public class ServiceUrls
{
    public string CRM { get; set; } = "http://localhost:5001";
    public string Inventory { get; set; } = "http://localhost:5002";
    public string Sales { get; set; } = "http://localhost:5003";
    public string Service { get; set; } = "http://localhost:5004";
    public string Parts { get; set; } = "http://localhost:5005";
    public string Reporting { get; set; } = "http://localhost:5006";
}
