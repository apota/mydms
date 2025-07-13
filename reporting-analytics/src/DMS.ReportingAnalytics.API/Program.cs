using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Integration;
using DMS.ReportingAnalytics.Infrastructure.Data;
using DMS.ReportingAnalytics.Infrastructure.Repositories;
using DMS.ReportingAnalytics.API.Services;
using Microsoft.OpenApi.Models;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "DMS Reporting & Analytics API", 
        Version = "v1",
        Description = "API for the Dealership Management System Reporting & Analytics module"
    });
    c.EnableAnnotations();
});

// Configure PostgreSQL for main relational data
builder.Services.AddDbContext<ReportingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReportingDatabase")));

// Register AWS services
builder.Services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
builder.Services.AddAWSService<Amazon.SQS.IAmazonSQS>();

// Register repositories
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IDataMartRepository, DataMartRepository>();

// Register application services
builder.Services.AddScoped<IReportExecutionEngine, ReportExecutionEngine>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAdvancedAnalyticsService, AdvancedAnalyticsService>();

// Register module data connectors
builder.Services.AddHttpClient<InventoryDataConnector>();
builder.Services.AddHttpClient<SalesDataConnector>();
builder.Services.AddHttpClient<ServiceDataConnector>();
builder.Services.AddHttpClient<PartsDataConnector>();
builder.Services.AddHttpClient<CrmDataConnector>();
builder.Services.AddHttpClient<FinancialDataConnector>();

builder.Services.AddScoped<IEnumerable<IModuleDataConnector>>(serviceProvider => 
{
    return new List<IModuleDataConnector>
    {
        serviceProvider.GetRequiredService<InventoryDataConnector>(),
        serviceProvider.GetRequiredService<SalesDataConnector>(),
        serviceProvider.GetRequiredService<ServiceDataConnector>(),
        serviceProvider.GetRequiredService<PartsDataConnector>(),
        serviceProvider.GetRequiredService<CrmDataConnector>(),
        serviceProvider.GetRequiredService<FinancialDataConnector>()
    };
});

// Configure Quartz for scheduling
builder.Services.AddQuartz(q =>
{
    // Register the job
    q.UseMicrosoftDependencyInjectionJobFactory();
    
    // Create a "reporting-schedule" job
    var jobKey = new JobKey("reporting-schedule");
    q.AddJob<ScheduledReportJob>(opts => opts.WithIdentity(jobKey));
    
    // Create a trigger for the job
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("reporting-schedule-trigger")
        .WithCronSchedule("0 0/5 * * * ?"));  // Runs every 5 minutes
    
    // Create a data mart refresh job
    var dataMartJobKey = new JobKey("data-mart-refresh");
    q.AddJob<DataMartRefreshJob>(opts => opts.WithIdentity(dataMartJobKey));
    
    // Create a trigger for the data mart job
    q.AddTrigger(opts => opts
        .ForJob(dataMartJobKey)
        .WithIdentity("data-mart-refresh-trigger")
        .WithCronSchedule("0 0 2 * * ?"));  // Runs at 2:00 AM every day
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Apply migrations in development
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
