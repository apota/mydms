using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory/operations")]
    public class InventoryOperationsController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IVehicleImageService _imageService;
        private readonly IVehicleDocumentService _documentService;
        private readonly ILogger<InventoryOperationsController> _logger;

        public InventoryOperationsController(
            IVehicleService vehicleService,
            IVehicleImageService imageService,
            IVehicleDocumentService documentService,
            ILogger<InventoryOperationsController> logger)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Transfers a vehicle between locations
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to transfer</param>
        /// <param name="request">The transfer request details</param>
        /// <returns>Status of the transfer operation</returns>
        [HttpPost("vehicles/{vehicleId}/transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TransferVehicle(Guid vehicleId, [FromBody] VehicleTransferRequest request)
        {
            _logger.LogInformation("Transferring vehicle {VehicleId} to location {LocationId}", vehicleId, request.DestinationLocationId);
            
            try
            {
                var result = await _vehicleService.TransferVehicleAsync(vehicleId, request.DestinationLocationId, request.DestinationZoneId, request.TransferReason);
                
                if (result == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }
                
                return Ok(new
                {
                    Success = true,
                    Message = $"Vehicle successfully transferred to location {request.DestinationLocationId}",
                    TransferDate = result.TransferDate,
                    PreviousLocation = result.PreviousLocationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring vehicle {VehicleId}", vehicleId);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Updates the status of a vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="request">The status update request</param>
        /// <returns>Status of the update operation</returns>
        [HttpPost("vehicles/{vehicleId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVehicleStatus(Guid vehicleId, [FromBody] VehicleStatusUpdateRequest request)
        {
            _logger.LogInformation("Updating status for vehicle {VehicleId} to {Status}", vehicleId, request.NewStatus);
            
            try
            {
                var result = await _vehicleService.UpdateVehicleStatusAsync(
                    vehicleId, 
                    request.NewStatus, 
                    request.StatusChangeReason,
                    request.AdditionalInfo);
                
                if (result == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }
                
                return Ok(new
                {
                    Success = true,
                    Message = $"Vehicle status updated to {request.NewStatus}",
                    PreviousStatus = result.PreviousStatus,
                    StatusChangeDate = result.StatusChangeDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for vehicle {VehicleId}", vehicleId);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Uploads vehicle images
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="images">The images to upload</param>
        /// <returns>List of uploaded image URLs</returns>
        [HttpPost("vehicles/{vehicleId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadVehicleImages(Guid vehicleId, [FromForm] IFormFileCollection images)
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest("No images provided");
            }
            
            _logger.LogInformation("Uploading {Count} images for vehicle {VehicleId}", images.Count, vehicleId);
            
            try
            {
                var uploadedImages = await _imageService.UploadVehicleImagesAsync(vehicleId, images);
                
                return Ok(new
                {
                    Success = true,
                    Message = $"Successfully uploaded {uploadedImages.Count} images",
                    Images = uploadedImages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images for vehicle {VehicleId}", vehicleId);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Uploads vehicle documents
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="documents">The documents to upload</param>
        /// <returns>List of uploaded document information</returns>
        [HttpPost("vehicles/{vehicleId}/documents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadVehicleDocuments(Guid vehicleId, [FromForm] VehicleDocumentUploadRequest request)
        {
            if (request.Documents == null || request.Documents.Count == 0)
            {
                return BadRequest("No documents provided");
            }
            
            _logger.LogInformation("Uploading {Count} documents for vehicle {VehicleId}", request.Documents.Count, vehicleId);
            
            try
            {
                var uploadedDocuments = await _documentService.UploadVehicleDocumentsAsync(
                    vehicleId, 
                    request.Documents, 
                    request.DocumentTypes);
                
                return Ok(new
                {
                    Success = true,
                    Message = $"Successfully uploaded {uploadedDocuments.Count} documents",
                    Documents = uploadedDocuments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents for vehicle {VehicleId}", vehicleId);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Bulk imports vehicles from a CSV or other file format
        /// </summary>
        /// <param name="importRequest">The import request containing the file and options</param>
        /// <returns>Result of the import operation</returns>
        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportVehicles([FromForm] VehicleImportRequest importRequest)
        {
            if (importRequest.File == null)
            {
                return BadRequest("No import file provided");
            }
            
            _logger.LogInformation(
                "Starting bulk import of vehicles from file {FileName} with format {Format}", 
                importRequest.File.FileName,
                importRequest.Format);
            
            try
            {
                var importId = await _vehicleService.StartVehicleImportAsync(
                    importRequest.File, 
                    importRequest.Format, 
                    importRequest.Options);
                
                return Accepted(new
                {
                    Success = true,
                    Message = "Vehicle import started",
                    ImportId = importId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting vehicle import");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
