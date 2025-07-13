using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Repositories
{
    public interface IServiceJobRepository
    {
        Task<IEnumerable<ServiceJob>> GetAllAsync();
        Task<ServiceJob> GetByIdAsync(Guid id);
        Task<IEnumerable<ServiceJob>> GetByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<ServiceJob>> GetByStatusAsync(JobStatus status);
        Task<IEnumerable<ServiceJob>> GetByTechnicianIdAsync(Guid technicianId);
        Task<ServiceJob> CreateAsync(ServiceJob serviceJob);
        Task<ServiceJob> UpdateAsync(ServiceJob serviceJob);
        Task<bool> DeleteAsync(Guid id);
        Task<ServiceJob> AssignTechnicianAsync(Guid id, Guid technicianId);
        Task<ServiceJob> StartJobAsync(Guid id);
        Task<ServiceJob> CompleteJobAsync(Guid id);
    }
}
