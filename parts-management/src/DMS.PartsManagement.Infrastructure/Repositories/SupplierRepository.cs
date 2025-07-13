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
    public class SupplierRepository : ISupplierRepository
    {
        private readonly PartsDbContext _context;

        public SupplierRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Supplier>> GetByNameAsync(string name, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Name.Contains(name))
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Supplier>> GetByTypeAsync(SupplierType type, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Type == type)
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Supplier>> GetActiveAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return supplier;
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return supplier;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);
            if (supplier == null)
            {
                return false;
            }
            
            // Check if supplier has associated orders
            bool hasOrders = await _context.PartOrders.AnyAsync(o => o.SupplierId == id, cancellationToken);
            if (hasOrders)
            {
                // Soft delete instead of hard delete
                supplier.IsActive = false;
                _context.Entry(supplier).State = EntityState.Modified;
            }
            else
            {
                _context.Suppliers.Remove(supplier);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        
        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.CountAsync(cancellationToken);
        }
        
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id, cancellationToken);
        }
    }
}
