using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.ServiceManagement.Infrastructure.Repositories
{
    public class ServiceJobRepository : IServiceJobRepository
    {
        private readonly ServiceManagementDbContext _context;

        public ServiceJobRepository(ServiceManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceJob>> GetAllAsync()
        {
            return await _context.ServiceJobs
                .Include(j => j.Parts)
                .Include(j => j.InspectionResults)
                .Include(j => j.RepairOrder)
                .ToListAsync();
        }

        public async Task<ServiceJob> GetByIdAsync(Guid id)
        {
            return await _context.ServiceJobs
                .Include(j => j.Parts)
                .Include(j => j.InspectionResults)
                .Include(j => j.RepairOrder)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<ServiceJob>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _context.ServiceJobs
                .Include(j => j.Parts)
                .Include(j => j.InspectionResults)
                .Where(j => j.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceJob>> GetByTechnicianIdAsync(Guid technicianId)
        {
            return await _context.ServiceJobs
                .Include(j => j.Parts)
                .Include(j => j.InspectionResults)
                .Include(j => j.RepairOrder)
                .Where(j => j.TechnicianId == technicianId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceJob>> GetByStatusAsync(JobStatus status)
        {
            return await _context.ServiceJobs
                .Include(j => j.Parts)
                .Include(j => j.InspectionResults)
                .Include(j => j.RepairOrder)
                .Where(j => j.Status == status)
                .ToListAsync();
        }

        public async Task<ServiceJob> CreateAsync(ServiceJob job)
        {
            job.Id = job.Id == Guid.Empty ? Guid.NewGuid() : job.Id;
            job.CreatedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            _context.ServiceJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ServiceJob> UpdateAsync(ServiceJob job)
        {
            job.UpdatedAt = DateTime.UtcNow;
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var job = await _context.ServiceJobs.FindAsync(id);
            if (job == null)
                return false;

            _context.ServiceJobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceJob> AssignTechnicianAsync(Guid id, Guid technicianId)
        {
            var job = await _context.ServiceJobs.FindAsync(id);
            if (job == null)
                return null;

            job.TechnicianId = technicianId;
            job.UpdatedAt = DateTime.UtcNow;
            
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ServiceJob> StartJobAsync(Guid id)
        {
            var job = await _context.ServiceJobs.FindAsync(id);
            if (job == null)
                return null;

            job.Status = JobStatus.InProgress;
            job.StartTime = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ServiceJob> CompleteJobAsync(Guid id)
        {
            var job = await _context.ServiceJobs.FindAsync(id);
            if (job == null)
                return null;

            job.Status = JobStatus.Completed;
            job.EndTime = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;
            
            // Calculate actual hours
            if (job.StartTime.HasValue && job.EndTime.HasValue)
            {
                job.ActualHours = (decimal)(job.EndTime.Value - job.StartTime.Value).TotalHours;
            }
            
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ServiceJob> AddPartsAsync(Guid jobId, IEnumerable<ServicePart> parts)
        {
            var job = await _context.ServiceJobs
                .Include(j => j.Parts)
                .FirstOrDefaultAsync(j => j.Id == jobId);
                
            if (job == null)
                return null;

            foreach (var part in parts)
            {
                part.Id = part.Id == Guid.Empty ? Guid.NewGuid() : part.Id;
                part.ServiceJobId = jobId;
                part.RequestTime = DateTime.UtcNow;
                part.CreatedAt = DateTime.UtcNow;
                part.UpdatedAt = DateTime.UtcNow;
                
                job.Parts.Add(part);
            }
            
            job.UpdatedAt = DateTime.UtcNow;
            
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ServiceJob> UpdateStatusAsync(Guid id, JobStatus status)
        {
            var job = await _context.ServiceJobs.FindAsync(id);
            if (job == null)
                return null;

            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;
            
            _context.ServiceJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }
    }
}
