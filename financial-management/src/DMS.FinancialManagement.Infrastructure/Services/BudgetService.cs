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
    /// Service implementation for managing budgets
    /// </summary>
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;

        public BudgetService(
            IBudgetRepository budgetRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryRepository journalEntryRepository)
        {
            _budgetRepository = budgetRepository;
            _chartOfAccountRepository = chartOfAccountRepository;
            _financialPeriodRepository = financialPeriodRepository;
            _journalEntryRepository = journalEntryRepository;
        }        /// <inheritdoc />
        public async Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(int? year = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            if (year.HasValue)
            {
                var budgets = await _budgetRepository.GetByFiscalYearAsync(year.Value, cancellationToken);
                return budgets.Select(MapToDto);
            }
            else
            {
                var budgets = await _budgetRepository.GetAllAsync(skip, take, cancellationToken);
                return budgets.Select(MapToDto);
            }
        }        /// <inheritdoc />
        public async Task<BudgetDetailDto?> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
            return budget == null ? null : MapToDetailDto(budget);
        }        /// <inheritdoc />
        public async Task<IEnumerable<BudgetDto>> GetBudgetsByYearAsync(int year, CancellationToken cancellationToken = default)
        {
            var budgets = await _budgetRepository.GetByFiscalYearAsync(year, cancellationToken);
            return budgets.Select(MapToDto);
        }

        /// <inheritdoc />
        public async Task<BudgetDto> CreateBudgetAsync(BudgetCreateDto budgetDto, CancellationToken cancellationToken = default)
        {
            // Validate fiscal year
            if (budgetDto.FiscalYear < DateTime.Now.Year - 10 || budgetDto.FiscalYear > DateTime.Now.Year + 10)
            {
                throw new InvalidOperationException($"Fiscal year {budgetDto.FiscalYear} is out of valid range");
            }
            
            // Validate budget lines
            if (!budgetDto.BudgetLines.Any())
            {
                throw new InvalidOperationException("Budget must have at least one budget line");
            }
            
            // Validate account IDs
            var accountIds = budgetDto.BudgetLines.Select(bl => bl.AccountId).Distinct().ToList();
            foreach (var accountId in accountIds)
            {
                var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} does not exist");
                }
                
                if (!account.IsActive)
                {
                    throw new InvalidOperationException($"Account {account.Name} is not active");
                }
            }
            
            // Validate financial period IDs
            var periodIds = budgetDto.BudgetLines.Select(bl => bl.FinancialPeriodId).Distinct().ToList();
            foreach (var periodId in periodIds)
            {
                var period = await _financialPeriodRepository.GetByIdAsync(periodId, cancellationToken);
                if (period == null)
                {
                    throw new InvalidOperationException($"Financial period with ID {periodId} does not exist");
                }
                
                // Check if financial period is in the specified fiscal year
                if (period.FiscalYear != budgetDto.FiscalYear)
                {
                    throw new InvalidOperationException($"Financial period {period.FiscalYear}-{period.PeriodNumber} is not in fiscal year {budgetDto.FiscalYear}");
                }
            }
            
            // Create budget
            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                Name = budgetDto.Name,
                Description = budgetDto.Description,
                FiscalYear = budgetDto.FiscalYear,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system" // This should be replaced with actual user info from auth context
            };
            
            // Create budget lines
            foreach (var lineDto in budgetDto.BudgetLines)
            {
                var budgetLine = new BudgetLine
                {
                    Id = Guid.NewGuid(),
                    BudgetId = budget.Id,
                    AccountId = lineDto.AccountId,
                    FinancialPeriodId = lineDto.FinancialPeriodId,
                    PlannedAmount = lineDto.PlannedAmount,
                    Notes = lineDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system" // This should be replaced with actual user info from auth context
                };
                
                budget.BudgetLines.Add(budgetLine);
            }
              // Save to database
            await _budgetRepository.AddAsync(budget, cancellationToken);
            
            // Reload budget with navigation properties
            var savedBudget = await _budgetRepository.GetByIdAsync(budget.Id, cancellationToken);
            if (savedBudget == null)
            {
                throw new InvalidOperationException("Failed to create budget");
            }
            
            return MapToDetailDto(savedBudget);
        }

        /// <inheritdoc />
        public async Task<BudgetDto?> UpdateBudgetAsync(Guid id, BudgetUpdateDto budgetDto, CancellationToken cancellationToken = default)
        {
            var existingBudget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
            if (existingBudget == null)
            {
                return null;
            }
            
            // Cannot update an approved budget
            if (existingBudget.IsApproved)
            {
                throw new InvalidOperationException($"Cannot update approved budget {existingBudget.Name}");
            }
            
            // Validate budget lines
            if (!budgetDto.BudgetLines.Any())
            {
                throw new InvalidOperationException("Budget must have at least one budget line");
            }
            
            // Validate account IDs
            var accountIds = budgetDto.BudgetLines.Select(bl => bl.AccountId).Distinct().ToList();
            foreach (var accountId in accountIds)
            {
                var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} does not exist");
                }
                
                if (!account.IsActive)
                {
                    throw new InvalidOperationException($"Account {account.Name} is not active");
                }
            }
            
            // Validate financial period IDs
            var periodIds = budgetDto.BudgetLines.Select(bl => bl.FinancialPeriodId).Distinct().ToList();
            foreach (var periodId in periodIds)
            {
                var period = await _financialPeriodRepository.GetByIdAsync(periodId, cancellationToken);
                if (period == null)
                {
                    throw new InvalidOperationException($"Financial period with ID {periodId} does not exist");
                }
                
                // Check if financial period is in the same fiscal year as the budget
                if (period.FiscalYear != existingBudget.FiscalYear)
                {
                    throw new InvalidOperationException($"Financial period {period.FiscalYear}-{period.PeriodNumber} is not in fiscal year {existingBudget.FiscalYear}");
                }
            }
            
            // Update budget properties
            existingBudget.Name = budgetDto.Name;
            existingBudget.Description = budgetDto.Description;
            existingBudget.UpdatedAt = DateTime.UtcNow;
            existingBudget.UpdatedBy = "system"; // This should be replaced with actual user info from auth context
            
            // Handle budget lines
            var existingLines = existingBudget.BudgetLines.ToList();
            var updatedLines = new List<BudgetLine>();
            
            // Process updated budget lines
            foreach (var lineDto in budgetDto.BudgetLines)
            {
                if (lineDto.Id.HasValue)
                {
                    // Update existing budget line
                    var existingLine = existingLines.FirstOrDefault(l => l.Id == lineDto.Id.Value);
                    if (existingLine != null)
                    {
                        existingLine.AccountId = lineDto.AccountId;
                        existingLine.FinancialPeriodId = lineDto.FinancialPeriodId;
                        existingLine.PlannedAmount = lineDto.PlannedAmount;
                        existingLine.Notes = lineDto.Notes;
                        existingLine.UpdatedAt = DateTime.UtcNow;
                        existingLine.UpdatedBy = "system"; // This should be replaced with actual user info from auth context
                        
                        updatedLines.Add(existingLine);
                    }
                }
                else
                {
                    // Add new budget line
                    var newLine = new BudgetLine
                    {
                        Id = Guid.NewGuid(),
                        BudgetId = existingBudget.Id,
                        AccountId = lineDto.AccountId,
                        FinancialPeriodId = lineDto.FinancialPeriodId,
                        PlannedAmount = lineDto.PlannedAmount,
                        Notes = lineDto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system" // This should be replaced with actual user info from auth context
                    };
                    
                    updatedLines.Add(newLine);
                }
            }
            
            // Remove budget lines not in the updated list
            var linesToRemove = existingLines
                .Where(el => !budgetDto.BudgetLines.Any(l => l.Id.HasValue && l.Id.Value == el.Id))
                .ToList();
            
            foreach (var line in linesToRemove)
            {
                existingBudget.BudgetLines.Remove(line);
            }
            
            // Add new budget lines
            var linesToAdd = updatedLines.Where(ul => !existingLines.Any(el => el.Id == ul.Id)).ToList();
            foreach (var line in linesToAdd)
            {
                existingBudget.BudgetLines.Add(line);
            }
            
            // Save changes
            await _budgetRepository.UpdateAsync(existingBudget, cancellationToken);
              // Get updated budget with all navigation properties
            var updatedBudget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
            if (updatedBudget == null)
            {
                throw new InvalidOperationException("Failed to update budget");
            }
            
            return MapToDetailDto(updatedBudget);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
            if (budget == null)
            {
                return false;
            }
            
            try
            {
                await _budgetRepository.DeleteAsync(budget, cancellationToken);
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }        /// <inheritdoc />
        public async Task<BudgetDetailDto?> ApproveBudgetAsync(Guid id, BudgetApproveDto approveDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var budget = await _budgetRepository.ApproveAsync(id, approveDto.ApprovalDate, "system", cancellationToken);
                return budget == null ? null : MapToDetailDto(budget);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }
        
        /// <inheritdoc />
        public async Task<BudgetDetailDto?> RejectBudgetAsync(Guid id, BudgetRejectDto rejectDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
                if (budget == null)
                {
                    return null;
                }
                
                // Cannot reject an already approved budget
                if (budget.IsApproved)
                {
                    throw new InvalidOperationException($"Cannot reject an approved budget");
                }
                
                budget.IsRejected = true;
                budget.RejectedDate = rejectDto.RejectionDate;
                budget.RejectedBy = "system"; // This should be replaced with actual user info from auth context
                budget.RejectionReason = rejectDto.RejectionReason;
                budget.UpdatedAt = DateTime.UtcNow;
                budget.UpdatedBy = "system"; // This should be replaced with actual user info from auth context
                
                await _budgetRepository.UpdateAsync(budget, cancellationToken);
                
                var updatedBudget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
                return updatedBudget == null ? null : MapToDetailDto(updatedBudget);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }        /// <inheritdoc />
        public async Task<BudgetComparisonReportDto?> GetBudgetComparisonAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var budget = await _budgetRepository.GetByIdAsync(id, cancellationToken);
            if (budget == null)
            {
                return null;
            }
            
            // Get all financial periods for the fiscal year
            var periods = await _financialPeriodRepository.GetByFiscalYearAsync(budget.FiscalYear, cancellationToken);
            var periodsList = periods.ToList();
            
            // Determine date range for comparison
            var comparisonStartDate = startDate ?? periodsList.Min(p => p.StartDate);
            var comparisonEndDate = endDate ?? periodsList.Max(p => p.EndDate);
            
            // Get journal entries for the date range
            var journalEntries = await _journalEntryRepository.GetByDateRangeAsync(
                comparisonStartDate, 
                comparisonEndDate, 
                0, 
                int.MaxValue, 
                cancellationToken);
                
            // Group budget lines by account
            var budgetByAccount = budget.BudgetLines
                .GroupBy(bl => bl.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => new { Account = g.First().Account, TotalPlanned = g.Sum(bl => bl.PlannedAmount) }
                );
                
            // Calculate actuals from journal entries
            var actualsByAccount = journalEntries
                .SelectMany(je => je.LineItems)
                .GroupBy(li => li.AccountId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(li => li.DebitAmount - li.CreditAmount)
                );
                
            // Create comparison lines
            var comparisonLines = new List<BudgetComparisonLineDto>();
            
            foreach (var accountId in budgetByAccount.Keys.Union(actualsByAccount.Keys).Distinct())
            {
                var budgeted = budgetByAccount.ContainsKey(accountId) 
                    ? budgetByAccount[accountId].TotalPlanned 
                    : 0;
                    
                var actual = actualsByAccount.ContainsKey(accountId) 
                    ? actualsByAccount[accountId] 
                    : 0;
                    
                var variance = budgeted - actual;
                var variancePercentage = budgeted == 0 
                    ? 0 
                    : Math.Round((variance / budgeted) * 100, 2);
                    
                var account = budgetByAccount.ContainsKey(accountId)
                    ? budgetByAccount[accountId].Account
                    : await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
                    
                if (account == null)
                {
                    continue; // Skip if account not found
                }
                
                comparisonLines.Add(new BudgetComparisonLineDto
                {
                    AccountId = accountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.Name,
                    BudgetedAmount = budgeted,
                    ActualAmount = Math.Abs(actual), // Use absolute value for display
                    Variance = variance,
                    VariancePercentage = variancePercentage
                });
            }
              // Create comparison response
            var totalBudgeted = comparisonLines.Sum(l => l.BudgetedAmount);
            var totalActual = comparisonLines.Sum(l => l.ActualAmount);
            var totalVariance = totalBudgeted - totalActual;
            var totalVariancePercentage = totalBudgeted == 0 
                ? 0 
                : Math.Round((totalVariance / totalBudgeted) * 100, 2);
                
            var comparison = new BudgetComparisonDto
            {
                BudgetId = budget.Id,
                BudgetName = budget.Name,
                FiscalYear = budget.FiscalYear,
                StartDate = comparisonStartDate,
                EndDate = comparisonEndDate,
                ComparisonLines = comparisonLines.OrderBy(l => l.AccountCode).ToList(),
                TotalBudgeted = totalBudgeted,
                TotalActual = totalActual,
                Variance = totalVariance,
                VariancePercentage = totalVariancePercentage
            };
            
            // Create period-by-period comparisons
            var periodComparisons = new List<BudgetComparisonByPeriodDto>();
            foreach (var period in periodsList.OrderBy(p => p.StartDate))
            {
                // Get budget lines for this period
                var periodBudgetLines = budget.BudgetLines
                    .Where(bl => bl.FinancialPeriodId == period.Id)
                    .ToList();
                
                // Get journal entries for this period
                var periodJournalEntries = await _journalEntryRepository.GetByDateRangeAsync(
                    period.StartDate,
                    period.EndDate,
                    0,
                    int.MaxValue,
                    cancellationToken);
                
                // Calculate actuals by account for this period
                var periodActualsByAccount = periodJournalEntries
                    .SelectMany(je => je.LineItems)
                    .GroupBy(li => li.AccountId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(li => li.DebitAmount - li.CreditAmount)
                    );
                
                // Create period comparison lines
                var periodComparisonLines = new List<BudgetComparisonLineDto>();
                foreach (var accountId in periodBudgetLines.Select(bl => bl.AccountId).Distinct())
                {
                    var budgeted = periodBudgetLines
                        .Where(bl => bl.AccountId == accountId)
                        .Sum(bl => bl.PlannedAmount);
                    
                    var actual = periodActualsByAccount.ContainsKey(accountId)
                        ? periodActualsByAccount[accountId]
                        : 0;
                    
                    var variance = budgeted - actual;
                    var variancePercentage = budgeted == 0
                        ? 0
                        : Math.Round((variance / budgeted) * 100, 2);
                    
                    var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
                    if (account == null)
                    {
                        continue; // Skip if account not found
                    }
                    
                    periodComparisonLines.Add(new BudgetComparisonLineDto
                    {
                        AccountId = accountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.Name,
                        BudgetedAmount = budgeted,
                        ActualAmount = Math.Abs(actual),
                        Variance = variance,
                        VariancePercentage = variancePercentage
                    });
                }
                
                // Calculate totals for this period
                var periodTotalBudgeted = periodComparisonLines.Sum(l => l.BudgetedAmount);
                var periodTotalActual = periodComparisonLines.Sum(l => l.ActualAmount);
                var periodTotalVariance = periodTotalBudgeted - periodTotalActual;
                var periodTotalVariancePercentage = periodTotalBudgeted == 0
                    ? 0
                    : Math.Round((periodTotalVariance / periodTotalBudgeted) * 100, 2);
                
                periodComparisons.Add(new BudgetComparisonByPeriodDto
                {
                    FinancialPeriodId = period.Id,
                    FinancialPeriodName = $"{period.FiscalYear}-{period.PeriodNumber}",
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    TotalBudgeted = periodTotalBudgeted,
                    TotalActual = periodTotalActual,
                    Variance = periodTotalVariance,
                    VariancePercentage = periodTotalVariancePercentage,
                    ComparisonLines = periodComparisonLines.OrderBy(l => l.AccountCode).ToList()
                });
            }
            
            // Create and return the full comparison report
            return new BudgetComparisonReportDto
            {
                BudgetId = budget.Id,
                BudgetName = budget.Name,
                FiscalYear = budget.FiscalYear,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = "system", // This should be replaced with actual user info from auth context
                Comparison = comparison,
                PeriodComparisons = periodComparisons.OrderBy(pc => pc.StartDate).ToList(),
                YearToDateBudgeted = totalBudgeted,
                YearToDateActual = totalActual,
                YearToDateVariance = totalVariance,
                YearToDateVariancePercentage = totalVariancePercentage
            };
        }
        
        #region Helper Methods
        
        private BudgetDto MapToDto(Budget budget)
        {
            return new BudgetDto
            {
                Id = budget.Id,
                Name = budget.Name,
                Description = budget.Description,
                FiscalYear = budget.FiscalYear,
                IsApproved = budget.IsApproved,
                ApprovalDate = budget.ApprovedDate,
                ApprovedBy = budget.ApprovedBy,
                BudgetLines = budget.BudgetLines.Select(bl => new BudgetLineDto
                {
                    Id = bl.Id,
                    BudgetId = bl.BudgetId,
                    AccountId = bl.AccountId,
                    AccountCode = bl.Account?.AccountCode ?? string.Empty,
                    AccountName = bl.Account?.Name ?? string.Empty,
                    FinancialPeriodId = bl.FinancialPeriodId,
                    FinancialPeriodName = bl.FinancialPeriod != null 
                        ? $"{bl.FinancialPeriod.FiscalYear}-{bl.FinancialPeriod.PeriodNumber}" 
                        : string.Empty,
                    PlannedAmount = bl.PlannedAmount,
                    Notes = bl.Notes
                }).ToList(),
                TotalPlannedAmount = budget.BudgetLines.Sum(bl => bl.PlannedAmount),
                CreatedAt = budget.CreatedAt,
                CreatedBy = budget.CreatedBy,
                UpdatedAt = budget.UpdatedAt
            };
        }
        
        private BudgetDetailDto MapToDetailDto(Budget budget)
        {
            string status = "Draft";
            if (budget.IsApproved)
                status = "Approved";
            else if (budget.IsRejected)
                status = "Rejected";
            
            return new BudgetDetailDto
            {
                Id = budget.Id,
                Name = budget.Name,
                Description = budget.Description,
                FiscalYear = budget.FiscalYear,
                IsApproved = budget.IsApproved,
                IsRejected = budget.IsRejected,
                Status = status,
                ApprovalDate = budget.ApprovedDate,
                ApprovedBy = budget.ApprovedBy,
                RejectionDate = budget.RejectedDate,
                RejectedBy = budget.RejectedBy,
                RejectionReason = budget.RejectionReason,
                Department = budget.Department,
                BudgetLines = budget.BudgetLines.Select(bl => new BudgetLineDetailDto
                {
                    Id = bl.Id,
                    BudgetId = bl.BudgetId,
                    AccountId = bl.AccountId,
                    AccountCode = bl.Account?.AccountCode ?? string.Empty,
                    AccountName = bl.Account?.Name ?? string.Empty,
                    FinancialPeriodId = bl.FinancialPeriodId,
                    FinancialPeriodName = bl.FinancialPeriod != null 
                        ? $"{bl.FinancialPeriod.FiscalYear}-{bl.FinancialPeriod.PeriodNumber}" 
                        : string.Empty,
                    PlannedAmount = bl.PlannedAmount,
                    Notes = bl.Notes,
                    CreatedAt = bl.CreatedAt,
                    CreatedBy = bl.CreatedBy,
                    UpdatedAt = bl.UpdatedAt,
                    UpdatedBy = bl.UpdatedBy
                }).ToList(),
                TotalPlannedAmount = budget.BudgetLines.Sum(bl => bl.PlannedAmount),
                CreatedAt = budget.CreatedAt,
                CreatedBy = budget.CreatedBy,
                UpdatedAt = budget.UpdatedAt,
                UpdatedBy = budget.UpdatedBy
            };
        }
        
        private BudgetSummaryDto MapToSummaryDto(Budget budget)
        {
            return new BudgetSummaryDto
            {
                Id = budget.Id,
                Name = budget.Name,
                FiscalYear = budget.FiscalYear,
                IsApproved = budget.IsApproved,
                ApprovalDate = budget.ApprovedDate,
                TotalPlannedAmount = budget.BudgetLines.Sum(bl => bl.PlannedAmount),
                BudgetLineCount = budget.BudgetLines.Count
            };
        }
        
        #endregion
    }
}
