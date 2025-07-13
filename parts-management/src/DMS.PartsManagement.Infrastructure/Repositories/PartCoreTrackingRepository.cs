using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Infrastructure.Data;

namespace DMS.PartsManagement.Infrastructure.Repositories
{
    public class PartCoreTrackingRepository : IPartCoreTrackingRepository
    {
        private readonly PartsDbContext _context;

        public PartCoreTrackingRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PartCoreTracking>> GetAllAsync()
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PartCoreTracking> GetByIdAsync(int id)
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<PartCoreTracking>> GetByPartIdAsync(int partId)
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .Where(c => c.PartId == partId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartCoreTracking>> GetByCustomerInfoAsync(string customerInfo)
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .Where(c => c.CustomerInfo.Contains(customerInfo))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartCoreTracking>> GetByStatusAsync(CoreTrackingStatus status)
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .Where(c => c.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartCoreTracking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PartCoreTrackings
                .Include(c => c.Part)
                .Where(c => c.ReceivedDate >= startDate && c.ReceivedDate <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> AddAsync(PartCoreTracking coreTracking)
        {
            // Set received date if not already set
            if (coreTracking.ReceivedDate == default)
            {
                coreTracking.ReceivedDate = DateTime.UtcNow;
            }
            
            await _context.PartCoreTrackings.AddAsync(coreTracking);
            await _context.SaveChangesAsync();
            return coreTracking.Id;
        }

        public async Task UpdateAsync(PartCoreTracking coreTracking)
        {
            _context.Entry(coreTracking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, CoreTrackingStatus status)
        {
            var coreTracking = await _context.PartCoreTrackings.FindAsync(id);
            if (coreTracking != null)
            {
                coreTracking.Status = status;
                if (status == CoreTrackingStatus.ReturnedToSupplier)
                {
                    coreTracking.ReturnedDate = DateTime.UtcNow;
                }
                _context.Entry(coreTracking).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var coreTracking = await _context.PartCoreTrackings.FindAsync(id);
            if (coreTracking != null)
            {
                _context.PartCoreTrackings.Remove(coreTracking);
                await _context.SaveChangesAsync();
            }
        }
    }
}
