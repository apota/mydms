using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    [Authorize]
    public class ImportController : ControllerBase
    {
        private readonly IVehicleImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(
            IVehicleImportService importService,
            ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        /// <summary>
        /// Gets available CSV mapping templates
        /// </summary>
        /// <returns>List of available templates</returns>
        [HttpGet("templates")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(IEnumerable<MappingTemplateDto>), 200)]
        public async Task<ActionResult<IEnumerable<MappingTemplateDto>>> GetMappingTemplates()
        {
            try
            {
                var templates = await _importService.GetAvailableMappingTemplatesAsync();
                return Ok(templates.Select(t => new MappingTemplateDto
                {
                    Name = t.Name,
                    Description = t.Description
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mapping templates");
                return StatusCode(500, "An error occurred while retrieving mapping templates");
            }
        }

        /// <summary>
        /// Gets available manufacturer codes
        /// </summary>
        /// <returns>List of available manufacturer codes</returns>
        [HttpGet("manufacturers")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetManufacturerCodes()
        {
            try
            {
                var codes = await _importService.GetAvailableManufacturerCodesAsync();
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturer codes");
                return StatusCode(500, "An error occurred while retrieving manufacturer codes");
            }
        }

        /// <summary>
        /// Gets available auction codes
        /// </summary>
        /// <returns>List of available auction codes</returns>
        [HttpGet("auctions")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetAuctionCodes()
        {
            try
            {
                var codes = await _importService.GetAvailableAuctionCodesAsync();
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auction codes");
                return StatusCode(500, "An error occurred while retrieving auction codes");
            }
        }

        /// <summary>
        /// Imports vehicles from a CSV file
        /// </summary>
        /// <param name="file">The CSV file</param>
        /// <param name="templateName">The mapping template name</param>
        /// <returns>Import result</returns>
        [HttpPost("csv")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(ImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportResultDto>> ImportFromCsv([FromForm] IFormFile file, [FromForm] string templateName)
        {
            _logger.LogInformation("Importing vehicles from CSV file using template: {template}", templateName);
            
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file was provided");
                }

                if (string.IsNullOrEmpty(templateName))
                {
                    return BadRequest("No template name was provided");
                }

                // Save the file to a temporary location
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Import the vehicles
                var result = await _importService.ImportFromCsvAsync(filePath, templateName);
                
                // Delete the temporary file
                System.IO.File.Delete(filePath);

                return Ok(MapToDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from CSV");
                return StatusCode(500, "An error occurred while importing vehicles from CSV");
            }
        }

        /// <summary>
        /// Imports vehicles from a manufacturer feed
        /// </summary>
        /// <param name="manufacturerCode">The manufacturer code</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result</returns>
        [HttpPost("manufacturer/{manufacturerCode}")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(ImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportResultDto>> ImportFromManufacturer(
            string manufacturerCode, 
            [FromBody] ManufacturerImportOptionsDto options)
        {
            _logger.LogInformation("Importing vehicles from manufacturer: {manufacturer}", manufacturerCode);
            
            try
            {
                if (string.IsNullOrEmpty(manufacturerCode))
                {
                    return BadRequest("No manufacturer code was provided");
                }

                // Map options to core model
                var coreOptions = new ManufacturerImportOptions
                {
                    NewInventoryOnly = options.NewInventoryOnly,
                    UpdateExisting = options.UpdateExisting,
                    DealerCodes = options.DealerCodes.ToList(),
                    ArrivalDateFilter = options.StartDate.HasValue || options.EndDate.HasValue
                        ? new DateRangeFilter { StartDate = options.StartDate, EndDate = options.EndDate }
                        : null
                };

                // Import the vehicles
                var result = await _importService.ImportFromManufacturerFeedAsync(manufacturerCode, coreOptions);

                return Ok(MapToDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from manufacturer: {manufacturer}", manufacturerCode);
                return StatusCode(500, "An error occurred while importing vehicles from manufacturer");
            }
        }

        /// <summary>
        /// Imports vehicles from an auction platform
        /// </summary>
        /// <param name="auctionCode">The auction code</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result</returns>
        [HttpPost("auction/{auctionCode}")]
        [Authorize(Policy = "InventoryImport")]
        [ProducesResponseType(typeof(ImportResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportResultDto>> ImportFromAuction(
            string auctionCode, 
            [FromBody] AuctionImportOptionsDto options)
        {
            _logger.LogInformation("Importing vehicles from auction: {auction}", auctionCode);
            
            try
            {
                if (string.IsNullOrEmpty(auctionCode))
                {
                    return BadRequest("No auction code was provided");
                }

                // Map options to core model
                var coreOptions = new AuctionImportOptions
                {
                    WonAuctionsOnly = options.WonAuctionsOnly,
                    ActiveAuctionsOnly = options.ActiveAuctionsOnly,
                    WatchedAuctionsOnly = options.WatchedAuctionsOnly,
                    Makes = options.Makes.ToList(),
                    AuctionDateFilter = options.StartDate.HasValue || options.EndDate.HasValue
                        ? new DateRangeFilter { StartDate = options.StartDate, EndDate = options.EndDate }
                        : null
                };

                // Import the vehicles
                var result = await _importService.ImportFromAuctionAsync(auctionCode, coreOptions);

                return Ok(MapToDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from auction: {auction}", auctionCode);
                return StatusCode(500, "An error occurred while importing vehicles from auction");
            }
        }

        private ImportResultDto MapToDto(VehicleImportResult result)
        {
            return new ImportResultDto
            {
                Success = result.Success,
                TotalRecords = result.TotalRecords,
                SuccessCount = result.SuccessCount,
                ErrorCount = result.ErrorCount,
                ImportedVehicleIds = result.ImportedVehicles.Select(v => v.Id).ToList(),
                Errors = result.Errors,
                Warnings = result.Warnings.Select(w => new ValidationWarningDto
                {
                    RowNumber = w.RowNumber,
                    FieldName = w.FieldName,
                    Message = w.Message
                }).ToList()
            };
        }
    }

    /// <summary>
    /// DTO for mapping template
    /// </summary>
    public class MappingTemplateDto
    {
        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the template description
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for manufacturer import options
    /// </summary>
    public class ManufacturerImportOptionsDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether to import only new inventory
        /// </summary>
        public bool NewInventoryOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to update existing inventory
        /// </summary>
        public bool UpdateExisting { get; set; }
        
        /// <summary>
        /// Gets or sets dealer codes to filter by
        /// </summary>
        public IEnumerable<string> DealerCodes { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the start date to filter by arrival date
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date to filter by arrival date
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO for auction import options
    /// </summary>
    public class AuctionImportOptionsDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include only won auctions
        /// </summary>
        public bool WonAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to include only active auctions
        /// </summary>
        public bool ActiveAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to include only auctions the dealership is watching
        /// </summary>
        public bool WatchedAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets the start date to filter by auction date
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date to filter by auction date
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the makes to filter by
        /// </summary>
        public IEnumerable<string> Makes { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for import result
    /// </summary>
    public class ImportResultDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the import was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of records processed
        /// </summary>
        public int TotalRecords { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records successfully imported
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records that failed to import
        /// </summary>
        public int ErrorCount { get; set; }
        
        /// <summary>
        /// Gets or sets the list of errors encountered during import
        /// </summary>
        public List<string> Errors { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of imported vehicle IDs
        /// </summary>
        public List<Guid> ImportedVehicleIds { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of validation warnings
        /// </summary>
        public List<ValidationWarningDto> Warnings { get; set; } = new();
    }

    /// <summary>
    /// DTO for validation warning
    /// </summary>
    public class ValidationWarningDto
    {
        /// <summary>
        /// Gets or sets the row number in the source data
        /// </summary>
        public int RowNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the field name with the warning
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the warning message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
