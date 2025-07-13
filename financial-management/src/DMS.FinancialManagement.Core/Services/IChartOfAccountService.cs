using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Models;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing chart of accounts
    /// </summary>
    public interface IChartOfAccountService
    {
        /// <summary>
        /// Gets all accounts
        /// </summary>
        /// <param name="skip">The number of accounts to skip</param>
        /// <param name="take">The number of accounts to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of accounts</returns>
        Task<IEnumerable<ChartOfAccountDto>> GetAllAccountsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets an account by ID
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The account with the specified ID</returns>
        Task<ChartOfAccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets accounts by type
        /// </summary>
        /// <param name="accountType">The account type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of accounts of the specified type</returns>
        Task<IEnumerable<ChartOfAccountDto>> GetAccountsByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets accounts by parent ID
        /// </summary>
        /// <param name="parentId">The parent account ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of child accounts for the specified parent</returns>
        Task<IEnumerable<ChartOfAccountDto>> GetAccountsByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the full chart of accounts hierarchy
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive accounts</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The full hierarchy of accounts</returns>
        Task<IEnumerable<ChartOfAccountDto>> GetAccountHierarchyAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new account
        /// </summary>
        /// <param name="accountDto">The account data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created account</returns>
        Task<ChartOfAccountDto> CreateAccountAsync(ChartOfAccountCreateDto accountDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="accountDto">The updated account data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated account</returns>
        Task<ChartOfAccountDto?> UpdateAccountAsync(Guid id, ChartOfAccountUpdateDto accountDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deactivates an account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the account was deactivated, false otherwise</returns>
        Task<bool> DeactivateAccountAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Activates an account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if the account was activated, false otherwise</returns>
        Task<bool> ActivateAccountAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
