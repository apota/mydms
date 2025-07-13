using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Repositories
{
    public interface IRepairOrderRepository
    {
        Task<IEnumerable<RepairOrder>> GetAllAsync();
        Task<RepairOrder> GetByIdAsync(Guid id);
        Task<RepairOrder> GetByNumberAsync(string number);
        Task<IEnumerable<RepairOrder>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<RepairOrder>> GetByVehicleIdAsync(Guid vehicleId);
        Task<IEnumerable<RepairOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<RepairOrder>> GetByStatusAsync(RepairOrderStatus status);
        Task<IEnumerable<RepairOrder>> GetByAdvisorIdAsync(Guid advisorId);
        Task<RepairOrder> CreateAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateAsync(RepairOrder repairOrder);
        Task<bool> DeleteAsync(Guid id);
        Task<RepairOrder> UpdateStatusAsync(Guid id, RepairOrderStatus status);
        Task<RepairOrder> CloseRepairOrderAsync(Guid id);
    }
}
