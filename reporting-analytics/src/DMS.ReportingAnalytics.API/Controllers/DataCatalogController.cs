using Microsoft.AspNetCore.Mvc;
using DMS.ReportingAnalytics.Core.Integration;
using DMS.ReportingAnalytics.Core.Interfaces;

namespace DMS.ReportingAnalytics.API.Controllers;

[ApiController]
[Route("api/data-catalog")]
public class DataCatalogController : ControllerBase
{
    private readonly ILogger<DataCatalogController> _logger;
    private readonly IEnumerable<IModuleDataConnector> _moduleDataConnectors;
    private readonly IDataMartRepository _dataMartRepository;

    public DataCatalogController(
        ILogger<DataCatalogController> logger,
        IEnumerable<IModuleDataConnector> moduleDataConnectors,
        IDataMartRepository dataMartRepository)
    {
        _logger = logger;
        _moduleDataConnectors = moduleDataConnectors;
        _dataMartRepository = dataMartRepository;
    }

    /// <summary>
    /// Browse available data sources
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSourceInfo>>> GetDataSources()
    {
        try
        {
            var sourceList = new List<DataSourceInfo>();
            
            // Add module data sources
            foreach (var connector in _moduleDataConnectors)
            {
                if (await connector.IsAvailableAsync())
                {
                    sourceList.Add(new DataSourceInfo
                    {
                        SourceId = connector.ModuleName.ToLowerInvariant(),
                        Name = connector.ModuleName,
                        Type = "module",
                        Description = $"{connector.ModuleName} Module Data",
                        IsAvailable = true
                    });
                }
            }
            
            // Add data marts
            var dataMarts = await _dataMartRepository.GetAllDataMartDefinitionsAsync();
            foreach (var mart in dataMarts)
            {
                sourceList.Add(new DataSourceInfo
                {
                    SourceId = $"mart_{mart.MartName.ToLowerInvariant()}",
                    Name = mart.MartName,
                    Type = "datamart",
                    Description = mart.Description,
                    IsAvailable = mart.Status == "Active",
                    LastRefreshDate = mart.LastRefreshDate
                });
            }
            
            return Ok(sourceList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data sources");
            return StatusCode(500, "An error occurred while retrieving data sources");
        }
    }

    /// <summary>
    /// Get details about a specific data source
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DataSourceDetails>> GetDataSourceDetails(string id)
    {
        try
        {
            if (id.StartsWith("mart_"))
            {
                // Get data mart details
                var martName = id.Substring(5);
                var mart = await _dataMartRepository.GetDataMartDefinitionByNameAsync(martName);
                if (mart == null)
                {
                    return NotFound($"Data mart '{martName}' not found");
                }
                
                var details = new DataSourceDetails
                {
                    SourceId = id,
                    Name = mart.MartName,
                    Type = "datamart",
                    Description = mart.Description,
                    IsAvailable = mart.Status == "Active",
                    LastRefreshDate = mart.LastRefreshDate,
                    Schema = await _dataMartRepository.GetDataMartSchemaAsync(martName),
                    SampleData = await _dataMartRepository.GetDataMartSampleDataAsync(martName, 5)
                };
                
                return Ok(details);
            }
            else
            {
                // Get module data source details
                var connector = _moduleDataConnectors.FirstOrDefault(c => 
                    c.ModuleName.ToLowerInvariant() == id);
                    
                if (connector == null)
                {
                    return NotFound($"Data source '{id}' not found");
                }
                
                if (!await connector.IsAvailableAsync())
                {
                    return StatusCode(503, $"Data source '{id}' is currently unavailable");
                }
                
                var entities = await connector.GetAvailableEntitiesAsync();
                var entityList = entities.Select(e => new DataEntityInfo
                {
                    EntityName = e.EntityName,
                    Description = e.Description,
                    Fields = e.Fields?.Select(f => new DataFieldInfo
                    {
                        Name = f.Name,
                        Type = f.Type,
                        Description = f.Description
                    }).ToList() ?? new List<DataFieldInfo>()
                }).ToList();
                
                var details = new DataSourceDetails
                {
                    SourceId = id,
                    Name = connector.ModuleName,
                    Type = "module",
                    Description = $"{connector.ModuleName} Module Data",
                    IsAvailable = true,
                    Entities = entityList
                };
                
                return Ok(details);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving details for data source {Id}", id);
            return StatusCode(500, "An error occurred while retrieving data source details");
        }
    }

    /// <summary>
    /// Get available data fields across all sources
    /// </summary>
    [HttpGet("fields")]
    public async Task<ActionResult<IEnumerable<DataFieldCatalog>>> GetDataFields([FromQuery] string? source = null)
    {
        try
        {
            var fieldCatalog = new List<DataFieldCatalog>();
            
            // Get fields based on source filter
            if (!string.IsNullOrEmpty(source))
            {
                if (source.StartsWith("mart_"))
                {
                    // Get data mart fields
                    var martName = source.Substring(5);
                    var schema = await _dataMartRepository.GetDataMartSchemaAsync(martName);
                    foreach (var column in schema)
                    {
                        fieldCatalog.Add(new DataFieldCatalog
                        {
                            SourceId = source,
                            SourceName = martName,
                            SourceType = "datamart",
                            FieldName = column.Name,
                            DataType = column.DataType,
                            Description = column.Description,
                            IsFilterable = column.IsFilterable,
                            IsSortable = column.IsSortable,
                            IsGroupable = column.IsGroupable
                        });
                    }
                }
                else
                {
                    // Get module fields
                    var connector = _moduleDataConnectors.FirstOrDefault(c => 
                        c.ModuleName.ToLowerInvariant() == source);
                        
                    if (connector == null)
                    {
                        return NotFound($"Data source '{source}' not found");
                    }
                    
                    var entities = await connector.GetAvailableEntitiesAsync();
                    foreach (var entity in entities)
                    {
                        if (entity.Fields == null) continue;
                        
                        foreach (var field in entity.Fields)
                        {
                            fieldCatalog.Add(new DataFieldCatalog
                            {
                                SourceId = source,
                                SourceName = connector.ModuleName,
                                SourceType = "module",
                                EntityName = entity.EntityName,
                                FieldName = field.Name,
                                DataType = field.Type,
                                Description = field.Description,
                                IsFilterable = true,
                                IsSortable = IsTypeOrderable(field.Type),
                                IsGroupable = IsTypeGroupable(field.Type)
                            });
                        }
                    }
                }
            }
            else
            {
                // Get all data mart fields
                var dataMarts = await _dataMartRepository.GetAllDataMartDefinitionsAsync();
                foreach (var mart in dataMarts)
                {
                    var schema = await _dataMartRepository.GetDataMartSchemaAsync(mart.MartName);
                    foreach (var column in schema)
                    {
                        fieldCatalog.Add(new DataFieldCatalog
                        {
                            SourceId = $"mart_{mart.MartName.ToLowerInvariant()}",
                            SourceName = mart.MartName,
                            SourceType = "datamart",
                            FieldName = column.Name,
                            DataType = column.DataType,
                            Description = column.Description,
                            IsFilterable = column.IsFilterable,
                            IsSortable = column.IsSortable,
                            IsGroupable = column.IsGroupable
                        });
                    }
                }
                
                // Get fields from available module connectors
                foreach (var connector in _moduleDataConnectors)
                {
                    if (!await connector.IsAvailableAsync()) continue;
                    
                    var entities = await connector.GetAvailableEntitiesAsync();
                    foreach (var entity in entities)
                    {
                        if (entity.Fields == null) continue;
                        
                        foreach (var field in entity.Fields)
                        {
                            fieldCatalog.Add(new DataFieldCatalog
                            {
                                SourceId = connector.ModuleName.ToLowerInvariant(),
                                SourceName = connector.ModuleName,
                                SourceType = "module",
                                EntityName = entity.EntityName,
                                FieldName = field.Name,
                                DataType = field.Type,
                                Description = field.Description,
                                IsFilterable = true,
                                IsSortable = IsTypeOrderable(field.Type),
                                IsGroupable = IsTypeGroupable(field.Type)
                            });
                        }
                    }
                }
            }
            
            return Ok(fieldCatalog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data fields");
            return StatusCode(500, "An error occurred while retrieving data fields");
        }
    }

    /// <summary>
    /// Get data relationships between fields and entities
    /// </summary>
    [HttpGet("relationships")]
    public async Task<ActionResult<IEnumerable<DataRelationship>>> GetDataRelationships()
    {
        try
        {
            // In a real implementation, this would come from metadata
            // For now, return some sample relationships
            var relationships = new List<DataRelationship>
            {
                new DataRelationship
                {
                    RelationshipId = "rel_customer_sales",
                    SourceEntity = "customers",
                    SourceField = "CustomerId",
                    TargetEntity = "sales",
                    TargetField = "CustomerId",
                    RelationshipType = "one-to-many",
                    Description = "Customer to Sales relationship"
                },
                new DataRelationship
                {
                    RelationshipId = "rel_vehicle_sales",
                    SourceEntity = "vehicles",
                    SourceField = "VehicleId",
                    TargetEntity = "sales",
                    TargetField = "VehicleId",
                    RelationshipType = "one-to-many",
                    Description = "Vehicle to Sales relationship"
                },
                new DataRelationship
                {
                    RelationshipId = "rel_customer_service",
                    SourceEntity = "customers",
                    SourceField = "CustomerId",
                    TargetEntity = "service_orders",
                    TargetField = "CustomerId",
                    RelationshipType = "one-to-many",
                    Description = "Customer to Service Orders relationship"
                },
                new DataRelationship
                {
                    RelationshipId = "rel_vehicle_service",
                    SourceEntity = "vehicles",
                    SourceField = "VehicleId",
                    TargetEntity = "service_orders",
                    TargetField = "VehicleId",
                    RelationshipType = "one-to-many",
                    Description = "Vehicle to Service Orders relationship"
                }
            };
            
            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data relationships");
            return StatusCode(500, "An error occurred while retrieving data relationships");
        }
    }
    
    private bool IsTypeOrderable(string dataType)
    {
        return new[] { "int", "integer", "number", "decimal", "float", "double", "date", "datetime" }.Contains(dataType.ToLowerInvariant());
    }
    
    private bool IsTypeGroupable(string dataType)
    {
        return new[] { "string", "int", "integer", "date", "datetime", "boolean", "bool" }.Contains(dataType.ToLowerInvariant());
    }
}

// Response models

public class DataSourceInfo
{
    public string SourceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "module" or "datamart"
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime? LastRefreshDate { get; set; }
}

public class DataSourceDetails : DataSourceInfo
{
    public IEnumerable<DataEntityInfo>? Entities { get; set; }
    public IEnumerable<DataColumnSchema>? Schema { get; set; }
    public object? SampleData { get; set; }
}

public class DataEntityInfo
{
    public string EntityName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DataFieldInfo> Fields { get; set; } = new();
}

public class DataFieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DataColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public bool IsGroupable { get; set; }
}

public class DataFieldCatalog
{
    public string SourceId { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty; // "module" or "datamart"
    public string? EntityName { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public bool IsGroupable { get; set; }
}

public class DataRelationship
{
    public string RelationshipId { get; set; } = string.Empty;
    public string SourceEntity { get; set; } = string.Empty;
    public string SourceField { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // "one-to-one", "one-to-many", "many-to-many"
    public string Description { get; set; } = string.Empty;
}
