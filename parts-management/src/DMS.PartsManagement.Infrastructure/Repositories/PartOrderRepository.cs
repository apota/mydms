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
    public class PartOrderRepository : IPartOrderRepository
    {
        private readonly PartsDbContext _context;

        public PartOrderRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PartOrder>> GetAllAsync()
        {
            return await _context.PartOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Part)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PartOrder> GetByIdAsync(int id)
        {
            return await _context.PartOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Part)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<PartOrder>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.PartOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Part)
                .Where(o => o.SupplierId == supplierId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartOrder>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.PartOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Part)
                .Where(o => o.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PartOrders
                .Include(o => o.Supplier)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Part)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> AddAsync(PartOrder order)
        {
            await _context.PartOrders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }

        public async Task UpdateAsync(PartOrder order)
        {
            _context.Entry(order).State = EntityState.Modified;
            
            // Update order lines
            foreach (var orderLine in order.OrderLines)
            {
                if (orderLine.Id == 0)
                {
                    _context.PartOrderLines.Add(orderLine);
                }
                else
                {
                    _context.Entry(orderLine).State = EntityState.Modified;
                }
            }
            
            // Handle deleted order lines
            var existingOrderLines = await _context.PartOrderLines
                .Where(ol => ol.OrderId == order.Id)
                .ToListAsync();
                
            foreach (var existingLine in existingOrderLines)
            {
                if (!order.OrderLines.Any(ol => ol.Id == existingLine.Id))
                {
                    _context.PartOrderLines.Remove(existingLine);
                }
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.PartOrders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.PartOrders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order != null)
            {
                _context.PartOrderLines.RemoveRange(order.OrderLines);
                _context.PartOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}
