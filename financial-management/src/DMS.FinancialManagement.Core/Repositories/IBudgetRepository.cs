using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.FinancialManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for Budget entities
    /// </summary>
    public interface IBudgetRepository : IRepository<Budget>
    {
        /// <summary>
        /// Gets budgets by fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of budgets for the specified fiscal year</returns>
        Task<IEnumerable<Budget>> GetByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a budget with its budget lines
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The budget with the specified ID, including budget lines</returns>
        Task<Budget?> GetWithBudgetLinesAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Approves a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="approvalDate">The approval date</param>
        /// <param name="approvedBy">The user who approved the budget</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The approved budget</returns>
        Task<Budget?> ApproveAsync(Guid id, DateTime approvalDate, string approvedBy, CancellationToken cancellationToken = default);
    }
}
