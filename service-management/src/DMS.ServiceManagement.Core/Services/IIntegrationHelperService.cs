using System;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IIntegrationHelperService
    {
        Task<CustomerInfo> GetCustomerDetailsAsync(Guid customerId);
        Task<VehicleInfo> GetVehicleDetailsAsync(Guid vehicleId);
        Task<PartAvailability> CheckPartAvailabilityAsync(string partNumber, int quantity);
        Task<InvoiceResult> GenerateInvoiceFromRepairOrder(RepairOrder repairOrder);
        Task<bool> SendAppointmentConfirmation(ServiceAppointment appointment, CustomerInfo customer);
        Task<bool> SendServiceComplete(RepairOrder repairOrder, CustomerInfo customer);
        Task<bool> SendInspectionResults(ServiceInspection inspection, Guid customerId);
    }
}
