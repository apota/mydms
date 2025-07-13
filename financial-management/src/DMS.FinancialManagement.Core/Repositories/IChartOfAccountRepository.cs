using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.FinancialManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for ChartOfAccount entities
    /// </summary>
    public interface IChartOfAccountRepository : IRepository<ChartOfAccount>
    {
        /// <summary>
        /// Gets accounts by type
        /// </summary>
        /// <param name="accountType">The account type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of accounts of the specified type</returns>
        Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets accounts by parent account ID
        /// </summary>
        /// <param name="parentId">The parent account ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of child accounts for the specified parent</returns>
        Task<IEnumerable<ChartOfAccount>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets accounts by account code
        /// </summary>
        /// <param name="accountCode">The account code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The account with the specified code</returns>
        Task<ChartOfAccount?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the full chart of accounts hierarchy
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive accounts</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The full hierarchy of accounts</returns>
        Task<IEnumerable<ChartOfAccount>> GetHierarchyAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    }
}
