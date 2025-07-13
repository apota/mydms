using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<ServiceAppointment>> GetAllAppointmentsAsync();
        Task<ServiceAppointment> GetAppointmentByIdAsync(Guid id);
        Task<IEnumerable<ServiceAppointment>> GetAppointmentsByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<ServiceAppointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ServiceAppointment>> GetAvailableSlotsAsync(DateTime date, int duration);
        Task<ServiceAppointment> CreateAppointmentAsync(ServiceAppointment appointment);
        Task<ServiceAppointment> UpdateAppointmentAsync(ServiceAppointment appointment);
        Task<bool> CancelAppointmentAsync(Guid id);
        Task<bool> ConfirmAppointmentAsync(Guid id);
        Task<ServiceAppointment> CheckInAppointmentAsync(Guid id, Guid advisorId);
    }
}
