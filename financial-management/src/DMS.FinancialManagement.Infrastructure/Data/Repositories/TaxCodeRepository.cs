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
    /// Repository implementation for TaxCode entities
    /// </summary>
    public class TaxCodeRepository : Repository<TaxCode>, ITaxCodeRepository
    {
        private readonly FinancialDbContext _context;

        public TaxCodeRepository(FinancialDbContext context) : base(context)
        {
            _context = context;
        }
        
        /// <inheritdoc />
        public async Task<TaxCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.TaxCodes
                .FirstOrDefaultAsync(tc => tc.Code == code, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<TaxCode>> GetActiveOnDateAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var checkDate = date ?? DateTime.Today;
            
            return await _context.TaxCodes
                .Where(tc => 
                    tc.IsActive && 
                    tc.EffectiveDate <= checkDate && 
                    (!tc.ExpirationDate.HasValue || tc.ExpirationDate >= checkDate))
                .OrderBy(tc => tc.Code)
                .ToListAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<TaxCode?> ActivateAsync(Guid id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var taxCode = await _context.TaxCodes.FindAsync(new object[] { id }, cancellationToken);
            
            if (taxCode == null)
            {
                return null;
            }
            
            taxCode.IsActive = true;
            taxCode.UpdatedAt = DateTime.UtcNow;
            taxCode.UpdatedBy = modifiedBy;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return taxCode;
        }
        
        /// <inheritdoc />
        public async Task<TaxCode?> DeactivateAsync(Guid id, DateTime expirationDate, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var taxCode = await _context.TaxCodes.FindAsync(new object[] { id }, cancellationToken);
            
            if (taxCode == null)
            {
                return null;
            }
            
            taxCode.IsActive = false;
            taxCode.ExpirationDate = expirationDate;
            taxCode.UpdatedAt = DateTime.UtcNow;
            taxCode.UpdatedBy = modifiedBy;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return taxCode;
        }
    }
}
