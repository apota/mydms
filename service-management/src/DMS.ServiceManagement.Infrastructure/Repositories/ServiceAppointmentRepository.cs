using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.ServiceManagement.Infrastructure.Repositories
{
    public class ServiceAppointmentRepository : IServiceAppointmentRepository
    {
        private readonly ServiceManagementDbContext _context;

        public ServiceAppointmentRepository(ServiceManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAllAsync()
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .ToListAsync();
        }

        public async Task<ServiceAppointment> GetByIdAsync(Guid id)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<ServiceAppointment>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceAppointment>> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .Where(a => a.VehicleId == vehicleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceAppointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .Where(a => a.ScheduledStartTime >= startDate && a.ScheduledStartTime <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceAppointment>> GetByStatusAsync(AppointmentStatus status)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceAppointment>> GetByAdvisorIdAsync(Guid advisorId)
        {
            return await _context.ServiceAppointments
                .Include(a => a.Bay)
                .Where(a => a.AdvisorId == advisorId)
                .ToListAsync();
        }

        public async Task<ServiceAppointment> CreateAsync(ServiceAppointment appointment)
        {
            appointment.Id = appointment.Id == Guid.Empty ? Guid.NewGuid() : appointment.Id;
            _context.ServiceAppointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<ServiceAppointment> UpdateAsync(ServiceAppointment appointment)
        {
            _context.ServiceAppointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _context.ServiceAppointments.FindAsync(id);
            if (appointment == null)
                return false;

            _context.ServiceAppointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ServiceAppointment>> GetAvailableSlotsAsync(DateTime date, int duration)
        {
            // This is a simplified implementation that returns all bays that are available
            // on the requested date for the requested duration
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);
            
            // Get all appointments for the day
            var existingAppointments = await _context.ServiceAppointments
                .Where(a => a.ScheduledEndTime >= startOfDay && a.ScheduledStartTime <= endOfDay)
                .ToListAsync();
                
            // Get all service bays
            var allBays = await _context.ServiceBays
                .Where(b => b.Status != BayStatus.OutOfService)
                .ToListAsync();
                
            // For each bay, find time slots where it's not already booked
            var availableSlots = new List<ServiceAppointment>();
            
            foreach (var bay in allBays)
            {
                // Calculate available time slots
                // This is a simplified algorithm - in a real app, you'd have more complex logic
                
                // Start with standard appointment slots (e.g. every hour)
                for (var slotStart = startOfDay.AddHours(8); // Start at 8 AM
                     slotStart < startOfDay.AddHours(17); // End at 5 PM
                     slotStart = slotStart.AddHours(1)) // 1 hour increments
                {
                    var slotEnd = slotStart.AddMinutes(duration);
                    
                    // Check if this slot conflicts with any existing appointments for this bay
                    bool isConflict = existingAppointments
                        .Where(a => a.BayId == bay.Id)
                        .Any(a => 
                            (slotStart >= a.ScheduledStartTime && slotStart < a.ScheduledEndTime) || // Slot start is within existing appointment
                            (slotEnd > a.ScheduledStartTime && slotEnd <= a.ScheduledEndTime) || // Slot end is within existing appointment
                            (slotStart <= a.ScheduledStartTime && slotEnd >= a.ScheduledEndTime)); // Slot completely contains existing appointment
                            
                    if (!isConflict)
                    {
                        // This is an available slot
                        availableSlots.Add(new ServiceAppointment
                        {
                            Id = Guid.NewGuid(), // This is just a placeholder ID
                            BayId = bay.Id,
                            Bay = bay,
                            ScheduledStartTime = slotStart,
                            ScheduledEndTime = slotEnd,
                            Status = AppointmentStatus.Scheduled // This is just for indication
                        });
                    }
                }
            }
            
            return availableSlots;
        }
    }
}
