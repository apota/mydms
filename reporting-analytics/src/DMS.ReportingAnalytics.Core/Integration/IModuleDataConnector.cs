namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Base interface for all module data connectors.
/// </summary>
public interface IModuleDataConnector
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string ModuleName { get; }
    
    /// <summary>
    /// Checks if the connector is available and can connect to the source system.
    /// </summary>
    /// <returns>True if the connector is available, otherwise false.</returns>
    Task<bool> IsAvailableAsync();
    
    /// <summary>
    /// Gets the available data entities in this module that can be used for reporting.
    /// </summary>
    /// <returns>A collection of data entity metadata.</returns>
    Task<IEnumerable<DataEntityMetadata>> GetAvailableEntitiesAsync();
    
    /// <summary>
    /// Extracts data from the module for a specific entity.
    /// </summary>
    /// <param name="entityName">The name of the entity to extract.</param>
    /// <param name="filter">Optional filter criteria.</param>
    /// <param name="lastExtractTime">The time of the last successful extraction, if any.</param>
    /// <returns>The extracted data as a JSON string.</returns>
    Task<string> ExtractDataAsync(string entityName, string? filter = null, DateTime? lastExtractTime = null);
}

/// <summary>
/// Metadata about a data entity available for reporting.
/// </summary>
public class DataEntityMetadata
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    
    /// <summary>
    /// A description of the entity.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// The schema of the entity as a JSON string.
    /// </summary>
    public string Schema { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the entity supports incremental extraction.
    /// </summary>
    public bool SupportsIncrementalExtraction { get; set; }
    
    /// <summary>
    /// The column used for incremental extraction, if any.
    /// </summary>
    public string? IncrementalExtractionColumn { get; set; }
}
