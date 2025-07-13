using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IRepairOrderService
    {
        Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync();
        Task<RepairOrder> GetRepairOrderByIdAsync(Guid id);
        Task<RepairOrder> GetRepairOrderByNumberAsync(string number);
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<RepairOrder>> GetRepairOrdersByVehicleIdAsync(Guid vehicleId);
        Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder);
        Task<RepairOrder> UpdateRepairOrderStatusAsync(Guid id, RepairOrderStatus status);
        Task<RepairOrder> AddServiceJobAsync(Guid repairOrderId, ServiceJob serviceJob);
        Task<RepairOrder> CloseRepairOrderAsync(Guid id);
        Task<decimal> CalculateTotalAmountAsync(Guid id);
    }
}
