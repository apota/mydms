using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IVehicleDocumentRepository _documentRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IVehicleDocumentRepository documentRepository,
            IVehicleRepository vehicleRepository,
            IDocumentStorageService documentStorageService,
            ILogger<DocumentsController> logger)
        {
            _documentRepository = documentRepository;
            _vehicleRepository = vehicleRepository;
            _documentStorageService = documentStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Gets documents for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>A list of documents</returns>
        [HttpGet("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDocumentDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<VehicleDocumentDto>>> GetByVehicleId(Guid vehicleId)
        {
            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found");
            }

            var documents = await _documentRepository.GetByVehicleIdAsync(vehicleId);
            return Ok(documents.Select(MapToDto));
        }

        /// <summary>
        /// Gets documents by type for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentType">The document type</param>
        /// <returns>A list of documents</returns>
        [HttpGet("vehicle/{vehicleId:guid}/type/{documentType}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDocumentDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<VehicleDocumentDto>>> GetByVehicleIdAndType(Guid vehicleId, DocumentType documentType)
        {
            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found");
            }

            var documents = await _documentRepository.GetByVehicleIdAndTypeAsync(vehicleId, documentType);
            return Ok(documents.Select(MapToDto));
        }

        /// <summary>
        /// Uploads a document for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The uploaded document</returns>
        [HttpPost("vehicle/{vehicleId:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)] // 50MB
        [ProducesResponseType(typeof(VehicleDocumentDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleDocumentDto>> UploadDocument(Guid vehicleId, [FromForm] UploadVehicleDocumentDto uploadDto)
        {
            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found");
            }

            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            // Generate S3 file path using vin and document title
            var fileName = $"{vehicle.VIN}-{Path.GetFileNameWithoutExtension(uploadDto.File.FileName)}-{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(uploadDto.File.FileName)}";
            
            try
            {
                // Upload the file to S3
                var filePath = await _documentStorageService.UploadDocumentAsync(uploadDto.File, fileName);

                // Create the document record
                var document = new VehicleDocument
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicleId,
                    Title = uploadDto.Title,
                    DocumentType = uploadDto.DocumentType,
                    FilePath = filePath,
                    FileSize = uploadDto.File.Length,
                    MimeType = uploadDto.File.ContentType,
                    UploadDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name
                };

                await _documentRepository.AddAsync(document);
                await (_documentRepository as IUnitOfWork).SaveChangesAsync();

                return CreatedAtAction(nameof(GetByVehicleId), new { vehicleId }, MapToDto(document));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading document");
            }
        }

        /// <summary>
        /// Deletes a document
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            try
            {
                // Delete the file from S3
                await _documentStorageService.DeleteDocumentAsync(document.FilePath);

                // Delete the document record
                await _documentRepository.DeleteAsync(document);
                await (_documentRepository as IUnitOfWork).SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting document");
            }
        }

        /// <summary>
        /// Updates document metadata
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="updateDto">The updated document information</param>
        /// <returns>No content</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, UpdateVehicleDocumentDto updateDto)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            document.Title = updateDto.Title;
            document.DocumentType = updateDto.DocumentType;
            document.UpdatedAt = DateTime.UtcNow;
            document.UpdatedBy = User.Identity?.Name;

            await _documentRepository.UpdateAsync(document);
            await (_documentRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        #region Helper Methods

        private static VehicleDocumentDto MapToDto(VehicleDocument document)
        {
            return new VehicleDocumentDto
            {
                Id = document.Id,
                VehicleId = document.VehicleId,
                Title = document.Title,
                DocumentType = document.DocumentType,
                FilePath = document.FilePath,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                UploadDate = document.UploadDate,
                CreatedAt = document.CreatedAt,
                CreatedBy = document.CreatedBy
            };
        }

        #endregion
    }

    /// <summary>
    /// Data transfer object for uploading a vehicle document
    /// </summary>
    public class UploadVehicleDocumentDto
    {
        /// <summary>
        /// Gets or sets the document title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the file
        /// </summary>
        public IFormFile? File { get; set; }
    }
}
