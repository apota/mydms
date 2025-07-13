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
    public class PartTransactionRepository : IPartTransactionRepository
    {
        private readonly PartsDbContext _context;

        public PartTransactionRepository(PartsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PartTransaction>> GetAllAsync()
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PartTransaction> GetByIdAsync(int id)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<PartTransaction>> GetByPartIdAsync(int partId)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .Where(t => t.PartId == partId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartTransaction>> GetByLocationIdAsync(int locationId)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .Where(t => t.LocationId == locationId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartTransaction>> GetByTransactionTypeAsync(TransactionType type)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .Where(t => t.Type == type)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PartTransaction>> GetByReferenceNumberAsync(string referenceNumber)
        {
            return await _context.PartTransactions
                .Include(t => t.Part)
                .Include(t => t.Location)
                .Where(t => t.ReferenceNumber == referenceNumber)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> AddAsync(PartTransaction transaction)
        {
            // Set transaction date if not already set
            if (transaction.TransactionDate == default)
            {
                transaction.TransactionDate = DateTime.UtcNow;
            }
            
            await _context.PartTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            
            // Update inventory quantities
            var inventory = await _context.PartInventories
                .FirstOrDefaultAsync(i => i.PartId == transaction.PartId && i.LocationId == transaction.LocationId);
                
            if (inventory == null)
            {
                // Create new inventory record if it doesn't exist
                inventory = new PartInventory
                {
                    PartId = transaction.PartId,
                    LocationId = transaction.LocationId,
                    QuantityOnHand = transaction.Type == TransactionType.Receipt ? transaction.Quantity : -transaction.Quantity,
                    ReorderPoint = 5, // Default reorder point
                    ReorderQuantity = 10 // Default reorder quantity
                };
                await _context.PartInventories.AddAsync(inventory);
            }
            else
            {
                // Update existing inventory
                if (transaction.Type == TransactionType.Receipt || 
                    transaction.Type == TransactionType.Adjustment ||
                    transaction.Type == TransactionType.Return)
                {
                    inventory.QuantityOnHand += transaction.Quantity;
                }
                else
                {
                    inventory.QuantityOnHand -= transaction.Quantity;
                }
                _context.Entry(inventory).State = EntityState.Modified;
            }
            
            await _context.SaveChangesAsync();
            return transaction.Id;
        }

        public async Task UpdateAsync(PartTransaction transaction)
        {
            // Get the original transaction to determine inventory adjustment
            var originalTransaction = await _context.PartTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);
                
            if (originalTransaction != null)
            {
                // Calculate the difference in quantity
                int quantityDifference = transaction.Quantity - originalTransaction.Quantity;
                
                if (quantityDifference != 0)
                {
                    // Update inventory based on the difference
                    var inventory = await _context.PartInventories
                        .FirstOrDefaultAsync(i => i.PartId == transaction.PartId && i.LocationId == transaction.LocationId);
                        
                    if (inventory != null)
                    {
                        if (transaction.Type == TransactionType.Receipt || 
                            transaction.Type == TransactionType.Adjustment ||
                            transaction.Type == TransactionType.Return)
                        {
                            inventory.QuantityOnHand += quantityDifference;
                        }
                        else
                        {
                            inventory.QuantityOnHand -= quantityDifference;
                        }
                        _context.Entry(inventory).State = EntityState.Modified;
                    }
                }
            }
            
            _context.Entry(transaction).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _context.PartTransactions.FindAsync(id);
            if (transaction != null)
            {
                // Revert the inventory change
                var inventory = await _context.PartInventories
                    .FirstOrDefaultAsync(i => i.PartId == transaction.PartId && i.LocationId == transaction.LocationId);
                    
                if (inventory != null)
                {
                    if (transaction.Type == TransactionType.Receipt || 
                        transaction.Type == TransactionType.Adjustment ||
                        transaction.Type == TransactionType.Return)
                    {
                        inventory.QuantityOnHand -= transaction.Quantity;
                    }
                    else
                    {
                        inventory.QuantityOnHand += transaction.Quantity;
                    }
                    _context.Entry(inventory).State = EntityState.Modified;
                }
                
                _context.PartTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
