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
    public class PartInventoryRepository : IPartInventoryRepository
    {
        private readonly PartsDbContext _context;

        public PartInventoryRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PartInventory>> GetAllAsync()
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PartInventory> GetByIdAsync(int id)
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<PartInventory>> GetByPartIdAsync(int partId)
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .Where(i => i.PartId == partId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartInventory>> GetByLocationIdAsync(int locationId)
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .Where(i => i.LocationId == locationId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PartInventory> GetByPartAndLocationAsync(int partId, int locationId)
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.PartId == partId && i.LocationId == locationId);
        }

        public async Task<IEnumerable<PartInventory>> GetLowStockAsync(int threshold = 5)
        {
            return await _context.PartInventories
                .Include(i => i.Part)
                .Include(i => i.Location)
                .Where(i => i.QuantityOnHand <= threshold)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> AddAsync(PartInventory inventory)
        {
            await _context.PartInventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory.Id;
        }

        public async Task UpdateAsync(PartInventory inventory)
        {
            _context.Entry(inventory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int partId, int locationId, int quantityChange)
        {
            var inventory = await _context.PartInventories
                .FirstOrDefaultAsync(i => i.PartId == partId && i.LocationId == locationId);

            if (inventory != null)
            {
                inventory.QuantityOnHand += quantityChange;
                _context.Entry(inventory).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var inventory = await _context.PartInventories.FindAsync(id);
            if (inventory != null)
            {
                _context.PartInventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
