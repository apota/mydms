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
    /// Service implementation for managing financial periods
    /// </summary>
    public class FinancialPeriodService : IFinancialPeriodService
    {
        private readonly IFinancialPeriodRepository _financialPeriodRepository;

        public FinancialPeriodService(IFinancialPeriodRepository financialPeriodRepository)
        {
            _financialPeriodRepository = financialPeriodRepository;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<FinancialPeriodDto>> GetAllPeriodsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var periods = await _financialPeriodRepository.GetAllAsync(skip, take, cancellationToken);
            return periods.Select(MapToDto);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> GetPeriodByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var period = await _financialPeriodRepository.GetByIdAsync(id, cancellationToken);
            return period == null ? null : MapToDto(period);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<FinancialPeriodDto>> GetPeriodsByFiscalYearAsync(int fiscalYear, CancellationToken cancellationToken = default)
        {
            var periods = await _financialPeriodRepository.GetByFiscalYearAsync(fiscalYear, cancellationToken);
            return periods.Select(MapToDto);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> GetCurrentPeriodAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var period = await _financialPeriodRepository.GetCurrentPeriodAsync(date, cancellationToken);
            return period == null ? null : MapToDto(period);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> GetPeriodByYearAndNumberAsync(int fiscalYear, int periodNumber, CancellationToken cancellationToken = default)
        {
            var period = await _financialPeriodRepository.GetByYearAndPeriodAsync(fiscalYear, periodNumber, cancellationToken);
            return period == null ? null : MapToDto(period);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto> CreatePeriodAsync(FinancialPeriodCreateDto periodDto, CancellationToken cancellationToken = default)
        {
            // Validate fiscal year and period number
            if (periodDto.FiscalYear < DateTime.Now.Year - 10 || periodDto.FiscalYear > DateTime.Now.Year + 10)
            {
                throw new InvalidOperationException($"Fiscal year {periodDto.FiscalYear} is out of valid range");
            }
            
            if (periodDto.PeriodNumber < 1 || periodDto.PeriodNumber > 12)
            {
                throw new InvalidOperationException($"Period number must be between 1 and 12");
            }
            
            // Validate start date is before end date
            if (periodDto.StartDate >= periodDto.EndDate)
            {
                throw new InvalidOperationException("Start date must be before end date");
            }
            
            // Check if period already exists
            var existingPeriod = await _financialPeriodRepository.GetByYearAndPeriodAsync(periodDto.FiscalYear, periodDto.PeriodNumber, cancellationToken);
            if (existingPeriod != null)
            {
                throw new InvalidOperationException($"Financial period {periodDto.FiscalYear}-{periodDto.PeriodNumber} already exists");
            }
            
            // Check for overlapping periods
            var periodsInYear = await _financialPeriodRepository.GetByFiscalYearAsync(periodDto.FiscalYear, cancellationToken);
            
            if (periodsInYear.Any(p => 
                (periodDto.StartDate >= p.StartDate && periodDto.StartDate <= p.EndDate) ||
                (periodDto.EndDate >= p.StartDate && periodDto.EndDate <= p.EndDate) ||
                (periodDto.StartDate <= p.StartDate && periodDto.EndDate >= p.EndDate)))
            {
                throw new InvalidOperationException("The new period overlaps with an existing period");
            }
            
            // Create new period
            var newPeriod = new FinancialPeriod
            {
                Id = Guid.NewGuid(),
                FiscalYear = periodDto.FiscalYear,
                PeriodNumber = periodDto.PeriodNumber,
                StartDate = periodDto.StartDate,
                EndDate = periodDto.EndDate,
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system" // This should be replaced with actual user info from auth context
            };
            
            await _financialPeriodRepository.AddAsync(newPeriod, cancellationToken);
            
            return MapToDto(newPeriod);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> UpdatePeriodAsync(Guid id, FinancialPeriodUpdateDto periodDto, CancellationToken cancellationToken = default)
        {
            var existingPeriod = await _financialPeriodRepository.GetByIdAsync(id, cancellationToken);
            
            if (existingPeriod == null)
            {
                return null;
            }
            
            // Cannot update a closed period
            if (existingPeriod.IsClosed)
            {
                throw new InvalidOperationException("Cannot update a closed financial period");
            }
            
            // Validate start date is before end date
            if (periodDto.StartDate >= periodDto.EndDate)
            {
                throw new InvalidOperationException("Start date must be before end date");
            }
            
            // Check for overlapping periods
            var periodsInYear = await _financialPeriodRepository.GetByFiscalYearAsync(existingPeriod.FiscalYear, cancellationToken);
            
            if (periodsInYear.Any(p => 
                p.Id != id && (
                (periodDto.StartDate >= p.StartDate && periodDto.StartDate <= p.EndDate) ||
                (periodDto.EndDate >= p.StartDate && periodDto.EndDate <= p.EndDate) ||
                (periodDto.StartDate <= p.StartDate && periodDto.EndDate >= p.EndDate))))
            {
                throw new InvalidOperationException("The updated period would overlap with an existing period");
            }
            
            // Update period
            existingPeriod.StartDate = periodDto.StartDate;
            existingPeriod.EndDate = periodDto.EndDate;
            existingPeriod.UpdatedAt = DateTime.UtcNow;
            existingPeriod.UpdatedBy = "system"; // This should be replaced with actual user info from auth context
            
            await _financialPeriodRepository.UpdateAsync(existingPeriod, cancellationToken);
            
            return MapToDto(existingPeriod);
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> ClosePeriodAsync(Guid id, FinancialPeriodCloseDto closeDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var closedPeriod = await _financialPeriodRepository.ClosePeriodAsync(id, "system", closeDto.ClosedDate, cancellationToken);
                return closedPeriod == null ? null : MapToDto(closedPeriod);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<FinancialPeriodDto?> ReopenPeriodAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var reopenedPeriod = await _financialPeriodRepository.ReopenPeriodAsync(id, "system", cancellationToken);
                return reopenedPeriod == null ? null : MapToDto(reopenedPeriod);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<FinancialPeriodDto>> GeneratePeriodsForYearAsync(int fiscalYear, int startMonth = 1, int periodCount = 12, CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (fiscalYear < DateTime.Now.Year - 10 || fiscalYear > DateTime.Now.Year + 10)
            {
                throw new InvalidOperationException($"Fiscal year {fiscalYear} is out of valid range");
            }
            
            if (startMonth < 1 || startMonth > 12)
            {
                throw new InvalidOperationException($"Start month must be between 1 and 12");
            }
            
            if (periodCount < 1 || periodCount > 12)
            {
                throw new InvalidOperationException($"Period count must be between 1 and 12");
            }
            
            // Check if periods already exist for the fiscal year
            var existingPeriods = await _financialPeriodRepository.GetByFiscalYearAsync(fiscalYear, cancellationToken);
            if (existingPeriods.Any())
            {
                throw new InvalidOperationException($"Financial periods already exist for fiscal year {fiscalYear}");
            }
            
            var periods = new List<FinancialPeriod>();
            
            // Generate periods based on fiscal year and start month
            var currentDate = new DateTime(fiscalYear, startMonth, 1);
            
            for (int i = 1; i <= periodCount; i++)
            {
                var startDate = currentDate;
                var endDate = startDate.AddMonths(1).AddDays(-1); // Last day of the month
                
                var period = new FinancialPeriod
                {
                    Id = Guid.NewGuid(),
                    FiscalYear = fiscalYear,
                    PeriodNumber = i,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsClosed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system" // This should be replaced with actual user info from auth context
                };
                
                periods.Add(period);
                await _financialPeriodRepository.AddAsync(period, cancellationToken);
                
                currentDate = endDate.AddDays(1); // Start of next month
            }
            
            return periods.Select(MapToDto);
        }
        
        #region Helper Methods
        
        private FinancialPeriodDto MapToDto(FinancialPeriod period)
        {
            return new FinancialPeriodDto
            {
                Id = period.Id,
                FiscalYear = period.FiscalYear,
                PeriodNumber = period.PeriodNumber,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsClosed = period.IsClosed,
                ClosedDate = period.ClosedDate,
                ClosedBy = period.ClosedBy,
                CreatedAt = period.CreatedAt,
                UpdatedAt = period.UpdatedAt
            };
        }
        
        #endregion
    }
}
