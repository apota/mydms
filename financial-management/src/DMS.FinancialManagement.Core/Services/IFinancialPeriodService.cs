using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing financial periods
    /// </summary>
    public interface IFinancialPeriodService
    {
        /// <summary>
        /// Gets all financial periods
        /// </summary>
        /// <param name="skip">The number of periods to skip</param>
        /// <param name="take">The number of periods to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of financial periods</returns>
        Task<IEnumerable<FinancialPeriodDto>> GetAllPeriodsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a financial period by ID
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period with the specified ID</returns>
        Task<FinancialPeriodDto?> GetPeriodByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets financial periods by fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of financial periods for the specified fiscal year</returns>
        Task<IEnumerable<FinancialPeriodDto>> GetPeriodsByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current financial period
        /// </summary>
        /// <param name="date">The date to check (defaults to today)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period that contains the specified date</returns>
        Task<FinancialPeriodDto?> GetCurrentPeriodAsync(DateTime? date = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the financial period by year and period number
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period for the specified year and period number</returns>
        Task<FinancialPeriodDto?> GetPeriodByYearAndNumberAsync(int fiscalYear, int periodNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new financial period
        /// </summary>
        /// <param name="periodDto">The financial period data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created financial period</returns>
        Task<FinancialPeriodDto> CreatePeriodAsync(FinancialPeriodCreateDto periodDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="periodDto">The updated financial period data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated financial period</returns>
        Task<FinancialPeriodDto?> UpdatePeriodAsync(Guid id, FinancialPeriodUpdateDto periodDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Closes a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="closeDto">The closing data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The closed financial period</returns>
        Task<FinancialPeriodDto?> ClosePeriodAsync(Guid id, FinancialPeriodCloseDto closeDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reopens a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reopened financial period</returns>
        Task<FinancialPeriodDto?> ReopenPeriodAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generates financial periods for a fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="startMonth">The starting month of the fiscal year (1-12)</param>
        /// <param name="periodCount">The number of periods to generate (default is 12 for monthly)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The collection of generated financial periods</returns>
        Task<IEnumerable<FinancialPeriodDto>> GeneratePeriodsForYearAsync(int fiscalYear, int startMonth = 1, int periodCount = 12, CancellationToken cancellationToken = default);
    }
}
