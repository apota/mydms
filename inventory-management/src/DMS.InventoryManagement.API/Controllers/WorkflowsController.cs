using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models.Workflows;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.API.Controllers
{
    /// <summary>
    /// Controller for workflow management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly IAgingManagementService _agingService;
        private readonly IReconditioningService _reconditioningService;
        private readonly IAcquisitionService _acquisitionService;
        private readonly ILogger<WorkflowsController> _logger;        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowsController"/> class
        /// </summary>
        public WorkflowsController(
            IWorkflowService workflowService,
            IAgingManagementService agingService,
            IReconditioningService reconditioningService,
            IAcquisitionService acquisitionService,
            ILogger<WorkflowsController> logger)
        {
            _workflowService = workflowService;
            _agingService = agingService;
            _reconditioningService = reconditioningService;
            _acquisitionService = acquisitionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all workflow definitions
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive workflow definitions</param>
        /// <returns>List of workflow definitions</returns>
        [HttpGet("definitions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions([FromQuery] bool includeInactive = false)
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync(includeInactive);
            return Ok(definitions);
        }

        /// <summary>
        /// Gets a workflow definition by ID
        /// </summary>
        /// <param name="id">The workflow definition ID</param>
        /// <returns>The workflow definition</returns>
        [HttpGet("definitions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
        {
            var definition = await _workflowService.GetWorkflowDefinitionAsync(id);
            
            if (definition == null)
            {
                return NotFound();
            }
            
            return Ok(definition);
        }

        /// <summary>
        /// Creates a new workflow definition
        /// </summary>
        /// <param name="definition">The workflow definition to create</param>
        /// <returns>The created workflow definition</returns>
        [HttpPost("definitions")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition([FromBody] WorkflowDefinition definition)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var created = await _workflowService.CreateWorkflowDefinitionAsync(definition);
            
            return CreatedAtAction(
                nameof(GetWorkflowDefinition),
                new { id = created.Id },
                created);
        }

        /// <summary>
        /// Updates a workflow definition
        /// </summary>
        /// <param name="id">The workflow definition ID</param>
        /// <param name="definition">The updated workflow definition</param>
        /// <returns>No content</returns>
        [HttpPut("definitions/{id}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWorkflowDefinition(Guid id, [FromBody] WorkflowDefinition definition)
        {
            if (id != definition.Id)
            {
                return BadRequest("ID mismatch");
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.UpdateWorkflowDefinitionAsync(definition);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        /// <summary>
        /// Deletes a workflow definition
        /// </summary>
        /// <param name="id">The workflow definition ID</param>
        /// <returns>No content</returns>
        [HttpDelete("definitions/{id}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteWorkflowDefinition(Guid id)
        {
            var result = await _workflowService.DeleteWorkflowDefinitionAsync(id);
            
            if (!result)
            {
                // Check if it doesn't exist or if it has active instances
                var definition = await _workflowService.GetWorkflowDefinitionAsync(id);
                
                if (definition == null)
                {
                    return NotFound();
                }
                
                return Conflict("Cannot delete workflow definition with active instances");
            }
            
            return NoContent();
        }

        /// <summary>
        /// Gets all workflow instances for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>List of workflow instances</returns>
        [HttpGet("vehicle/{vehicleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetVehicleWorkflows(Guid vehicleId)
        {
            var instances = await _workflowService.GetVehicleWorkflowInstancesAsync(vehicleId);
            return Ok(instances);
        }

        /// <summary>
        /// Gets a workflow instance by ID
        /// </summary>
        /// <param name="id">The workflow instance ID</param>
        /// <returns>The workflow instance</returns>
        [HttpGet("instances/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(Guid id)
        {
            var instance = await _workflowService.GetWorkflowInstanceAsync(id);
            
            if (instance == null)
            {
                return NotFound();
            }
            
            return Ok(instance);
        }

        /// <summary>
        /// Creates a new workflow instance
        /// </summary>
        /// <param name="request">The workflow instance creation request</param>
        /// <returns>The created workflow instance</returns>
        [HttpPost("instances")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance([FromBody] CreateWorkflowInstanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var created = await _workflowService.CreateWorkflowInstanceAsync(
                    request.WorkflowDefinitionId, 
                    request.VehicleId, 
                    request.Priority);
                    
                return CreatedAtAction(
                    nameof(GetWorkflowInstance),
                    new { id = created.Id },
                    created);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow instance");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the status of a workflow instance
        /// </summary>
        /// <param name="id">The workflow instance ID</param>
        /// <param name="request">The status update request</param>
        /// <returns>No content</returns>
        [HttpPut("instances/{id}/status")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWorkflowStatus(Guid id, [FromBody] UpdateWorkflowStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.UpdateWorkflowInstanceStatusAsync(id, request.Status);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow status");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the status of a workflow step instance
        /// </summary>
        /// <param name="id">The workflow step instance ID</param>
        /// <param name="request">The status update request</param>
        /// <returns>No content</returns>
        [HttpPut("steps/{id}/status")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStepStatus(Guid id, [FromBody] UpdateStepStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.UpdateWorkflowStepInstanceStatusAsync(id, request.Status, request.Notes);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating step status");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Assigns a workflow step to a user
        /// </summary>
        /// <param name="id">The workflow step instance ID</param>
        /// <param name="request">The assignment request</param>
        /// <returns>No content</returns>
        [HttpPut("steps/{id}/assign")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignStep(Guid id, [FromBody] AssignStepRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.AssignWorkflowStepInstanceAsync(id, request.UserId);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning step");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Approves a workflow step
        /// </summary>
        /// <param name="id">The workflow step instance ID</param>
        /// <param name="request">The approval request</param>
        /// <returns>No content</returns>
        [HttpPut("steps/{id}/approve")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveStep(Guid id, [FromBody] ApproveStepRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.ApproveWorkflowStepInstanceAsync(id, request.UserId, request.Notes);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving step");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Rejects a workflow step
        /// </summary>
        /// <param name="id">The workflow step instance ID</param>
        /// <param name="request">The rejection request</param>
        /// <returns>No content</returns>
        [HttpPut("steps/{id}/reject")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectStep(Guid id, [FromBody] RejectStepRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _workflowService.RejectWorkflowStepInstanceAsync(id, request.UserId, request.Notes);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting step");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Advances a workflow to the next step
        /// </summary>
        /// <param name="id">The workflow instance ID</param>
        /// <returns>The next step or null if the workflow is complete</returns>
        [HttpPost("instances/{id}/advance")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowStepInstance>> AdvanceWorkflow(Guid id)
        {
            try
            {
                var nextStep = await _workflowService.AdvanceWorkflowAsync(id);
                
                if (nextStep == null)
                {
                    return NoContent();
                }
                
                return Ok(nextStep);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing workflow");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets vehicles requiring aging management
        /// </summary>
        /// <param name="daysThreshold">Optional days threshold</param>
        /// <returns>List of vehicles requiring price adjustments</returns>
        [HttpGet("aging/vehicles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetAgingVehicles([FromQuery] int? daysThreshold = null)
        {
            var vehicles = await _agingService.GetVehiclesForPriceAdjustmentAsync(daysThreshold);
            return Ok(vehicles);
        }

        /// <summary>
        /// Gets vehicles with critical aging status
        /// </summary>
        /// <returns>List of vehicles with critical aging</returns>
        [HttpGet("aging/critical")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetCriticalAgingVehicles()
        {
            var vehicles = await _agingService.GetCriticalAgingVehiclesAsync();
            return Ok(vehicles);
        }

        /// <summary>
        /// Creates an aging management workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        [HttpPost("aging/{vehicleId}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowInstance>> CreateAgingWorkflow(Guid vehicleId)
        {
            try
            {
                var workflow = await _agingService.CreateAgingManagementWorkflowAsync(vehicleId);
                
                return CreatedAtAction(
                    nameof(GetWorkflowInstance),
                    new { id = workflow.Id },
                    workflow);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating aging workflow");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets a price adjustment recommendation for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The price adjustment recommendation</returns>
        [HttpGet("aging/{vehicleId}/price-recommendation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PriceAdjustmentRecommendation>> GetPriceRecommendation(Guid vehicleId)
        {
            try
            {
                var recommendation = await _agingService.GetPriceAdjustmentRecommendationAsync(vehicleId);
                return Ok(recommendation);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Applies a price adjustment to a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="request">The price adjustment request</param>
        /// <returns>The price history entry</returns>
        [HttpPost("aging/{vehicleId}/apply-price-adjustment")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PriceHistoryEntry>> ApplyPriceAdjustment(
            Guid vehicleId, 
            [FromBody] ApplyPriceAdjustmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var userId = User.Identity.Name ?? "Unknown";
                
                var priceEntry = await _agingService.ApplyPriceAdjustmentAsync(
                    vehicleId,
                    request.NewPrice,
                    request.Reason,
                    userId);
                    
                return Ok(priceEntry);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying price adjustment");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets vehicles currently in reconditioning
        /// </summary>
        /// <returns>List of vehicles in reconditioning</returns>
        [HttpGet("reconditioning/vehicles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesInReconditioning()
        {
            var vehicles = await _reconditioningService.GetVehiclesInReconditioningAsync();
            return Ok(vehicles);
        }

        /// <summary>
        /// Creates a reconditioning workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        [HttpPost("reconditioning/{vehicleId}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowInstance>> CreateReconditioningWorkflow(Guid vehicleId)
        {
            try
            {
                var workflow = await _reconditioningService.CreateReconditioningWorkflowAsync(vehicleId);
                
                return CreatedAtAction(
                    nameof(GetWorkflowInstance),
                    new { id = workflow.Id },
                    workflow);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reconditioning workflow");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets reconditioning records for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>List of reconditioning records</returns>
        [HttpGet("reconditioning/{vehicleId}/records")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReconditioningRecord>>> GetReconditioningRecords(Guid vehicleId)
        {
            try
            {
                var records = await _reconditioningService.GetVehicleReconditioningRecordsAsync(vehicleId);
                return Ok(records);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets reconditioning summary for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The reconditioning summary</returns>
        [HttpGet("reconditioning/{vehicleId}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReconditioningSummary>> GetReconditioningSummary(Guid vehicleId)
        {
            try
            {
                var summary = await _reconditioningService.GetReconditioningSummaryAsync(vehicleId);
                return Ok(summary);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Adds a reconditioning record for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="record">The reconditioning record</param>
        /// <returns>The created reconditioning record</returns>
        [HttpPost("reconditioning/{vehicleId}/records")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReconditioningRecord>> AddReconditioningRecord(
            Guid vehicleId, 
            [FromBody] ReconditioningRecord record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (vehicleId != record.VehicleId)
            {
                return BadRequest("Vehicle ID mismatch");
            }
            
            try
            {
                // Set the creating user
                record.CreatedBy = User.Identity.Name ?? "Unknown";
                
                var created = await _reconditioningService.AddReconditioningRecordAsync(record);
                
                return CreatedAtAction(
                    nameof(GetReconditioningRecords),
                    new { vehicleId },
                    created);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reconditioning record");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Marks a vehicle as ready for front line
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="request">The request with optional notes</param>
        /// <returns>The updated vehicle</returns>
        [HttpPost("reconditioning/{vehicleId}/complete")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Vehicle>> MarkVehicleReadyForFrontLine(
            Guid vehicleId, 
            [FromBody] CompleteReconditioningRequest request)
        {
            try
            {
                var vehicle = await _reconditioningService.MarkVehicleReadyForFrontLineAsync(
                    vehicleId, 
                    request?.Notes);
                    
                return Ok(vehicle);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking vehicle ready for front line");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets vehicles currently in acquisition
        /// </summary>
        /// <returns>List of vehicles in acquisition</returns>
        [HttpGet("acquisition/vehicles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesInAcquisition()
        {
            var vehicles = await _acquisitionService.GetVehiclesInAcquisitionAsync();
            return Ok(vehicles);
        }
        
        /// <summary>
        /// Creates an acquisition workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        [HttpPost("acquisition/{vehicleId}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkflowInstance>> CreateAcquisitionWorkflow(Guid vehicleId)
        {
            try
            {
                var workflow = await _acquisitionService.CreateAcquisitionWorkflowAsync(vehicleId);
                
                return CreatedAtAction(
                    nameof(GetWorkflowInstance),
                    new { id = workflow.Id },
                    workflow);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating acquisition workflow");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Gets acquisition statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Acquisition statistics</returns>
        [HttpGet("acquisition/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<AcquisitionStatistics>> GetAcquisitionStatistics(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _acquisitionService.GetAcquisitionStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        
        /// <summary>
        /// Records a vehicle inspection
        /// </summary>
        /// <param name="inspection">The inspection details</param>
        /// <returns>The recorded inspection</returns>
        [HttpPost("acquisition/inspections")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleInspection>> RecordVehicleInspection([FromBody] VehicleInspection inspection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var recorded = await _acquisitionService.RecordVehicleInspectionAsync(inspection);
                
                return CreatedAtAction(
                    nameof(GetVehicleInspection),
                    new { vehicleId = recorded.VehicleId },
                    recorded);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording vehicle inspection");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Gets a vehicle inspection
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle inspection</returns>
        [HttpGet("acquisition/{vehicleId}/inspection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleInspection>> GetVehicleInspection(Guid vehicleId)
        {
            var inspection = await _acquisitionService.GetVehicleInspectionAsync(vehicleId);
            
            if (inspection == null)
            {
                return NotFound();
            }
            
            return Ok(inspection);
        }
        
        /// <summary>
        /// Updates acquisition documents for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="request">The document update request</param>
        /// <returns>Success indicator</returns>
        [HttpPost("acquisition/{vehicleId}/documents")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> UpdateAcquisitionDocuments(
            Guid vehicleId, 
            [FromBody] UpdateDocumentsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var result = await _acquisitionService.UpdateAcquisitionDocumentsAsync(vehicleId, request.DocumentIds);
                return Ok(result);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating acquisition documents");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Completes the vehicle intake process
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="request">Optional notes</param>
        /// <returns>The updated vehicle</returns>
        [HttpPost("acquisition/{vehicleId}/complete")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Vehicle>> CompleteVehicleIntake(
            Guid vehicleId, 
            [FromBody] CompleteIntakeRequest request)
        {
            try
            {
                var userId = User.Identity.Name ?? "Unknown";
                var vehicle = await _acquisitionService.CompleteVehicleIntakeAsync(
                    vehicleId, 
                    userId,
                    request?.Notes);
                    
                return Ok(vehicle);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing vehicle intake");
                return BadRequest(ex.Message);
            }
        }
    }
}
