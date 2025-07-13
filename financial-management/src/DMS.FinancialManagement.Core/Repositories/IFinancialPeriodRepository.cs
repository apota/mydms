using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.FinancialManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for FinancialPeriod entities
    /// </summary>
    public interface IFinancialPeriodRepository : IRepository<FinancialPeriod>
    {
        /// <summary>
        /// Gets financial periods by fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of financial periods for the specified fiscal year</returns>
        Task<IEnumerable<FinancialPeriod>> GetByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current financial period
        /// </summary>
        /// <param name="date">The date to check (defaults to today)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period that contains the specified date</returns>
        Task<FinancialPeriod?> GetCurrentPeriodAsync(DateTime? date = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the financial period by year and period number
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period for the specified year and period number</returns>
        Task<FinancialPeriod?> GetByYearAndPeriodAsync(int fiscalYear, int periodNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Closes a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="closedBy">The user who closed the period</param>
        /// <param name="closedDate">The date when the period was closed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The closed financial period</returns>
        Task<FinancialPeriod?> ClosePeriodAsync(Guid id, string closedBy, DateTime closedDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reopens a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="reopenedBy">The user who reopened the period</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reopened financial period</returns>
        Task<FinancialPeriod?> ReopenPeriodAsync(Guid id, string reopenedBy, CancellationToken cancellationToken = default);
    }
}
