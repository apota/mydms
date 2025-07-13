using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Repositories;
using DMS.FinancialManagement.Infrastructure.Data;
using DMS.Shared.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.FinancialManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Journal Entry entities
    /// </summary>
    public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
    {
        private readonly FinancialDbContext _context;

        public JournalEntryRepository(FinancialDbContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .Where(je => je.EntryDate >= startDate && je.EntryDate <= endDate)
                .OrderByDescending(je => je.EntryDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntry>> GetByFinancialPeriodIdAsync(Guid financialPeriodId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .Where(je => je.FinancialPeriodId == financialPeriodId)
                .OrderByDescending(je => je.EntryDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntry>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .Where(je => je.Reference.Contains(reference))
                .OrderByDescending(je => je.EntryDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .FirstOrDefaultAsync(je => je.EntryNumber == entryNumber, cancellationToken);
        }

        /// <inheritdoc />
        public override async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .FirstOrDefaultAsync(je => je.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<JournalEntry?> PostJournalEntryAsync(Guid id, DateTime postingDate, string postedBy, CancellationToken cancellationToken = default)
        {
            var entry = await GetByIdAsync(id, cancellationToken);
            
            if (entry == null)
            {
                return null;
            }
            
            // Cannot post an already posted entry
            if (entry.IsPosted)
            {
                return null;
            }
            
            entry.IsPosted = true;
            entry.PostingDate = postingDate;
            entry.UpdatedBy = postedBy;
            entry.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return entry;
        }

        /// <inheritdoc />
        public async Task<JournalEntry?> ReverseJournalEntryAsync(Guid id, DateTime reversalDate, string description, string reversedBy, CancellationToken cancellationToken = default)
        {
            var originalEntry = await GetByIdAsync(id, cancellationToken);
            
            if (originalEntry == null)
            {
                return null;
            }
            
            // Cannot reverse an unposted entry
            if (!originalEntry.IsPosted)
            {
                return null;
            }
            
            // Create reversal entry (with opposite debits and credits)
            var reversalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryDate = reversalDate,
                Description = $"Reversal of {originalEntry.EntryNumber}: {description}",
                Reference = originalEntry.Reference,
                IsPosted = false,
                IsRecurring = false,
                FinancialPeriodId = originalEntry.FinancialPeriodId,
                CreatedBy = reversedBy,
                CreatedAt = DateTime.UtcNow
            };
            
            // Generate a new entry number (will be handled by the service layer)
            reversalEntry.EntryNumber = $"R-{originalEntry.EntryNumber}";
            
            // Reverse line items (swap debits and credits)
            foreach (var originalLine in originalEntry.LineItems)
            {
                var reversalLine = new JournalLineItem
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = reversalEntry.Id,
                    AccountId = originalLine.AccountId,
                    Description = $"Reversal of {originalLine.Description}",
                    DebitAmount = originalLine.CreditAmount,
                    CreditAmount = originalLine.DebitAmount,
                    DepartmentId = originalLine.DepartmentId,
                    CostCenterId = originalLine.CostCenterId,
                    CreatedBy = reversedBy,
                    CreatedAt = DateTime.UtcNow
                };
                
                reversalEntry.LineItems.Add(reversalLine);
            }
            
            await _context.JournalEntries.AddAsync(reversalEntry, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return reversalEntry;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<JournalEntry>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(je => je.LineItems)
                    .ThenInclude(li => li.Account)
                .Include(je => je.FinancialPeriod)
                .OrderByDescending(je => je.EntryDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}
