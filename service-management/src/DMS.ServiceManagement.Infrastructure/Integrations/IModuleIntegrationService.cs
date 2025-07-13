using System;
using System.Threading.Tasks;

namespace DMS.ServiceManagement.Infrastructure.Integrations
{
    public interface IModuleIntegrationService
    {
        Task<T> GetCustomerInfoAsync<T>(Guid customerId);
        Task<T> GetVehicleInfoAsync<T>(Guid vehicleId);
        Task<T> GetPartsInfoAsync<T>(string partNumber);
        Task<T> CheckPartsInventoryAsync<T>(string[] partNumbers);
        Task<T> CreateInvoiceAsync<T>(object invoiceData);
        Task<T> GetAvailableTechniciansAsync<T>(DateTime startTime, DateTime endTime, string specialization = null);
        Task<bool> NotifyCustomerAsync(Guid customerId, string messageType, object payload);
    }
}
