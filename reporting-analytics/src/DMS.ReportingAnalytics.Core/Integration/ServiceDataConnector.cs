namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Connector for retrieving data from the Service Management module.
/// </summary>
public class ServiceDataConnector : IModuleDataConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServiceDataConnector> _logger;
    private readonly string _serviceManagementApiBaseUrl;

    public ServiceDataConnector(
        HttpClient httpClient,
        ILogger<ServiceDataConnector> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceManagementApiBaseUrl = configuration["ModuleConnections:ServiceManagement:ApiBaseUrl"] 
            ?? throw new ArgumentNullException(nameof(configuration), "ServiceManagement API base URL is not configured.");
    }

    /// <inheritdoc />
    public string ModuleName => "ServiceManagement";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_serviceManagementApiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to the Service Management API.");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DataEntityMetadata>> GetAvailableEntitiesAsync()
    {
        try
        {
            var entities = new List<DataEntityMetadata>
            {
                new() 
                { 
                    EntityName = "ServiceOrders",
                    Description = "All service orders with their details",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "ServiceOrderId", Type = "string", Description = "Unique identifier for the service order" },
                        new() { Name = "VehicleId", Type = "string", Description = "ID of the vehicle being serviced" },
                        new() { Name = "CustomerId", Type = "string", Description = "ID of the customer" },
                        new() { Name = "TechnicianId", Type = "string", Description = "ID of the assigned technician" },
                        new() { Name = "CreatedDate", Type = "datetime", Description = "Date the service order was created" },
                        new() { Name = "CompletedDate", Type = "datetime", Description = "Date the service was completed" },
                        new() { Name = "Status", Type = "string", Description = "Current status of the service order" },
                        new() { Name = "TotalCost", Type = "decimal", Description = "Total cost of the service" },
                        new() { Name = "LaborCost", Type = "decimal", Description = "Cost of labor" },
                        new() { Name = "PartsCost", Type = "decimal", Description = "Cost of parts used" }
                    }
                },
                new() 
                { 
                    EntityName = "ServiceLines",
                    Description = "Individual service line items",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "ServiceLineId", Type = "string", Description = "Unique identifier for the service line" },
                        new() { Name = "ServiceOrderId", Type = "string", Description = "ID of the parent service order" },
                        new() { Name = "ServiceType", Type = "string", Description = "Type of service performed" },
                        new() { Name = "Description", Type = "string", Description = "Description of the service" },
                        new() { Name = "LaborHours", Type = "decimal", Description = "Hours of labor" },
                        new() { Name = "PartId", Type = "string", Description = "ID of part used, if any" },
                        new() { Name = "Quantity", Type = "integer", Description = "Quantity of parts used" },
                        new() { Name = "Cost", Type = "decimal", Description = "Total cost of this service line" }
                    }
                },
                new() 
                { 
                    EntityName = "TechnicianPerformance",
                    Description = "Metrics and KPIs for technician performance",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "TechnicianId", Type = "string", Description = "Technician identifier" },
                        new() { Name = "Date", Type = "date", Description = "Date of performance record" },
                        new() { Name = "JobsCompleted", Type = "integer", Description = "Number of jobs completed" },
                        new() { Name = "LaborHours", Type = "decimal", Description = "Total labor hours logged" },
                        new() { Name = "BillableHours", Type = "decimal", Description = "Total billable hours" },
                        new() { Name = "Efficiency", Type = "decimal", Description = "Efficiency rating (billable/logged hours)" }
                    }
                }
            };
            
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity metadata from Service Management module.");
            return Enumerable.Empty<DataEntityMetadata>();
        }
    }

    /// <inheritdoc />
    public async Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null)
    {
        try
        {
            var requestUrl = $"{_serviceManagementApiBaseUrl}/api/reporting/{entityName}";
            
            // Add query parameters if provided
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(filter))
                queryParams.Add($"filter={Uri.EscapeDataString(filter)}");
                
            if (lastExtractTime.HasValue)
                queryParams.Add($"changedSince={Uri.EscapeDataString(lastExtractTime.Value.ToString("o"))}");
                
            if (queryParams.Any())
                requestUrl += "?" + string.Join("&", queryParams);
            
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract {EntityName} data from Service Management module.", entityName);
            throw new DataExtractionException($"Failed to extract {entityName} data from Service Management module.", ex);
        }
    }
}
