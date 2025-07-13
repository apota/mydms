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
    /// Repository implementation for Budget entities
    /// </summary>
    public class BudgetRepository : Repository<Budget>, IBudgetRepository
    {
        private readonly FinancialDbContext _context;

        public BudgetRepository(FinancialDbContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Budget>> GetByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default)
        {
            return await _context.Budgets
                .Where(b => b.FiscalYear == fiscalYear)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Budget?> GetWithBudgetLinesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Budgets
                .Include(b => b.BudgetLines)
                    .ThenInclude(bl => bl.Account)
                .Include(b => b.BudgetLines)
                    .ThenInclude(bl => bl.FinancialPeriod)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Budget?> ApproveAsync(Guid id, DateTime approvalDate, string approvedBy, CancellationToken cancellationToken = default)
        {
            var budget = await _context.Budgets.FindAsync(new object[] { id }, cancellationToken);
            
            if (budget == null)
            {
                return null;
            }
            
            // Cannot approve an already approved budget
            if (budget.IsApproved)
            {
                throw new InvalidOperationException($"Budget {budget.Name} is already approved");
            }
            
            budget.IsApproved = true;
            budget.ApprovedDate = approvalDate;
            budget.ApprovedBy = approvedBy;
            budget.UpdatedAt = DateTime.UtcNow;
            budget.UpdatedBy = approvedBy;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return budget;
        }
        
        /// <inheritdoc />
        public override async Task<IEnumerable<Budget>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Budgets
                .OrderByDescending(b => b.FiscalYear)
                .ThenBy(b => b.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
        
        /// <inheritdoc />
        public override async Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await GetWithBudgetLinesAsync(id, cancellationToken);
        }
        
        /// <inheritdoc />
        public override async Task DeleteAsync(Budget entity, CancellationToken cancellationToken = default)
        {
            // Check if budget is approved
            if (entity.IsApproved)
            {
                throw new InvalidOperationException($"Cannot delete approved budget {entity.Name}");
            }
            
            // Remove all budget lines
            var budgetLines = await _context.BudgetLines
                .Where(bl => bl.BudgetId == entity.Id)
                .ToListAsync(cancellationToken);
                
            _context.BudgetLines.RemoveRange(budgetLines);
            
            // Remove budget
            _context.Budgets.Remove(entity);
            
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
