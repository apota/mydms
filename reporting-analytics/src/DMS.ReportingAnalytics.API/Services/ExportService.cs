using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.API.Services;

/// <summary>
/// Interface for report export operations.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a report in the specified format.
    /// </summary>
    /// <param name="executionId">The execution ID with results to export.</param>
    /// <param name="format">The export format (PDF, Excel, CSV, etc.).</param>
    /// <param name="userId">The ID of the user initiating the export.</param>
    /// <returns>The URL or path to the exported file.</returns>
    Task<string> ExportReportAsync(Guid executionId, string format, string userId);
    
    /// <summary>
    /// Exports and distributes a scheduled report.
    /// </summary>
    /// <param name="executionId">The execution ID with results to export.</param>
    /// <param name="schedule">The schedule containing export and distribution details.</param>
    /// <returns>The URL or path to the exported file.</returns>
    Task<string> ExportScheduledReportAsync(Guid executionId, ScheduledReport schedule);
    
    /// <summary>
    /// Gets the supported export formats.
    /// </summary>
    /// <returns>A collection of supported export format names.</returns>
    Task<IEnumerable<string>> GetSupportedFormatsAsync();
}

/// <summary>
/// Implementation of the export service.
/// </summary>
public class ExportService : IExportService
{
    private readonly IReportExecutionEngine _executionEngine;
    private readonly ILogger<ExportService> _logger;
    private readonly Amazon.S3.IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportService"/> class.
    /// </summary>
    /// <param name="executionEngine">The report execution engine.</param>
    /// <param name="s3Client">The S3 client.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public ExportService(
        IReportExecutionEngine executionEngine,
        Amazon.S3.IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<ExportService> logger)
    {
        _executionEngine = executionEngine;
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<string> ExportReportAsync(Guid executionId, string format, string userId)
    {
        try
        {
            _logger.LogInformation("Exporting report execution {ExecutionId} in format {Format}", executionId, format);
            
            // Get the report data
            var reportData = await _executionEngine.GetExecutionResultsAsync(executionId);
            
            // Generate the export based on format
            var exportData = GenerateExport(reportData, format);
            
            // Upload to S3
            var bucketName = _configuration["AWS:ExportBucket"];
            var key = $"exports/{userId}/{executionId}/{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
            
            await UploadToS3(exportData, bucketName, key);
            
            // Generate pre-signed URL for download
            var url = GeneratePreSignedUrl(bucketName, key);
            
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report {ExecutionId}", executionId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> ExportScheduledReportAsync(Guid executionId, ScheduledReport schedule)
    {
        try
        {
            _logger.LogInformation("Exporting scheduled report {ScheduleId} in format {Format}", 
                schedule.ScheduleId, schedule.Format);
            
            var outputLocation = await ExportReportAsync(executionId, schedule.Format, "system");
            
            // TODO: Send email to recipients
            
            return outputLocation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting scheduled report {ScheduleId}", schedule.ScheduleId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetSupportedFormatsAsync()
    {
        var formats = new List<string>
        {
            "PDF",
            "Excel",
            "CSV",
            "JSON"
        };
        
        return Task.FromResult<IEnumerable<string>>(formats);
    }
    
    private byte[] GenerateExport(string reportData, string format)
    {
        // This would be implemented with appropriate libraries based on format
        // For a real implementation, you would use libraries like:
        // - PDF: iText, PDFsharp
        // - Excel: EPPlus, NPOI
        // - CSV: CsvHelper
        
        // For now, just convert to bytes
        switch (format.ToUpperInvariant())
        {
            case "PDF":
                // TODO: Generate PDF
                return System.Text.Encoding.UTF8.GetBytes("PDF PLACEHOLDER");
                
            case "EXCEL":
                // TODO: Generate Excel
                return System.Text.Encoding.UTF8.GetBytes("EXCEL PLACEHOLDER");
                
            case "CSV":
                // TODO: Generate CSV
                return System.Text.Encoding.UTF8.GetBytes("CSV PLACEHOLDER");
                
            case "JSON":
            default:
                return System.Text.Encoding.UTF8.GetBytes(reportData);
        }
    }
    
    private async Task UploadToS3(byte[] data, string bucketName, string key)
    {
        using var stream = new MemoryStream(data);
        
        var request = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = GetContentType(Path.GetExtension(key))
        };
        
        await _s3Client.PutObjectAsync(request);
    }
    
    private string GeneratePreSignedUrl(string bucketName, string key)
    {
        var request = new Amazon.S3.Model.GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        return _s3Client.GetPreSignedURL(request);
    }
    
    private string GetContentType(string fileExtension)
    {
        return fileExtension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}
