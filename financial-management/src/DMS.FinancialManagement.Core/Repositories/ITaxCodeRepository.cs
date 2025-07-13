using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.FinancialManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for TaxCode entities
    /// </summary>
    public interface ITaxCodeRepository : IRepository<TaxCode>
    {
        /// <summary>
        /// Gets tax codes by code
        /// </summary>
        /// <param name="code">The tax code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tax code with the specified code</returns>
        Task<TaxCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets active tax codes as of a specific date
        /// </summary>
        /// <param name="date">The date to check (defaults to today)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of active tax codes as of the specified date</returns>
        Task<IEnumerable<TaxCode>> GetActiveOnDateAsync(DateTime? date = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Activates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="modifiedBy">The user who activated the tax code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The activated tax code</returns>
        Task<TaxCode?> ActivateAsync(Guid id, string modifiedBy, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deactivates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="expirationDate">The expiration date for the tax code</param>
        /// <param name="modifiedBy">The user who deactivated the tax code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deactivated tax code</returns>
        Task<TaxCode?> DeactivateAsync(Guid id, DateTime expirationDate, string modifiedBy, CancellationToken cancellationToken = default);
    }
}
