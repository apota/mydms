namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Connector for retrieving data from the Parts Management module.
/// </summary>
public class PartsDataConnector : IModuleDataConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartsDataConnector> _logger;
    private readonly string _partsManagementApiBaseUrl;

    public PartsDataConnector(
        HttpClient httpClient,
        ILogger<PartsDataConnector> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _partsManagementApiBaseUrl = configuration["ModuleConnections:PartsManagement:ApiBaseUrl"] 
            ?? throw new ArgumentNullException(nameof(configuration), "PartsManagement API base URL is not configured.");
    }

    /// <inheritdoc />
    public string ModuleName => "PartsManagement";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_partsManagementApiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to the Parts Management API.");
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
                    EntityName = "PartsInventory",
                    Description = "Current parts inventory with details",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "PartId", Type = "string", Description = "Unique identifier for the part" },
                        new() { Name = "PartNumber", Type = "string", Description = "Manufacturer part number" },
                        new() { Name = "Description", Type = "string", Description = "Description of the part" },
                        new() { Name = "Category", Type = "string", Description = "Part category" },
                        new() { Name = "QuantityOnHand", Type = "integer", Description = "Current quantity in stock" },
                        new() { Name = "QuantityAllocated", Type = "integer", Description = "Quantity allocated but not yet consumed" },
                        new() { Name = "ReorderPoint", Type = "integer", Description = "Minimum quantity before reorder" },
                        new() { Name = "OptimalStock", Type = "integer", Description = "Optimal inventory quantity" },
                        new() { Name = "CostPrice", Type = "decimal", Description = "Cost price of the part" },
                        new() { Name = "RetailPrice", Type = "decimal", Description = "Retail price of the part" }
                    }
                },
                new() 
                { 
                    EntityName = "PartTransactions",
                    Description = "History of all part movements",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "TransactionId", Type = "string", Description = "Unique identifier for the transaction" },
                        new() { Name = "PartId", Type = "string", Description = "ID of the part" },
                        new() { Name = "TransactionType", Type = "string", Description = "Type of transaction (Receipt, Issue, Return, etc.)" },
                        new() { Name = "ReferenceId", Type = "string", Description = "Reference to related document (order, invoice, etc.)" },
                        new() { Name = "ReferenceType", Type = "string", Description = "Type of reference document" },
                        new() { Name = "Quantity", Type = "integer", Description = "Quantity of parts in the transaction" },
                        new() { Name = "TransactionDate", Type = "datetime", Description = "Date and time of the transaction" },
                        new() { Name = "UnitPrice", Type = "decimal", Description = "Unit price in the transaction" },
                        new() { Name = "TotalValue", Type = "decimal", Description = "Total value of the transaction" }
                    }
                },
                new() 
                { 
                    EntityName = "PartUsage",
                    Description = "Usage statistics for parts",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "PartId", Type = "string", Description = "Part identifier" },
                        new() { Name = "Month", Type = "date", Description = "Month of the usage data" },
                        new() { Name = "QuantitySold", Type = "integer", Description = "Number of units sold" },
                        new() { Name = "QuantityUsedInService", Type = "integer", Description = "Number of units used in service" },
                        new() { Name = "TurnoverRate", Type = "decimal", Description = "Inventory turnover rate" },
                        new() { Name = "ProfitMargin", Type = "decimal", Description = "Average profit margin" }
                    }
                },
                new() 
                { 
                    EntityName = "PartSuppliers",
                    Description = "Suppliers for each part",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "PartId", Type = "string", Description = "Part identifier" },
                        new() { Name = "SupplierId", Type = "string", Description = "Supplier identifier" },
                        new() { Name = "SupplierName", Type = "string", Description = "Name of the supplier" },
                        new() { Name = "LeadTime", Type = "integer", Description = "Lead time in days" },
                        new() { Name = "SupplierPartNumber", Type = "string", Description = "Supplier's part number" },
                        new() { Name = "UnitCost", Type = "decimal", Description = "Cost per unit from this supplier" },
                        new() { Name = "IsPreferredSupplier", Type = "boolean", Description = "Whether this is the preferred supplier" }
                    }
                }
            };
            
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity metadata from Parts Management module.");
            return Enumerable.Empty<DataEntityMetadata>();
        }
    }

    /// <inheritdoc />
    public async Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null)
    {
        try
        {
            var requestUrl = $"{_partsManagementApiBaseUrl}/api/reporting/{entityName}";
            
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
            _logger.LogError(ex, "Failed to extract {EntityName} data from Parts Management module.", entityName);
            throw new DataExtractionException($"Failed to extract {entityName} data from Parts Management module.", ex);
        }
    }
}
