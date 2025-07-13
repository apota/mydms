using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.FinancialManagement.Infrastructure.Data.Repositories
{
    public class ChartOfAccountRepository : IChartOfAccountRepository
    {
        private readonly FinancialDbContext _dbContext;

        public ChartOfAccountRepository(FinancialDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<ChartOfAccount>> GetAllAsync(int skip = 0, int take = 50)
        {
            return await _dbContext.ChartOfAccounts
                .Include(a => a.ParentAccount)
                .OrderBy(a => a.AccountCode)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ChartOfAccount?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChartOfAccounts
                .Include(a => a.ParentAccount)
                .Include(a => a.ChildAccounts)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChartOfAccounts
                .Where(a => a.AccountType == accountType)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChartOfAccounts
                .Where(a => a.ParentAccountId == parentId)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChartOfAccount?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChartOfAccounts
                .Include(a => a.ParentAccount)
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode, cancellationToken);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetHierarchyAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.ChartOfAccounts.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }
            
            // Get root accounts (with no parent)
            var rootAccounts = await query
                .Where(a => a.ParentAccountId == null)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
            
            // For each root account, recursively load its children
            foreach (var account in rootAccounts)
            {
                await LoadChildAccountsAsync(account, includeInactive, cancellationToken);
            }
            
            return rootAccounts;
        }

        public async Task<ChartOfAccount> AddAsync(ChartOfAccount account)
        {
            _dbContext.ChartOfAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<ChartOfAccount> UpdateAsync(ChartOfAccount account)
        {
            _dbContext.Entry(account).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var account = await _dbContext.ChartOfAccounts.FindAsync(id);
            if (account == null)
            {
                return false;
            }
            
            // Check if this account has child accounts
            bool hasChildren = await _dbContext.ChartOfAccounts.AnyAsync(a => a.ParentAccountId == id);
            if (hasChildren)
            {
                return false; // Cannot delete an account with children
            }
            
            // Check if this account has any journal entries
            bool hasJournalEntries = await _dbContext.JournalLineItems.AnyAsync(j => j.AccountId == id);
            if (hasJournalEntries)
            {
                return false; // Cannot delete an account with journal entries
            }
            
            _dbContext.ChartOfAccounts.Remove(account);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        
        private async Task LoadChildAccountsAsync(ChartOfAccount parent, bool includeInactive, CancellationToken cancellationToken)
        {
            var query = _dbContext.ChartOfAccounts.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }
            
            var children = await query
                .Where(a => a.ParentAccountId == parent.Id)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
                
            parent.ChildAccounts = children;
            
            // Recursively load children of children
            foreach (var child in children)
            {
                await LoadChildAccountsAsync(child, includeInactive, cancellationToken);
            }
        }
    }
}
