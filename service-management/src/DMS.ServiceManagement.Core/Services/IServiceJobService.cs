using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IServiceJobService
    {
        Task<IEnumerable<ServiceJob>> GetAllServiceJobsAsync();
        Task<ServiceJob> GetServiceJobByIdAsync(Guid id);
        Task<IEnumerable<ServiceJob>> GetServiceJobsByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<ServiceJob>> GetServiceJobsByTechnicianIdAsync(Guid technicianId);
        Task<ServiceJob> CreateServiceJobAsync(ServiceJob serviceJob);
        Task<ServiceJob> UpdateServiceJobAsync(ServiceJob serviceJob);
        Task<ServiceJob> AssignTechnicianAsync(Guid id, Guid technicianId);
        Task<ServiceJob> StartJobAsync(Guid id);
        Task<ServiceJob> CompleteJobAsync(Guid id);
        Task<ServiceJob> AddPartToJobAsync(Guid jobId, ServicePart part);
        Task<ServiceJob> AddPartsAsync(Guid jobId, List<ServicePart> parts);
        Task<bool> AuthorizeJobAsync(Guid id, Guid authorizedById);
    }
}
