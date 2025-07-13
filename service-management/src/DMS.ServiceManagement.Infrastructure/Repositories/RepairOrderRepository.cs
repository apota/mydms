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
    public class RepairOrderRepository : IRepairOrderRepository
    {
        private readonly ServiceManagementDbContext _context;

        public RepairOrderRepository(ServiceManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RepairOrder>> GetAllAsync()
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .ToListAsync();
        }

        public async Task<RepairOrder> GetByIdAsync(Guid id)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RepairOrder> GetByNumberAsync(string number)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .FirstOrDefaultAsync(r => r.Number == number);
        }

        public async Task<IEnumerable<RepairOrder>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .Where(r => r.VehicleId == vehicleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .Where(r => r.OpenDate >= startDate && r.OpenDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetByStatusAsync(RepairOrderStatus status)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepairOrder>> GetByAdvisorIdAsync(Guid advisorId)
        {
            return await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .Include(r => r.Appointment)
                .Include(r => r.Inspections)
                .Where(r => r.AdvisorId == advisorId)
                .ToListAsync();
        }

        public async Task<RepairOrder> CreateAsync(RepairOrder repairOrder)
        {
            repairOrder.Id = repairOrder.Id == Guid.Empty ? Guid.NewGuid() : repairOrder.Id;
            _context.RepairOrders.Add(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }

        public async Task<RepairOrder> UpdateAsync(RepairOrder repairOrder)
        {
            _context.RepairOrders.Update(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(id);
            if (repairOrder == null)
                return false;

            _context.RepairOrders.Remove(repairOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RepairOrder> UpdateStatusAsync(Guid id, RepairOrderStatus status)
        {
            var repairOrder = await _context.RepairOrders.FindAsync(id);
            if (repairOrder == null)
                return null;

            repairOrder.Status = status;
            repairOrder.UpdatedAt = DateTime.UtcNow;

            _context.RepairOrders.Update(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }

        public async Task<RepairOrder> CloseRepairOrderAsync(Guid id)
        {
            var repairOrder = await _context.RepairOrders
                .Include(r => r.ServiceJobs)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (repairOrder == null)
                return null;

            // Ensure all service jobs are completed before closing
            if (repairOrder.ServiceJobs.Any(j => j.Status != JobStatus.Completed))
                throw new InvalidOperationException("Cannot close repair order with incomplete service jobs");

            repairOrder.Status = RepairOrderStatus.Closed;
            repairOrder.UpdatedAt = DateTime.UtcNow;
            repairOrder.CompletionDate = DateTime.UtcNow;

            _context.RepairOrders.Update(repairOrder);
            await _context.SaveChangesAsync();
            return repairOrder;
        }
    }
}
