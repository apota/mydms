namespace DMS.ReportingAnalytics.Core.Integration;

/// <summary>
/// Metadata about a field in a data entity.
/// </summary>
public class DataFieldMetadata
{
    /// <summary>
    /// Field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Field data type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the field.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
