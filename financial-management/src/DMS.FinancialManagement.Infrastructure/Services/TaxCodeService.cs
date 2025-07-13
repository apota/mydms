using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Repositories;
using DMS.FinancialManagement.Core.Services;

namespace DMS.FinancialManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for managing tax codes
    /// </summary>
    public class TaxCodeService : ITaxCodeService
    {
        private readonly ITaxCodeRepository _taxCodeRepository;
        
        public TaxCodeService(ITaxCodeRepository taxCodeRepository)
        {
            _taxCodeRepository = taxCodeRepository;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<TaxCodeDto>> GetAllTaxCodesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var taxCodes = await _taxCodeRepository.GetAllAsync(skip, take, cancellationToken);
            return taxCodes.Select(MapToDto);
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto?> GetTaxCodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var taxCode = await _taxCodeRepository.GetByIdAsync(id, cancellationToken);
            return taxCode != null ? MapToDto(taxCode) : null;
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto?> GetTaxCodeByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var taxCode = await _taxCodeRepository.GetByCodeAsync(code, cancellationToken);
            return taxCode != null ? MapToDto(taxCode) : null;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<TaxCodeDto>> GetActiveTaxCodesAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var taxCodes = await _taxCodeRepository.GetActiveOnDateAsync(date, cancellationToken);
            return taxCodes.Select(MapToDto);
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto> CreateTaxCodeAsync(TaxCodeCreateDto taxCodeDto, CancellationToken cancellationToken = default)
        {
            // Validate tax code uniqueness
            var existingTaxCode = await _taxCodeRepository.GetByCodeAsync(taxCodeDto.Code, cancellationToken);
            if (existingTaxCode != null)
            {
                throw new InvalidOperationException($"Tax code with code '{taxCodeDto.Code}' already exists");
            }
            
            // Validate effective and expiration dates
            if (taxCodeDto.ExpirationDate.HasValue && taxCodeDto.EffectiveDate > taxCodeDto.ExpirationDate.Value)
            {
                throw new InvalidOperationException("Effective date must be before or equal to expiration date");
            }
            
            var newTaxCode = new TaxCode
            {
                Id = Guid.NewGuid(),
                Code = taxCodeDto.Code,
                Description = taxCodeDto.Description,
                Rate = taxCodeDto.Rate,
                EffectiveDate = taxCodeDto.EffectiveDate,
                ExpirationDate = taxCodeDto.ExpirationDate,
                IsActive = taxCodeDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system" // This should be replaced with actual user info from auth context
            };
            
            await _taxCodeRepository.AddAsync(newTaxCode, cancellationToken);
            
            return MapToDto(newTaxCode);
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto?> UpdateTaxCodeAsync(Guid id, TaxCodeUpdateDto taxCodeDto, CancellationToken cancellationToken = default)
        {
            var taxCode = await _taxCodeRepository.GetByIdAsync(id, cancellationToken);
            
            if (taxCode == null)
            {
                return null;
            }
            
            // Validate effective and expiration dates
            if (taxCodeDto.ExpirationDate.HasValue && taxCodeDto.EffectiveDate > taxCodeDto.ExpirationDate.Value)
            {
                throw new InvalidOperationException("Effective date must be before or equal to expiration date");
            }
            
            // Update properties
            taxCode.Description = taxCodeDto.Description;
            taxCode.Rate = taxCodeDto.Rate;
            taxCode.EffectiveDate = taxCodeDto.EffectiveDate;
            taxCode.ExpirationDate = taxCodeDto.ExpirationDate;
            taxCode.UpdatedAt = DateTime.UtcNow;
            taxCode.UpdatedBy = "system"; // This should be replaced with actual user info from auth context
            
            await _taxCodeRepository.UpdateAsync(taxCode, cancellationToken);
            
            return MapToDto(taxCode);
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto?> ActivateTaxCodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var taxCode = await _taxCodeRepository.ActivateAsync(id, "system", cancellationToken); // "system" should be replaced with actual user info
            return taxCode != null ? MapToDto(taxCode) : null;
        }
        
        /// <inheritdoc />
        public async Task<TaxCodeDto?> DeactivateTaxCodeAsync(Guid id, TaxCodeDeactivateDto deactivateDto, CancellationToken cancellationToken = default)
        {
            var taxCode = await _taxCodeRepository.DeactivateAsync(id, deactivateDto.ExpirationDate, "system", cancellationToken); // "system" should be replaced with actual user info
            return taxCode != null ? MapToDto(taxCode) : null;
        }
        
        #region Helper Methods
        
        private TaxCodeDto MapToDto(TaxCode taxCode)
        {
            return new TaxCodeDto
            {
                Id = taxCode.Id,
                Code = taxCode.Code,
                Description = taxCode.Description,
                Rate = taxCode.Rate,
                EffectiveDate = taxCode.EffectiveDate,
                ExpirationDate = taxCode.ExpirationDate,
                IsActive = taxCode.IsActive,
                CreatedAt = taxCode.CreatedAt,
                CreatedBy = taxCode.CreatedBy,
                UpdatedAt = taxCode.UpdatedAt
            };
        }
        
        #endregion
    }
}
