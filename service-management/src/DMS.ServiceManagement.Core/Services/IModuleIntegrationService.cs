using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IModuleIntegrationService
    {
        Task<CustomerInfo> GetCustomerAsync(Guid customerId);
        Task<VehicleInfo> GetVehicleAsync(Guid vehicleId);
        Task<bool> CheckPartAvailabilityAsync(Guid partId, int quantity);
        Task<decimal> GetPartPriceAsync(Guid partId);
        Task<bool> CreateInvoiceAsync(RepairOrder repairOrder);
        Task<bool> SendNotificationAsync(Guid customerId, string messageType, object data);
        Task<List<object>> GetInventoryPartsAsync(List<Guid> partIds);
        Task<bool> UpdateInventoryAsync(Guid partId, int quantityUsed);
        Task<object> GetCustomerPreferencesAsync(Guid customerId);
        Task<bool> LogServiceHistoryAsync(Guid customerId, Guid vehicleId, ServiceJob serviceJob);
    }
}
