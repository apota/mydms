using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Infrastructure.Data;

namespace DMS.PartsManagement.Infrastructure.Repositories
{
    public class PartRepository : IPartRepository
    {
        private readonly PartsDbContext _context;

        public PartRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Part>> GetAllAsync()
        {
            return await _context.Parts
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Pricing)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Part> GetByIdAsync(int id)
        {
            return await _context.Parts
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Pricing)
                .Include(p => p.SupersededByPart)
                .Include(p => p.SupersedesPart)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Part> GetByPartNumberAsync(string partNumber)
        {
            return await _context.Parts
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Pricing)
                .Include(p => p.SupersededByPart)
                .Include(p => p.SupersedesPart)
                .FirstOrDefaultAsync(p => p.PartNumber == partNumber);
        }

        public async Task<IEnumerable<Part>> SearchAsync(string searchTerm)
        {
            return await _context.Parts
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Where(p => p.PartNumber.Contains(searchTerm) || 
                            p.Description.Contains(searchTerm) || 
                            p.Manufacturer.Name.Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Part>> GetByFitmentAsync(int year, string make, string model, string subModel = null)
        {
            var query = _context.Parts
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Pricing)
                .Where(p => p.FitmentInfo != null && 
                            p.FitmentInfo.Contains(year.ToString()) &&
                            p.FitmentInfo.Contains(make) &&
                            p.FitmentInfo.Contains(model));

            if (!string.IsNullOrWhiteSpace(subModel))
            {
                query = query.Where(p => p.FitmentInfo.Contains(subModel));
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Part>> GetSupersessionChainAsync(string partNumber)
        {
            var result = new List<Part>();
            var currentPart = await _context.Parts
                .Include(p => p.SupersededByPart)
                .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

            if (currentPart == null)
                return result;

            result.Add(currentPart);

            // Navigate up the supersession chain
            var supersededBy = currentPart.SupersededByPart;
            while (supersededBy != null)
            {
                result.Add(supersededBy);
                supersededBy = await _context.Parts
                    .Include(p => p.SupersededByPart)
                    .FirstOrDefaultAsync(p => p.Id == supersededBy.Id);

                supersededBy = supersededBy?.SupersededByPart;
            }

            return result;
        }

        public async Task<IEnumerable<Part>> GetCrossSellPartsAsync(int partId)
        {
            return await _context.Parts
                .Include(p => p.Pricing)
                .Include(p => p.CrossSellParts)
                .SelectMany(p => p.CrossSellParts)
                .Where(p => p.Id != partId)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> AddAsync(Part part)
        {
            await _context.Parts.AddAsync(part);
            await _context.SaveChangesAsync();
            return part.Id;
        }

        public async Task UpdateAsync(Part part)
        {
            _context.Entry(part).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part != null)
            {
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Part>> GetPartsBySupplierIdAsync(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            // The relationship between parts and suppliers is typically through orders
            // or a direct many-to-many relationship
            
            // Method 1: Get parts that have been ordered from this supplier
            var partsFromOrders = await _context.PartOrders
                .Where(o => o.SupplierId == supplierId)
                .SelectMany(o => o.OrderLines.Select(ol => ol.Part))
                .Distinct()
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return partsFromOrders;
            
            // Method 2: If there's a direct relationship between parts and suppliers:
            // return await _context.Parts
            //     .Where(p => p.SupplierId == supplierId || p.PreferredSupplierId == supplierId)
            //     .Skip(skip)
            //     .Take(take)
            //     .ToListAsync(cancellationToken);
        }
    }
}
