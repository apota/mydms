using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Connector for the Sales Management module.
/// </summary>
public class SalesDataConnector : IModuleDataConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SalesDataConnector> _logger;
    private readonly string _baseUrl;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SalesDataConnector"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public SalesDataConnector(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SalesDataConnector> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ModuleIntegration:SalesManagement:BaseUrl"] 
                  ?? "http://sales-management-api";
    }

    /// <inheritdoc/>
    public string ModuleName => "SalesManagement";

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Sales Management module");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataEntityMetadata>> GetAvailableEntitiesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<DataEntityMetadata>>(
                $"{_baseUrl}/api/metadata/entities");
                
            return response ?? Enumerable.Empty<DataEntityMetadata>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available entities from Sales Management module");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null)
    {
        try
        {
            var url = $"{_baseUrl}/api/data/{entityName}";
            
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(filter))
            {
                queryParams.Add($"filter={Uri.EscapeDataString(filter)}");
            }
            
            if (lastExtractTime.HasValue)
            {
                queryParams.Add($"changedSince={Uri.EscapeDataString(lastExtractTime.Value.ToString("o"))}");
            }
            
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting data from Sales Management module for entity {EntityName}", entityName);
            throw;
        }
    }
}
