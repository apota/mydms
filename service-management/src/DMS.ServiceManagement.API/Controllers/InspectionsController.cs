using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Linq;

namespace DMS.ServiceManagement.API.Controllers
{    [ApiController]
    [Route("api/service/inspections")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class InspectionsController : ControllerBase
    {
        private readonly ILogger<InspectionsController> _logger;
        private readonly IServiceInspectionService _inspectionService;
        
        public InspectionsController(
            IServiceInspectionService inspectionService,
            ILogger<InspectionsController> logger)
        {
            _inspectionService = inspectionService ?? throw new ArgumentNullException(nameof(inspectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ServiceInspection> GetInspectionById(Guid id)
        {
            try
            {
                // This would typically call a service to get the inspection
                var inspection = new ServiceInspection
                {
                    Id = id,
                    Type = InspectionType.MultiPoint,
                    Status = InspectionStatus.Completed,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    EndTime = DateTime.UtcNow.AddHours(-1),
                    Notes = "Sample inspection",
                    TechnicianId = Guid.NewGuid(),
                    RepairOrderId = Guid.NewGuid(),
                    RecommendedServices = new List<RecommendedService>
                    {
                        new RecommendedService
                        {
                            Id = Guid.NewGuid(),
                            Description = "Replace brake pads",
                            Urgency = ServiceUrgency.Soon,
                            EstimatedPrice = 250.00m
                        }
                    },
                    InspectionImages = new List<string> { "https://example.com/image1.jpg" }
                };
                
                return Ok(inspection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inspection with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving inspection");
            }
        }

        [HttpGet("repair-order/{repairOrderId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<ServiceInspection>> GetInspectionsByRepairOrderId(Guid repairOrderId)
        {
            try
            {
                // This would typically call a service to get the inspections
                var inspections = new List<ServiceInspection>
                {
                    new ServiceInspection
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Type = InspectionType.MultiPoint,
                        Status = InspectionStatus.Completed,
                        StartTime = DateTime.UtcNow.AddHours(-2),
                        EndTime = DateTime.UtcNow.AddHours(-1),
                        Notes = "Sample multi-point inspection"
                    },
                    new ServiceInspection
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        Type = InspectionType.Safety,
                        Status = InspectionStatus.InProgress,
                        StartTime = DateTime.UtcNow.AddMinutes(-30),
                        EndTime = null,
                        Notes = "Sample safety inspection"
                    }
                };
                
                return Ok(inspections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inspections for repair order ID: {repairOrderId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving inspections");
            }
        }        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceInspection>> CreateInspection([FromBody] ServiceInspection inspection)
        {
            try
            {
                if (inspection == null)
                {
                    return BadRequest("Inspection data is null");
                }

                // Basic validation
                if (inspection.RepairOrderId == Guid.Empty)
                {
                    return BadRequest(new { error = "RepairOrderId is required" });
                }

                if (inspection.TechnicianId == Guid.Empty)
                {
                    return BadRequest(new { error = "TechnicianId is required" });
                }

                if (!Enum.IsDefined(typeof(InspectionType), inspection.Type))
                {
                    return BadRequest(new { error = "Invalid inspection type" });
                }

                // Create the inspection via the service
                var createdInspection = await _inspectionService.CreateInspectionAsync(inspection);
                
                _logger.LogInformation($"Created inspection with ID {createdInspection.Id} for repair order {createdInspection.RepairOrderId}");
                
                return CreatedAtAction(
                    nameof(GetInspectionById), 
                    new { id = createdInspection.Id }, 
                    createdInspection
                );
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error when creating inspection");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection");
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new { error = "An unexpected error occurred while creating the inspection" }
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ServiceInspection> UpdateInspection(Guid id, [FromBody] ServiceInspection inspection)
        {
            try
            {
                if (inspection == null)
                {
                    return BadRequest("Inspection data is null");
                }

                if (id != inspection.Id)
                {
                    return BadRequest("ID mismatch");
                }

                // This would typically call a service to update the inspection
                inspection.UpdatedAt = DateTime.UtcNow;
                
                return Ok(inspection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating inspection with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating inspection");
            }
        }

        [HttpPost("{id:guid}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ServiceInspection> UploadInspectionImages(Guid id, IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files were uploaded");
                }

                // This would typically call a service to upload the images and update the inspection
                var imageUrls = files.Select(file => $"https://example.com/{Guid.NewGuid()}.jpg").ToList();
                
                var inspection = new ServiceInspection
                {
                    Id = id,
                    InspectionImages = imageUrls
                };
                
                return Ok(inspection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading images for inspection with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading images");
            }
        }

        [HttpPost("{id:guid}/recommendations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ServiceInspection> AddRecommendations(Guid id, [FromBody] List<RecommendedService> recommendations)
        {
            try
            {
                if (recommendations == null || recommendations.Count == 0)
                {
                    return BadRequest("Recommendations data is null or empty");
                }

                // This would typically call a service to add recommendations to the inspection
                foreach (var recommendation in recommendations)
                {
                    recommendation.Id = Guid.NewGuid();
                    recommendation.ServiceInspectionId = id;
                }
                
                var inspection = new ServiceInspection
                {
                    Id = id,
                    RecommendedServices = recommendations
                };
                
                return Ok(inspection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding recommendations for inspection with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding recommendations");
            }
        }
    }
}
