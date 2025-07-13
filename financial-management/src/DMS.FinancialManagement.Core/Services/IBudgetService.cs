using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing budgets
    /// </summary>
    public interface IBudgetService
    {        /// <summary>
        /// Gets all budgets
        /// </summary>
        /// <param name="year">Optional filter by fiscal year</param>
        /// <param name="skip">The number of budgets to skip</param>
        /// <param name="take">The number of budgets to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of budget summaries</returns>
        Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(int? year = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
          /// <summary>
        /// Gets a budget by ID
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The budget with the specified ID</returns>
        Task<BudgetDetailDto?> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken = default);
          /// <summary>
        /// Gets budgets by fiscal year
        /// </summary>
        /// <param name="year">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of budget summaries for the specified fiscal year</returns>
        Task<IEnumerable<BudgetDto>> GetBudgetsByYearAsync(int year, CancellationToken cancellationToken = default);
          /// <summary>
        /// Creates a new budget
        /// </summary>
        /// <param name="createDto">The budget data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created budget</returns>
        Task<BudgetDetailDto> CreateBudgetAsync(BudgetCreateDto createDto, CancellationToken cancellationToken = default);
          /// <summary>
        /// Updates an existing budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="updateDto">The updated budget data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated budget</returns>
        Task<BudgetDetailDto?> UpdateBudgetAsync(Guid id, BudgetUpdateDto updateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the budget was deleted, false otherwise</returns>
        Task<bool> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken = default);
          /// <summary>
        /// Approves a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="approveDto">The approval data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The approved budget</returns>
        Task<BudgetDetailDto?> ApproveBudgetAsync(Guid id, BudgetApproveDto approveDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rejects a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="rejectDto">The rejection data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The rejected budget</returns>
        Task<BudgetDetailDto?> RejectBudgetAsync(Guid id, BudgetRejectDto rejectDto, CancellationToken cancellationToken = default);
          /// <summary>
        /// Gets budget comparison against actuals
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The budget comparison report</returns>
        Task<BudgetComparisonReportDto?> GetBudgetComparisonAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
