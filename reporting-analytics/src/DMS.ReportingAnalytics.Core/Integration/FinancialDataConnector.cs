namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Connector for retrieving data from the Financial Management module.
/// </summary>
public class FinancialDataConnector : IModuleDataConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinancialDataConnector> _logger;
    private readonly string _financialManagementApiBaseUrl;

    public FinancialDataConnector(
        HttpClient httpClient,
        ILogger<FinancialDataConnector> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _financialManagementApiBaseUrl = configuration["ModuleConnections:FinancialManagement:ApiBaseUrl"] 
            ?? throw new ArgumentNullException(nameof(configuration), "FinancialManagement API base URL is not configured.");
    }

    /// <inheritdoc />
    public string ModuleName => "FinancialManagement";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_financialManagementApiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to the Financial Management API.");
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
                    EntityName = "Transactions",
                    Description = "Financial transactions",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "TransactionId", Type = "string", Description = "Unique identifier for the transaction" },
                        new() { Name = "TransactionDate", Type = "datetime", Description = "Date and time of the transaction" },
                        new() { Name = "PostingDate", Type = "date", Description = "Date transaction was posted to the general ledger" },
                        new() { Name = "TransactionType", Type = "string", Description = "Type of transaction" },
                        new() { Name = "AccountId", Type = "string", Description = "Account identifier" },
                        new() { Name = "DepartmentId", Type = "string", Description = "Department identifier" },
                        new() { Name = "Amount", Type = "decimal", Description = "Transaction amount" },
                        new() { Name = "Description", Type = "string", Description = "Transaction description" },
                        new() { Name = "ReferenceNumber", Type = "string", Description = "Reference number (invoice, PO, etc.)" },
                        new() { Name = "EnteredBy", Type = "string", Description = "User who entered the transaction" }
                    }
                },
                new() 
                { 
                    EntityName = "GeneralLedgerAccounts",
                    Description = "Chart of accounts",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "AccountId", Type = "string", Description = "Unique identifier for the account" },
                        new() { Name = "AccountNumber", Type = "string", Description = "Account number" },
                        new() { Name = "AccountName", Type = "string", Description = "Account name" },
                        new() { Name = "AccountType", Type = "string", Description = "Type of account (Asset, Liability, Equity, Revenue, Expense)" },
                        new() { Name = "ParentAccountId", Type = "string", Description = "Parent account ID for hierarchical accounts" },
                        new() { Name = "IsActive", Type = "boolean", Description = "Whether the account is active" },
                        new() { Name = "CreatedDate", Type = "datetime", Description = "Date the account was created" },
                        new() { Name = "LastModifiedDate", Type = "datetime", Description = "Date the account was last modified" }
                    }
                },
                new() 
                { 
                    EntityName = "Departments",
                    Description = "Financial departments",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "DepartmentId", Type = "string", Description = "Unique identifier for the department" },
                        new() { Name = "DepartmentCode", Type = "string", Description = "Department code" },
                        new() { Name = "DepartmentName", Type = "string", Description = "Department name" },
                        new() { Name = "ManagerId", Type = "string", Description = "ID of department manager" },
                        new() { Name = "IsActive", Type = "boolean", Description = "Whether the department is active" },
                        new() { Name = "CostCenter", Type = "string", Description = "Cost center code" }
                    }
                },
                new() 
                { 
                    EntityName = "FinancialPeriods",
                    Description = "Financial reporting periods",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "PeriodId", Type = "string", Description = "Unique identifier for the period" },
                        new() { Name = "PeriodName", Type = "string", Description = "Period name" },
                        new() { Name = "StartDate", Type = "date", Description = "Start date of period" },
                        new() { Name = "EndDate", Type = "date", Description = "End date of period" },
                        new() { Name = "IsClosed", Type = "boolean", Description = "Whether period is closed for posting" },
                        new() { Name = "FiscalYear", Type = "integer", Description = "Fiscal year" },
                        new() { Name = "FiscalQuarter", Type = "integer", Description = "Fiscal quarter" },
                        new() { Name = "FiscalMonth", Type = "integer", Description = "Fiscal month" }
                    }
                },
                new() 
                { 
                    EntityName = "DepartmentBudgets",
                    Description = "Department budgets by period",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "BudgetId", Type = "string", Description = "Budget entry identifier" },
                        new() { Name = "DepartmentId", Type = "string", Description = "Department identifier" },
                        new() { Name = "AccountId", Type = "string", Description = "Account identifier" },
                        new() { Name = "PeriodId", Type = "string", Description = "Period identifier" },
                        new() { Name = "Amount", Type = "decimal", Description = "Budgeted amount" },
                        new() { Name = "Notes", Type = "string", Description = "Budget notes" },
                        new() { Name = "LastModifiedBy", Type = "string", Description = "User who last modified the budget" },
                        new() { Name = "LastModifiedDate", Type = "datetime", Description = "Date budget was last modified" }
                    }
                },
                new() 
                { 
                    EntityName = "FinancialStatements",
                    Description = "Pre-calculated financial statements",
                    Fields = new List<DataFieldMetadata>
                    {
                        new() { Name = "StatementId", Type = "string", Description = "Statement identifier" },
                        new() { Name = "StatementType", Type = "string", Description = "Type of statement (Income Statement, Balance Sheet, Cash Flow)" },
                        new() { Name = "PeriodId", Type = "string", Description = "Period identifier" },
                        new() { Name = "DepartmentId", Type = "string", Description = "Department identifier (optional)" },
                        new() { Name = "LineItemKey", Type = "string", Description = "Line item key (account or calculated value)" },
                        new() { Name = "LineItemName", Type = "string", Description = "Line item name" },
                        new() { Name = "Amount", Type = "decimal", Description = "Amount for this line item" },
                        new() { Name = "Order", Type = "integer", Description = "Display order" },
                        new() { Name = "Level", Type = "integer", Description = "Hierarchy level" },
                        new() { Name = "IsTotal", Type = "boolean", Description = "Whether this is a total/subtotal line" }
                    }
                }
            };
            
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity metadata from Financial Management module.");
            return Enumerable.Empty<DataEntityMetadata>();
        }
    }

    /// <inheritdoc />
    public async Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null)
    {
        try
        {
            var requestUrl = $"{_financialManagementApiBaseUrl}/api/reporting/{entityName}";
            
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
            _logger.LogError(ex, "Failed to extract {EntityName} data from Financial Management module.", entityName);
            throw new DataExtractionException($"Failed to extract {entityName} data from CRM module.", ex);
        }
    }
}
