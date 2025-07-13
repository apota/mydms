using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the acquisition workflow service
    /// </summary>
    public class AcquisitionService : IAcquisitionService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<AcquisitionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcquisitionService"/> class
        /// </summary>
        /// <param name="vehicleRepository">The vehicle repository</param>
        /// <param name="workflowService">The workflow service</param>
        /// <param name="logger">The logger</param>
        public AcquisitionService(
            IVehicleRepository vehicleRepository,
            IWorkflowService workflowService,
            ILogger<AcquisitionService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Creates an acquisition workflow for a newly acquired vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        public async Task<WorkflowInstance> CreateAcquisitionWorkflowAsync(Guid vehicleId)
        {
            // Get the vehicle
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found", nameof(vehicleId));
            }
            
            // Check if there's already an active acquisition workflow
            var existingWorkflow = await GetActiveAcquisitionWorkflowAsync(vehicleId);
            if (existingWorkflow != null)
            {
                throw new InvalidOperationException($"Vehicle already has an active acquisition workflow");
            }
            
            // Find the acquisition workflow definition
            var definitions = await _workflowService.GetWorkflowDefinitionsByTypeAsync("Acquisition");
            if (!definitions.Any())
            {
                throw new InvalidOperationException("No acquisition workflow definition found");
            }
            
            var definition = definitions.First();
            
            // Update vehicle status if it's not already in Receiving
            if (vehicle.Status != VehicleStatus.Receiving)
            {
                vehicle.Status = VehicleStatus.Receiving;
                await _vehicleRepository.SaveChangesAsync();
            }
            
            // Create the workflow instance
            return await _workflowService.CreateWorkflowInstanceAsync(
                definition.Id,
                vehicleId,
                3 // Normal priority
            );
        }
        
        /// <summary>
        /// Gets the active acquisition workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The active workflow instance, or null if none exists</returns>
        public async Task<WorkflowInstance?> GetActiveAcquisitionWorkflowAsync(Guid vehicleId)
        {
            var workflows = await _workflowService.GetVehicleWorkflowInstancesAsync(vehicleId);
            
            return workflows
                .Where(w => w.WorkflowDefinition.WorkflowType == "Acquisition")
                .Where(w => w.Status == WorkflowStatus.NotStarted || w.Status == WorkflowStatus.InProgress)
                .OrderByDescending(w => w.StartDate)
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Gets vehicles currently in the acquisition process
        /// </summary>
        /// <returns>The list of vehicles in acquisition</returns>
        public async Task<List<Vehicle>> GetVehiclesInAcquisitionAsync()
        {
            // Get vehicles in Receiving or InTransit status
            return await _vehicleRepository.Query()
                .Where(v => v.Status == VehicleStatus.Receiving || v.Status == VehicleStatus.InTransit)
                .Include(v => v.WorkflowInstances)
                    .ThenInclude(w => w.WorkflowDefinition)
                .ToListAsync();
        }
        
        /// <summary>
        /// Records an inspection for a vehicle in acquisition
        /// </summary>
        /// <param name="inspection">The inspection details</param>
        /// <returns>The recorded inspection</returns>
        public async Task<VehicleInspection> RecordVehicleInspectionAsync(VehicleInspection inspection)
        {
            // Get the vehicle
            var vehicle = await _vehicleRepository.GetByIdAsync(inspection.VehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {inspection.VehicleId} not found", nameof(inspection.VehicleId));
            }
            
            // Check if there's an active acquisition workflow
            var workflow = await GetActiveAcquisitionWorkflowAsync(inspection.VehicleId);
            if (workflow == null)
            {
                throw new InvalidOperationException("No active acquisition workflow found for this vehicle");
            }
            
            // Save the inspection
            inspection.InspectionDate = DateTime.UtcNow;
            
            // Add or update the inspection
            var existingInspection = await _vehicleRepository.GetVehicleInspection(inspection.VehicleId);
            if (existingInspection != null)
            {
                // Update existing inspection
                existingInspection.Inspector = inspection.Inspector;
                existingInspection.Notes = inspection.Notes;
                existingInspection.Status = inspection.Status;
                existingInspection.ChecklistItems = inspection.ChecklistItems;
                existingInspection.Issues = inspection.Issues;
                
                await _vehicleRepository.SaveChangesAsync();
                return existingInspection;
            }
            
            // Add new inspection
            _vehicleRepository.AddInspection(inspection);
            await _vehicleRepository.SaveChangesAsync();
            
            // If there's an inspection step in the workflow, update it
            var inspectionStep = workflow.StepInstances
                .FirstOrDefault(s => s.WorkflowStep.Name.Contains("Inspection", StringComparison.OrdinalIgnoreCase));
                
            if (inspectionStep != null && inspectionStep.Status != WorkflowStepStatus.Completed)
            {
                await _workflowService.UpdateWorkflowStepInstanceStatusAsync(
                    inspectionStep.Id,
                    WorkflowStepStatus.Completed,
                    $"Inspection completed on {DateTime.UtcNow}. Status: {inspection.Status}"
                );
                
                // If there are critical issues, put workflow on hold
                if (inspection.Status == InspectionStatus.Failed || 
                    inspection.Issues.Any(i => i.Severity == IssueSeverity.Critical))
                {
                    await _workflowService.UpdateWorkflowInstanceStatusAsync(workflow.Id, WorkflowStatus.OnHold);
                }
                else
                {
                    // Advance workflow if possible
                    await _workflowService.AdvanceWorkflowAsync(workflow.Id);
                }
            }
            
            return inspection;
        }
        
        /// <summary>
        /// Gets the inspection for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle inspection, or null if none exists</returns>
        public async Task<VehicleInspection?> GetVehicleInspectionAsync(Guid vehicleId)
        {
            return await _vehicleRepository.GetVehicleInspection(vehicleId);
        }
        
        /// <summary>
        /// Updates acquisition documents for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentIds">List of document IDs</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateAcquisitionDocumentsAsync(Guid vehicleId, List<Guid> documentIds)
        {
            // Get the vehicle
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found", nameof(vehicleId));
            }
            
            // Check if there's an active acquisition workflow
            var workflow = await GetActiveAcquisitionWorkflowAsync(vehicleId);
            if (workflow == null)
            {
                throw new InvalidOperationException("No active acquisition workflow found for this vehicle");
            }
            
            // Get documents
            var docs = await _vehicleRepository.GetVehicleDocuments(vehicleId, documentIds);
            if (docs.Count != documentIds.Count)
            {
                throw new ArgumentException("One or more document IDs are invalid");
            }
            
            // Mark documents as acquisition documents
            foreach (var doc in docs)
            {
                doc.Category = "Acquisition";
            }
            
            await _vehicleRepository.SaveChangesAsync();
            
            // Update workflow step if there's a documentation step
            var docStep = workflow.StepInstances
                .FirstOrDefault(s => s.WorkflowStep.Name.Contains("Document", StringComparison.OrdinalIgnoreCase));
                
            if (docStep != null && docStep.Status != WorkflowStepStatus.Completed)
            {
                await _workflowService.UpdateWorkflowStepInstanceStatusAsync(
                    docStep.Id,
                    WorkflowStepStatus.Completed,
                    $"Documents updated on {DateTime.UtcNow}. Count: {documentIds.Count}"
                );
                
                // Advance workflow if possible
                await _workflowService.AdvanceWorkflowAsync(workflow.Id);
            }
            
            return true;
        }
        
        /// <summary>
        /// Completes the intake process for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The updated vehicle</returns>
        public async Task<Vehicle> CompleteVehicleIntakeAsync(Guid vehicleId, string userId, string? notes = null)
        {
            // Get the vehicle
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found", nameof(vehicleId));
            }
            
            // Check if there's an active acquisition workflow
            var workflow = await GetActiveAcquisitionWorkflowAsync(vehicleId);
            if (workflow == null)
            {
                throw new InvalidOperationException("No active acquisition workflow found for this vehicle");
            }
            
            // Check if inspection has been done
            var inspection = await GetVehicleInspectionAsync(vehicleId);
            if (inspection == null)
            {
                throw new InvalidOperationException("Vehicle inspection is required before completing intake");
            }
            
            // Complete the workflow
            await _workflowService.UpdateWorkflowInstanceStatusAsync(workflow.Id, WorkflowStatus.Completed);
            
            // Update vehicle status based on inspection results
            if (inspection.Status == InspectionStatus.Failed || 
                inspection.Issues.Any(i => i.Severity == IssueSeverity.Critical || i.Severity == IssueSeverity.Major))
            {
                // If failed or has major/critical issues, send to reconditioning
                vehicle.Status = VehicleStatus.Reconditioning;
                
                // Create reconditioning workflow if available
                try
                {
                    // We'll access this through the controller later
                    //_reconditioningService.CreateReconditioningWorkflowAsync(vehicleId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create reconditioning workflow automatically");
                }
            }
            else
            {
                // If passed, move to InStock
                vehicle.Status = VehicleStatus.InStock;
            }
            
            // Add note if provided
            if (!string.IsNullOrEmpty(notes))
            {
                vehicle.Notes = string.IsNullOrEmpty(vehicle.Notes)
                    ? notes
                    : $"{vehicle.Notes}\n\n{DateTime.UtcNow}: Intake completed: {notes}";
            }
            
            // Update audit info
            vehicle.UpdatedAt = DateTime.UtcNow;
            vehicle.UpdatedBy = userId;
            
            await _vehicleRepository.SaveChangesAsync();
            return vehicle;
        }
        
        /// <summary>
        /// Gets acquisition statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Acquisition statistics</returns>
        public async Task<AcquisitionStatistics> GetAcquisitionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Default to last 30 days if no date range provided
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            var query = _vehicleRepository.Query()
                .Where(v => v.AcquisitionDate >= start && v.AcquisitionDate <= end);
                
            var vehicles = await query.ToListAsync();
            
            var stats = new AcquisitionStatistics
            {
                TotalAcquired = vehicles.Count
            };
            
            // Calculate average cost
            if (vehicles.Any())
            {
                stats.AverageAcquisitionCost = vehicles.Average(v => v.AcquisitionCost);
            }
            
            // Calculate average time to frontline
            var vehiclesWithReconditioning = vehicles
                .Where(v => v.ReconditioningRecords.Any() && v.Status == VehicleStatus.FrontLine)
                .ToList();
                
            if (vehiclesWithReconditioning.Any())
            {
                var avgDays = vehiclesWithReconditioning.Average(v => 
                {
                    var firstRecondRecord = v.ReconditioningRecords.OrderBy(r => r.CreatedAt).First();
                    var lastRecondRecord = v.ReconditioningRecords.OrderByDescending(r => r.CreatedAt).First();
                    
                    return (lastRecondRecord.CreatedAt - v.AcquisitionDate).TotalDays;
                });
                
                stats.AverageTimeToFrontline = avgDays;
            }
            
            // Acquisition sources breakdown
            stats.AcquisitionSources = vehicles
                .GroupBy(v => v.AcquisitionSource ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Vehicles by status
            stats.VehiclesByStatus = vehicles
                .GroupBy(v => v.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Pending inspections
            stats.PendingInspections = await _vehicleRepository.Query()
                .CountAsync(v => v.Status == VehicleStatus.Receiving && !v.WorkflowInstances
                    .Any(w => w.WorkflowDefinition.WorkflowType == "Acquisition" && w.Status == WorkflowStatus.Completed));
                
            return stats;
        }
    }
}
