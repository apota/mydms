using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing tax codes
    /// </summary>
    public interface ITaxCodeService
    {
        /// <summary>
        /// Gets all tax codes
        /// </summary>
        /// <param name="skip">The number of tax codes to skip</param>
        /// <param name="take">The number of tax codes to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of tax codes</returns>
        Task<IEnumerable<TaxCodeDto>> GetAllTaxCodesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax code by ID
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tax code with the specified ID</returns>
        Task<TaxCodeDto?> GetTaxCodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax code by code
        /// </summary>
        /// <param name="code">The tax code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tax code with the specified code</returns>
        Task<TaxCodeDto?> GetTaxCodeByCodeAsync(string code, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets active tax codes as of a specific date
        /// </summary>
        /// <param name="date">The date to check (defaults to today)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of active tax codes as of the specified date</returns>
        Task<IEnumerable<TaxCodeDto>> GetActiveTaxCodesAsync(DateTime? date = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new tax code
        /// </summary>
        /// <param name="taxCodeDto">The tax code data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created tax code</returns>
        Task<TaxCodeDto> CreateTaxCodeAsync(TaxCodeCreateDto taxCodeDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="taxCodeDto">The updated tax code data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated tax code</returns>
        Task<TaxCodeDto?> UpdateTaxCodeAsync(Guid id, TaxCodeUpdateDto taxCodeDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Activates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The activated tax code</returns>
        Task<TaxCodeDto?> ActivateTaxCodeAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deactivates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="deactivateDto">The deactivation data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deactivated tax code</returns>
        Task<TaxCodeDto?> DeactivateTaxCodeAsync(Guid id, TaxCodeDeactivateDto deactivateDto, CancellationToken cancellationToken = default);
    }
}
