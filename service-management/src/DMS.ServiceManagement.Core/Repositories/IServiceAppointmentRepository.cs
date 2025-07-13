using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Repositories
{
    public interface IServiceAppointmentRepository
    {
        Task<IEnumerable<ServiceAppointment>> GetAllAsync();
        Task<ServiceAppointment> GetByIdAsync(Guid id);
        Task<IEnumerable<ServiceAppointment>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<ServiceAppointment>> GetByVehicleIdAsync(Guid vehicleId);
        Task<IEnumerable<ServiceAppointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ServiceAppointment>> GetByStatusAsync(AppointmentStatus status);
        Task<IEnumerable<ServiceAppointment>> GetByAdvisorIdAsync(Guid advisorId);
        Task<ServiceAppointment> CreateAsync(ServiceAppointment appointment);
        Task<ServiceAppointment> UpdateAsync(ServiceAppointment appointment);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<ServiceAppointment>> GetAvailableSlotsAsync(DateTime date, int duration);
    }
}
