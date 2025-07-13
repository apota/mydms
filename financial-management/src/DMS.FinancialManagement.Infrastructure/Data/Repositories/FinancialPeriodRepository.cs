using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Repositories;
using DMS.Shared.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.FinancialManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Financial Period entities
    /// </summary>
    public class FinancialPeriodRepository : Repository<FinancialPeriod>, IFinancialPeriodRepository
    {
        private readonly FinancialDbContext _context;

        public FinancialPeriodRepository(FinancialDbContext context) : base(context)
        {
            _context = context;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<FinancialPeriod>> GetByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(fp => fp.FiscalYear == fiscalYear)
                .OrderBy(fp => fp.PeriodNumber)
                .ToListAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriod?> GetCurrentPeriodAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var checkDate = date ?? DateTime.Today;
            
            return await _context.FinancialPeriods
                .Where(fp => fp.StartDate <= checkDate && fp.EndDate >= checkDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriod?> GetByYearAndPeriodAsync(int fiscalYear, int periodNumber, CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(fp => fp.FiscalYear == fiscalYear && fp.PeriodNumber == periodNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriod?> ClosePeriodAsync(Guid id, string closedBy, DateTime closedDate, CancellationToken cancellationToken = default)
        {
            var period = await GetByIdAsync(id, cancellationToken);
            
            if (period == null)
            {
                return null;
            }
            
            // Can't close an already closed period
            if (period.IsClosed)
            {
                throw new InvalidOperationException($"Financial period {period.FiscalYear}-{period.PeriodNumber} is already closed");
            }
            
            period.IsClosed = true;
            period.ClosedDate = closedDate;
            period.ClosedBy = closedBy;
            period.UpdatedAt = DateTime.UtcNow;
            period.UpdatedBy = closedBy;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return period;
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriod?> ReopenPeriodAsync(Guid id, string reopenedBy, CancellationToken cancellationToken = default)
        {
            var period = await GetByIdAsync(id, cancellationToken);
            
            if (period == null)
            {
                return null;
            }
            
            // Can't reopen an already open period
            if (!period.IsClosed)
            {
                throw new InvalidOperationException($"Financial period {period.FiscalYear}-{period.PeriodNumber} is not closed");
            }
            
            period.IsClosed = false;
            period.ClosedDate = null;
            period.ClosedBy = null;
            period.UpdatedAt = DateTime.UtcNow;
            period.UpdatedBy = reopenedBy;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return period;
        }
    }
}
