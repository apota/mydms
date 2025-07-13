using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Core.Services;

namespace DMS.ServiceManagement.Core.Services.Implementations
{
    public class ServiceJobService : IServiceJobService
    {
        private readonly IServiceJobRepository _serviceJobRepository;
        private readonly IRepairOrderService _repairOrderService;

        public ServiceJobService(
            IServiceJobRepository serviceJobRepository,
            IRepairOrderService repairOrderService)
        {
            _serviceJobRepository = serviceJobRepository;
            _repairOrderService = repairOrderService;
        }

        public async Task<IEnumerable<ServiceJob>> GetAllServiceJobsAsync()
        {
            return await _serviceJobRepository.GetAllAsync();
        }

        public async Task<ServiceJob> GetServiceJobByIdAsync(Guid id)
        {
            return await _serviceJobRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceJob>> GetServiceJobsByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _serviceJobRepository.GetByRepairOrderIdAsync(repairOrderId);
        }

        public async Task<IEnumerable<ServiceJob>> GetServiceJobsByTechnicianIdAsync(Guid technicianId)
        {
            return await _serviceJobRepository.GetByTechnicianIdAsync(technicianId);
        }

        public async Task<ServiceJob> CreateServiceJobAsync(ServiceJob serviceJob)
        {
            if (serviceJob == null)
                throw new ArgumentNullException(nameof(serviceJob));
            
            // Set default values
            serviceJob.Status = JobStatus.NotStarted;
            serviceJob.CreatedAt = DateTime.UtcNow;
            serviceJob.UpdatedAt = DateTime.UtcNow;
            
            // Save the job
            var createdJob = await _serviceJobRepository.CreateAsync(serviceJob);
            
            // Update repair order totals
            await _repairOrderService.CalculateTotalAmountAsync(serviceJob.RepairOrderId);
            
            return createdJob;
        }

        public async Task<ServiceJob> UpdateServiceJobAsync(ServiceJob serviceJob)
        {
            if (serviceJob == null)
                throw new ArgumentNullException(nameof(serviceJob));
            
            var existingJob = await _serviceJobRepository.GetByIdAsync(serviceJob.Id);
            if (existingJob == null)
                throw new KeyNotFoundException($"Service Job with ID {serviceJob.Id} not found");
            
            serviceJob.UpdatedAt = DateTime.UtcNow;
            var updatedJob = await _serviceJobRepository.UpdateAsync(serviceJob);
            
            // Update repair order totals
            await _repairOrderService.CalculateTotalAmountAsync(serviceJob.RepairOrderId);
            
            return updatedJob;
        }

        public async Task<ServiceJob> AssignTechnicianAsync(Guid id, Guid technicianId)
        {
            var job = await _serviceJobRepository.GetByIdAsync(id);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {id} not found");
            
            job.TechnicianId = technicianId;
            job.UpdatedAt = DateTime.UtcNow;
            
            return await _serviceJobRepository.UpdateAsync(job);
        }

        public async Task<ServiceJob> StartJobAsync(Guid id)
        {
            var job = await _serviceJobRepository.GetByIdAsync(id);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {id} not found");
            
            if (!job.TechnicianId.HasValue)
                throw new InvalidOperationException("Cannot start job without assigning a technician");
                
            if (!job.CustomerAuthorized)
                throw new InvalidOperationException("Cannot start job without customer authorization");
            
            job.Status = JobStatus.InProgress;
            job.StartTime = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            return await _serviceJobRepository.UpdateAsync(job);
        }

        public async Task<ServiceJob> CompleteJobAsync(Guid id)
        {
            var job = await _serviceJobRepository.GetByIdAsync(id);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {id} not found");
            
            if (job.Status != JobStatus.InProgress)
                throw new InvalidOperationException("Cannot complete a job that is not in progress");
            
            job.Status = JobStatus.Completed;
            job.EndTime = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            // Calculate actual hours
            if (job.StartTime.HasValue && job.EndTime.HasValue)
            {
                var duration = job.EndTime.Value - job.StartTime.Value;
                job.ActualHours = (decimal)duration.TotalHours;
            }
            
            var completedJob = await _serviceJobRepository.UpdateAsync(job);
            
            // Update repair order totals
            await _repairOrderService.CalculateTotalAmountAsync(job.RepairOrderId);
            
            return completedJob;
        }

        public async Task<ServiceJob> AddPartToJobAsync(Guid jobId, ServicePart part)
        {
            var job = await _serviceJobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {jobId} not found");
            
            part.ServiceJobId = jobId;
            part.CreatedAt = DateTime.UtcNow;
            part.UpdatedAt = DateTime.UtcNow;
            
            // Add the part to the job's parts collection
            job.Parts.Add(part);
            
            // Recalculate parts charge
            decimal partsTotal = 0;
            foreach (var p in job.Parts)
            {
                partsTotal += p.TotalAmount;
            }
            job.PartsCharge = partsTotal;
            job.UpdatedAt = DateTime.UtcNow;
            
            var updatedJob = await _serviceJobRepository.UpdateAsync(job);
            
            // Update repair order totals
            await _repairOrderService.CalculateTotalAmountAsync(job.RepairOrderId);
            
            return updatedJob;
        }

        public async Task<ServiceJob> AddPartsAsync(Guid jobId, List<ServicePart> parts)
        {
            var job = await _serviceJobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {jobId} not found");
            
            foreach (var part in parts)
            {
                part.ServiceJobId = jobId;
                part.CreatedAt = DateTime.UtcNow;
                job.Parts.Add(part);
            }
            
            job.UpdatedAt = DateTime.UtcNow;
            await _serviceJobRepository.UpdateAsync(job);
            return job;
        }

        public async Task<bool> AuthorizeJobAsync(Guid id, Guid authorizedById)
        {
            var job = await _serviceJobRepository.GetByIdAsync(id);
            if (job == null)
                throw new KeyNotFoundException($"Service Job with ID {id} not found");
            
            job.CustomerAuthorized = true;
            job.AuthorizationTime = DateTime.UtcNow;
            job.AuthorizedById = authorizedById;
            job.UpdatedAt = DateTime.UtcNow;
            
            await _serviceJobRepository.UpdateAsync(job);
            return true;
        }
    }
}
