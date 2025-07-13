namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Connector for retrieving data from the Customer Relationship Management (CRM) module.
/// </summary>
public class CrmDataConnector : IModuleDataConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CrmDataConnector> _logger;
    private readonly string _crmApiBaseUrl;

    public CrmDataConnector(
        HttpClient httpClient,
        ILogger<CrmDataConnector> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _crmApiBaseUrl = configuration["ModuleConnections:CRM:ApiBaseUrl"] 
            ?? throw new ArgumentNullException(nameof(configuration), "CRM API base URL is not configured.");
    }

    /// <inheritdoc />
    public string ModuleName => "CRM";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_crmApiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to the CRM API.");
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
                    EntityName = "Customers",
                    Description = "Customer master records",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "CustomerId", Type = "string", Description = "Unique identifier for the customer" },
                        new() { Name = "CustomerType", Type = "string", Description = "Type of customer (Individual, Business)" },
                        new() { Name = "FirstName", Type = "string", Description = "Customer's first name" },
                        new() { Name = "LastName", Type = "string", Description = "Customer's last name" },
                        new() { Name = "CompanyName", Type = "string", Description = "Company name for business customers" },
                        new() { Name = "Email", Type = "string", Description = "Primary email address" },
                        new() { Name = "Phone", Type = "string", Description = "Primary phone number" },
                        new() { Name = "Address", Type = "string", Description = "Primary address" },
                        new() { Name = "City", Type = "string", Description = "City" },
                        new() { Name = "State", Type = "string", Description = "State/Province" },
                        new() { Name = "PostalCode", Type = "string", Description = "Postal/Zip code" },
                        new() { Name = "Country", Type = "string", Description = "Country" },
                        new() { Name = "CreatedDate", Type = "datetime", Description = "Date customer record was created" },
                        new() { Name = "LastUpdatedDate", Type = "datetime", Description = "Date customer record was last updated" },
                        new() { Name = "CustomerScore", Type = "integer", Description = "Customer loyalty/value score" }
                    }
                },
                new() 
                { 
                    EntityName = "Leads",
                    Description = "Sales lead information",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "LeadId", Type = "string", Description = "Unique identifier for the lead" },
                        new() { Name = "LeadSource", Type = "string", Description = "Source of the lead" },
                        new() { Name = "ContactInfo", Type = "string", Description = "Contact information" },
                        new() { Name = "AssignedTo", Type = "string", Description = "Sales rep assigned to the lead" },
                        new() { Name = "Status", Type = "string", Description = "Current status of the lead" },
                        new() { Name = "CreatedDate", Type = "datetime", Description = "Date lead was created" },
                        new() { Name = "QualificationDate", Type = "datetime", Description = "Date lead was qualified" },
                        new() { Name = "ConversionDate", Type = "datetime", Description = "Date lead was converted to opportunity" },
                        new() { Name = "InterestLevel", Type = "string", Description = "Indicated level of interest" },
                        new() { Name = "ProductInterest", Type = "string", Description = "Products of interest" }
                    }
                },
                new() 
                { 
                    EntityName = "Opportunities",
                    Description = "Sales opportunities",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "OpportunityId", Type = "string", Description = "Unique identifier for the opportunity" },
                        new() { Name = "CustomerId", Type = "string", Description = "Associated customer ID" },
                        new() { Name = "LeadId", Type = "string", Description = "Original lead ID, if applicable" },
                        new() { Name = "SalesRepId", Type = "string", Description = "Sales rep handling the opportunity" },
                        new() { Name = "EstimatedValue", Type = "decimal", Description = "Estimated value of the opportunity" },
                        new() { Name = "Probability", Type = "decimal", Description = "Probability of closing (%)" },
                        new() { Name = "ExpectedCloseDate", Type = "date", Description = "Expected close date" },
                        new() { Name = "Status", Type = "string", Description = "Current stage in sales pipeline" },
                        new() { Name = "CreatedDate", Type = "datetime", Description = "Date opportunity was created" },
                        new() { Name = "LastUpdatedDate", Type = "datetime", Description = "Date opportunity was last updated" },
                        new() { Name = "CloseDate", Type = "datetime", Description = "Actual close date" },
                        new() { Name = "CloseReason", Type = "string", Description = "Reason for closing (won/lost)" }
                    }
                },
                new() 
                { 
                    EntityName = "CustomerInteractions",
                    Description = "History of interactions with customers",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "InteractionId", Type = "string", Description = "Unique identifier for the interaction" },
                        new() { Name = "CustomerId", Type = "string", Description = "Associated customer ID" },
                        new() { Name = "EmployeeId", Type = "string", Description = "Employee who had the interaction" },
                        new() { Name = "InteractionType", Type = "string", Description = "Type of interaction (call, email, visit, etc.)" },
                        new() { Name = "InteractionDate", Type = "datetime", Description = "Date and time of interaction" },
                        new() { Name = "Duration", Type = "integer", Description = "Duration in minutes" },
                        new() { Name = "Notes", Type = "string", Description = "Notes from the interaction" },
                        new() { Name = "OutcomeCode", Type = "string", Description = "Code indicating the outcome" }
                    }
                },
                new() 
                { 
                    EntityName = "CustomerSegments",
                    Description = "Customer segmentation data",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "CustomerId", Type = "string", Description = "Customer identifier" },
                        new() { Name = "SegmentId", Type = "string", Description = "Segment identifier" },
                        new() { Name = "SegmentName", Type = "string", Description = "Name of the segment" },
                        new() { Name = "LTV", Type = "decimal", Description = "Lifetime value estimate" },
                        new() { Name = "LastPurchaseDate", Type = "date", Description = "Date of last purchase" },
                        new() { Name = "PurchaseFrequency", Type = "decimal", Description = "Average purchases per year" },
                        new() { Name = "RecencyScore", Type = "integer", Description = "Recency score (1-10)" },
                        new() { Name = "FrequencyScore", Type = "integer", Description = "Frequency score (1-10)" },
                        new() { Name = "MonetaryScore", Type = "integer", Description = "Monetary score (1-10)" },
                        new() { Name = "ChurnRisk", Type = "decimal", Description = "Risk of customer churn (0-1)" }
                    }
                }
            };
            
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity metadata from CRM module.");
            return Enumerable.Empty<DataEntityMetadata>();
        }
    }

    /// <inheritdoc />
    public async Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null)
    {
        try
        {
            var requestUrl = $"{_crmApiBaseUrl}/api/reporting/{entityName}";
            
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
            _logger.LogError(ex, "Failed to extract {EntityName} data from CRM module.", entityName);
            throw new DataExtractionException($"Failed to extract {entityName} data from CRM module.", ex);
        }
    }
}
