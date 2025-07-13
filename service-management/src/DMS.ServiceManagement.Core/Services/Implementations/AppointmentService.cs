using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Core.Services;

namespace DMS.ServiceManagement.Core.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IServiceAppointmentRepository _appointmentRepository;

        public AppointmentService(IServiceAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAllAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAsync();
        }

        public async Task<ServiceAppointment> GetAppointmentByIdAsync(Guid id)
        {
            return await _appointmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAppointmentsByCustomerIdAsync(Guid customerId)
        {
            return await _appointmentRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _appointmentRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAvailableSlotsAsync(DateTime date, int duration)
        {
            return await _appointmentRepository.GetAvailableSlotsAsync(date, duration);
        }

        public async Task<ServiceAppointment> CreateAppointmentAsync(ServiceAppointment appointment)
        {
            // Validate the appointment
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));
                
            if (appointment.ScheduledStartTime >= appointment.ScheduledEndTime)
                throw new ArgumentException("Start time must be before end time");
                
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.ConfirmationStatus = ConfirmationStatus.Pending;
            
            return await _appointmentRepository.CreateAsync(appointment);
        }

        public async Task<ServiceAppointment> UpdateAppointmentAsync(ServiceAppointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));
                
            var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.Id);
            if (existingAppointment == null)
                throw new KeyNotFoundException($"Appointment with ID {appointment.Id} not found");
                
            appointment.UpdatedAt = DateTime.UtcNow;
            
            return await _appointmentRepository.UpdateAsync(appointment);
        }

        public async Task<bool> CancelAppointmentAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with ID {id} not found");
                
            appointment.Status = AppointmentStatus.Canceled;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> ConfirmAppointmentAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with ID {id} not found");
                
            appointment.ConfirmationStatus = ConfirmationStatus.Confirmed;
            appointment.ConfirmationTime = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        public async Task<ServiceAppointment> CheckInAppointmentAsync(Guid id, Guid advisorId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with ID {id} not found");
                
            appointment.Status = AppointmentStatus.InProgress;
            appointment.AdvisorId = advisorId;
            appointment.ActualStartTime = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            return await _appointmentRepository.UpdateAsync(appointment);
        }
    }
}
