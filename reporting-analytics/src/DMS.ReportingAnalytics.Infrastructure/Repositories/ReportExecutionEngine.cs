using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DMS.ReportingAnalytics.Infrastructure.Repositories;

/// <summary>
/// Implementation of the report execution engine using DynamoDB for caching results.
/// </summary>
public class ReportExecutionEngine : IReportExecutionEngine
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportExecutionEngine> _logger;
    private readonly string _tableName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportExecutionEngine"/> class.
    /// </summary>
    /// <param name="dynamoDbClient">The DynamoDB client.</param>
    /// <param name="reportRepository">The report repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="tableName">The DynamoDB table name for report results.</param>
    public ReportExecutionEngine(
        IAmazonDynamoDB dynamoDbClient,
        IReportRepository reportRepository,
        ILogger<ReportExecutionEngine> logger,
        string tableName = "report-cache")
    {
        _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tableName = tableName;
    }
    
    /// <inheritdoc/>
    public async Task<Guid> ExecuteReportAsync(Guid reportId, string parameters, string userId)
    {
        try
        {
            var executionId = Guid.NewGuid();
            _logger.LogInformation("Starting report execution {ExecutionId} for report {ReportId}", executionId, reportId);
            
            // Retrieve the report
            var report = await _reportRepository.GetReportByIdAsync(reportId);
            
            if (report == null)
            {
                throw new ArgumentException($"Report with ID {reportId} not found");
            }
            
            // Create execution record
            var execution = new ReportExecutionHistory
            {
                ExecutionId = executionId,
                ReportId = reportId,
                UserId = userId,
                ExecutionDate = DateTime.UtcNow,
                Parameters = parameters,
                Status = ExecutionStatus.Running
            };
            
            await _reportRepository.RecordExecutionAsync(execution);
            
            // Start execution asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    // Simulate report execution
                    await Task.Delay(2000); // Simulating processing time
                    
                    // Store results in DynamoDB
                    var mockResults = new
                    {
                        Headers = new[] { "Column1", "Column2", "Column3" },
                        Data = new[]
                        {
                            new object[] { 1, "Value1", 100 },
                            new object[] { 2, "Value2", 200 },
                            new object[] { 3, "Value3", 300 }
                        }
                    };
                    
                    var resultsJson = JsonSerializer.Serialize(mockResults);
                    
                    await StoreResultsAsync(executionId, reportId, resultsJson);
                    
                    // Update execution record
                    execution.Status = ExecutionStatus.Success;
                    execution.Duration = 2000; // milliseconds
                    execution.OutputLocation = $"dynamodb:{_tableName}:{executionId}";
                    
                    await _reportRepository.RecordExecutionAsync(execution);
                    
                    _logger.LogInformation("Completed report execution {ExecutionId} successfully", executionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing report {ReportId} with execution ID {ExecutionId}", reportId, executionId);
                    
                    // Update execution record with error
                    execution.Status = ExecutionStatus.Failed;
                    execution.ErrorMessage = ex.Message;
                    
                    await _reportRepository.RecordExecutionAsync(execution);
                }
            });
            
            return executionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating report execution for report {ReportId}", reportId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<ExecutionStatus> GetExecutionStatusAsync(Guid executionId)
    {
        try
        {
            var key = new Dictionary<string, AttributeValue>
            {
                { "ExecutionId", new AttributeValue { S = executionId.ToString() } }
            };
            
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = key,
                ProjectionExpression = "Status"
            };
            
            var response = await _dynamoDbClient.GetItemAsync(request);
            
            if (response.Item == null || !response.Item.ContainsKey("Status"))
            {
                return ExecutionStatus.Running; // Default to running if not found
            }
            
            var statusValue = response.Item["Status"].S;
            
            if (Enum.TryParse<ExecutionStatus>(statusValue, out var status))
            {
                return status;
            }
            
            return ExecutionStatus.Running;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution status for {ExecutionId}", executionId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> GetExecutionResultsAsync(Guid executionId)
    {
        try
        {
            var key = new Dictionary<string, AttributeValue>
            {
                { "ExecutionId", new AttributeValue { S = executionId.ToString() } }
            };
            
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = key
            };
            
            var response = await _dynamoDbClient.GetItemAsync(request);
            
            if (response.Item == null || !response.Item.ContainsKey("Results"))
            {
                throw new KeyNotFoundException($"Results for execution {executionId} not found");
            }
            
            return response.Item["Results"].S;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution results for {ExecutionId}", executionId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<bool> CancelExecutionAsync(Guid executionId)
    {
        try
        {
            // In a real implementation, you would have a mechanism to signal running tasks to cancel
            
            // Update status in DynamoDB
            var key = new Dictionary<string, AttributeValue>
            {
                { "ExecutionId", new AttributeValue { S = executionId.ToString() } }
            };
            
            var updates = new Dictionary<string, AttributeValueUpdate>
            {
                { 
                    "Status", 
                    new AttributeValueUpdate 
                    { 
                        Action = AttributeAction.PUT,
                        Value = new AttributeValue { S = ExecutionStatus.Canceled.ToString() }
                    }
                }
            };
            
            var request = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = key,
                AttributeUpdates = updates
            };
            
            await _dynamoDbClient.UpdateItemAsync(request);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling execution {ExecutionId}", executionId);
            return false;
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> ExportReportAsync(Guid executionId, string format)
    {
        try
        {
            // Get results
            var results = await GetExecutionResultsAsync(executionId);
            
            // In a real implementation, you would convert the results to the requested format
            // For now, just return the results as-is
            
            return $"Export completed in {format} format: {results.Substring(0, Math.Min(50, results.Length))}...";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report {ExecutionId} to {Format}", executionId, format);
            throw;
        }
    }
    
    private async Task StoreResultsAsync(Guid executionId, Guid reportId, string results)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "ExecutionId", new AttributeValue { S = executionId.ToString() } },
                { "ReportId", new AttributeValue { S = reportId.ToString() } },
                { "Timestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "Results", new AttributeValue { S = results } },
                { "Status", new AttributeValue { S = ExecutionStatus.Success.ToString() } },
                { "TTL", new AttributeValue { N = ((DateTimeOffset)DateTime.UtcNow.AddDays(30)).ToUnixTimeSeconds().ToString() } }
            };
            
            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };
            
            await _dynamoDbClient.PutItemAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing results for execution {ExecutionId}", executionId);
            throw;
        }
    }
}
