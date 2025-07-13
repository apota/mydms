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
    /// Service implementation for managing journal entries
    /// </summary>
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;

        public JournalEntryService(
            IJournalEntryRepository journalEntryRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository)
        {
            _journalEntryRepository = journalEntryRepository;
            _chartOfAccountRepository = chartOfAccountRepository;
            _financialPeriodRepository = financialPeriodRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntrySummaryDto>> GetAllEntriesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var entries = await _journalEntryRepository.GetAllAsync(skip, take, cancellationToken);
            return entries.Select(MapToSummaryDto);
        }

        /// <inheritdoc />
        public async Task<JournalEntryDto?> GetEntryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            return entry == null ? null : MapToDto(entry);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntrySummaryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var entries = await _journalEntryRepository.GetByDateRangeAsync(startDate, endDate, skip, take, cancellationToken);
            return entries.Select(MapToSummaryDto);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<JournalEntrySummaryDto>> GetEntriesByFinancialPeriodAsync(Guid financialPeriodId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var entries = await _journalEntryRepository.GetByFinancialPeriodIdAsync(financialPeriodId, skip, take, cancellationToken);
            return entries.Select(MapToSummaryDto);
        }

        /// <inheritdoc />
        public async Task<JournalEntryDto> CreateEntryAsync(JournalEntryCreateDto entryDto, CancellationToken cancellationToken = default)
        {
            // Validate entry has balanced debits and credits
            var totalDebits = entryDto.LineItems.Sum(li => li.DebitAmount);
            var totalCredits = entryDto.LineItems.Sum(li => li.CreditAmount);
            
            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException("Journal entry must have balanced debits and credits");
            }
            
            // Validate all accounts exist
            var accountIds = entryDto.LineItems.Select(li => li.AccountId).Distinct().ToList();
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
            
            // Validate financial period if specified
            if (entryDto.FinancialPeriodId.HasValue)
            {
                var period = await _financialPeriodRepository.GetByIdAsync(entryDto.FinancialPeriodId.Value, cancellationToken);
                if (period == null)
                {
                    throw new InvalidOperationException($"Financial period with ID {entryDto.FinancialPeriodId.Value} does not exist");
                }
                
                if (period.IsClosed)
                {
                    throw new InvalidOperationException($"Financial period {period.Name} is closed");
                }
            }
            
            // Generate entry number (format: JE-YYYYMMDD-XXXX where XXXX is a sequential number)
            string entryNumber = $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            
            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryNumber = entryNumber,
                EntryDate = entryDto.EntryDate,
                PostingDate = DateTime.MinValue,  // Will be set when posted
                Description = entryDto.Description,
                Reference = entryDto.Reference,
                IsPosted = false,
                IsRecurring = entryDto.IsRecurring,
                FinancialPeriodId = entryDto.FinancialPeriodId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"  // This should be replaced with actual user info from auth context
            };
            
            // Create journal line items
            foreach (var lineItemDto in entryDto.LineItems)
            {
                var lineItem = new JournalLineItem
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = lineItemDto.AccountId,
                    Description = lineItemDto.Description,
                    DebitAmount = lineItemDto.DebitAmount,
                    CreditAmount = lineItemDto.CreditAmount,
                    DepartmentId = lineItemDto.DepartmentId,
                    CostCenterId = lineItemDto.CostCenterId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"  // This should be replaced with actual user info from auth context
                };
                
                journalEntry.LineItems.Add(lineItem);
            }
            
            // Save to database
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
            
            // Load full entry with navigation properties for mapping
            var savedEntry = await _journalEntryRepository.GetByIdAsync(journalEntry.Id, cancellationToken);
            if (savedEntry == null)
            {
                throw new InvalidOperationException("Failed to create journal entry");
            }
            
            return MapToDto(savedEntry);
        }

        /// <inheritdoc />
        public async Task<JournalEntryDto?> UpdateEntryAsync(Guid id, JournalEntryUpdateDto entryDto, CancellationToken cancellationToken = default)
        {
            var existingEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (existingEntry == null)
            {
                return null;
            }
            
            // Cannot update a posted entry
            if (existingEntry.IsPosted)
            {
                throw new InvalidOperationException("Cannot update a posted journal entry");
            }
            
            // Validate entry has balanced debits and credits
            var totalDebits = entryDto.LineItems.Sum(li => li.DebitAmount);
            var totalCredits = entryDto.LineItems.Sum(li => li.CreditAmount);
            
            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException("Journal entry must have balanced debits and credits");
            }
            
            // Validate all accounts exist
            var accountIds = entryDto.LineItems.Select(li => li.AccountId).Distinct().ToList();
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
            
            // Validate financial period if specified
            if (entryDto.FinancialPeriodId.HasValue)
            {
                var period = await _financialPeriodRepository.GetByIdAsync(entryDto.FinancialPeriodId.Value, cancellationToken);
                if (period == null)
                {
                    throw new InvalidOperationException($"Financial period with ID {entryDto.FinancialPeriodId.Value} does not exist");
                }
                
                if (period.IsClosed)
                {
                    throw new InvalidOperationException($"Financial period {period.Name} is closed");
                }
            }
            
            // Update journal entry properties
            existingEntry.EntryDate = entryDto.EntryDate;
            existingEntry.Description = entryDto.Description;
            existingEntry.Reference = entryDto.Reference;
            existingEntry.IsRecurring = entryDto.IsRecurring;
            existingEntry.FinancialPeriodId = entryDto.FinancialPeriodId;
            existingEntry.UpdatedAt = DateTime.UtcNow;
            existingEntry.UpdatedBy = "system";  // This should be replaced with actual user info from auth context
            
            // Handle line items
            var existingLineItems = existingEntry.LineItems.ToList();
            var updatedLineItems = new List<JournalLineItem>();
            
            // Process updated line items
            foreach (var lineItemDto in entryDto.LineItems)
            {
                if (lineItemDto.Id.HasValue)
                {
                    // Update existing line item
                    var existingLineItem = existingLineItems.FirstOrDefault(li => li.Id == lineItemDto.Id.Value);
                    if (existingLineItem != null)
                    {
                        existingLineItem.AccountId = lineItemDto.AccountId;
                        existingLineItem.Description = lineItemDto.Description;
                        existingLineItem.DebitAmount = lineItemDto.DebitAmount;
                        existingLineItem.CreditAmount = lineItemDto.CreditAmount;
                        existingLineItem.DepartmentId = lineItemDto.DepartmentId;
                        existingLineItem.CostCenterId = lineItemDto.CostCenterId;
                        existingLineItem.UpdatedAt = DateTime.UtcNow;
                        existingLineItem.UpdatedBy = "system";  // This should be replaced with actual user info from auth context
                        
                        updatedLineItems.Add(existingLineItem);
                    }
                }
                else
                {
                    // Add new line item
                    var newLineItem = new JournalLineItem
                    {
                        Id = Guid.NewGuid(),
                        JournalEntryId = existingEntry.Id,
                        AccountId = lineItemDto.AccountId,
                        Description = lineItemDto.Description,
                        DebitAmount = lineItemDto.DebitAmount,
                        CreditAmount = lineItemDto.CreditAmount,
                        DepartmentId = lineItemDto.DepartmentId,
                        CostCenterId = lineItemDto.CostCenterId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"  // This should be replaced with actual user info from auth context
                    };
                    
                    updatedLineItems.Add(newLineItem);
                }
            }
            
            // Remove line items not in the updated list
            var lineItemsToRemove = existingLineItems
                .Where(eli => !entryDto.LineItems.Any(li => li.Id.HasValue && li.Id.Value == eli.Id))
                .ToList();
            
            foreach (var lineItem in lineItemsToRemove)
            {
                existingEntry.LineItems.Remove(lineItem);
            }
            
            // Add new line items
            var lineItemsToAdd = updatedLineItems.Where(uli => !existingLineItems.Any(eli => eli.Id == uli.Id)).ToList();
            foreach (var lineItem in lineItemsToAdd)
            {
                existingEntry.LineItems.Add(lineItem);
            }
            
            // Save changes
            await _journalEntryRepository.UpdateAsync(existingEntry, cancellationToken);
            
            // Get updated entry with all navigation properties
            var updatedEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (updatedEntry == null)
            {
                throw new InvalidOperationException("Failed to update journal entry");
            }
            
            return MapToDto(updatedEntry);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (entry == null)
            {
                return false;
            }
            
            // Cannot delete a posted entry
            if (entry.IsPosted)
            {
                throw new InvalidOperationException("Cannot delete a posted journal entry");
            }
            
            await _journalEntryRepository.DeleteAsync(entry, cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<JournalEntryDto?> PostEntryAsync(Guid id, JournalEntryPostDto postDto, CancellationToken cancellationToken = default)
        {
            // Validate the entry exists
            var entry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (entry == null)
            {
                return null;
            }
            
            // Validate it is not already posted
            if (entry.IsPosted)
            {
                throw new InvalidOperationException("Journal entry is already posted");
            }
            
            // Validate entry has line items
            if (!entry.LineItems.Any())
            {
                throw new InvalidOperationException("Cannot post a journal entry without line items");
            }
            
            // Validate entry is balanced
            var totalDebits = entry.LineItems.Sum(li => li.DebitAmount);
            var totalCredits = entry.LineItems.Sum(li => li.CreditAmount);
            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException("Cannot post an unbalanced journal entry");
            }
            
            // Post the entry
            var postedEntry = await _journalEntryRepository.PostJournalEntryAsync(
                id, 
                postDto.PostingDate, 
                "system", // This should be replaced with actual user info from auth context
                cancellationToken);
                
            if (postedEntry == null)
            {
                return null;
            }
            
            return MapToDto(postedEntry);
        }

        /// <inheritdoc />
        public async Task<JournalEntryDto?> ReverseEntryAsync(Guid id, JournalEntryReverseDto reverseDto, CancellationToken cancellationToken = default)
        {
            // Validate the entry exists
            var entry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (entry == null)
            {
                return null;
            }
            
            // Validate it is posted
            if (!entry.IsPosted)
            {
                throw new InvalidOperationException("Cannot reverse an unposted journal entry");
            }
            
            // Create reversal entry
            var reversalEntry = await _journalEntryRepository.ReverseJournalEntryAsync(
                id, 
                reverseDto.ReversalDate, 
                reverseDto.Description,
                "system", // This should be replaced with actual user info from auth context
                cancellationToken);
                
            if (reversalEntry == null)
            {
                return null;
            }
            
            return MapToDto(reversalEntry);
        }
        
        #region Helper Methods
        
        private JournalEntryDto MapToDto(JournalEntry entry)
        {
            return new JournalEntryDto
            {
                Id = entry.Id,
                EntryNumber = entry.EntryNumber,
                EntryDate = entry.EntryDate,
                PostingDate = entry.PostingDate,
                Description = entry.Description,
                Reference = entry.Reference,
                IsPosted = entry.IsPosted,
                IsRecurring = entry.IsRecurring,
                FinancialPeriodId = entry.FinancialPeriodId,
                FinancialPeriodName = entry.FinancialPeriod?.Name ?? string.Empty,
                LineItems = entry.LineItems.Select(li => new JournalLineItemDto
                {
                    Id = li.Id,
                    AccountId = li.AccountId,
                    AccountCode = li.Account?.AccountCode ?? string.Empty,
                    AccountName = li.Account?.Name ?? string.Empty,
                    Description = li.Description,
                    DebitAmount = li.DebitAmount,
                    CreditAmount = li.CreditAmount,
                    DepartmentId = li.DepartmentId,
                    DepartmentName = null,  // Would need a DepartmentRepository to populate this
                    CostCenterId = li.CostCenterId,
                    CostCenterName = null  // Would need a CostCenterRepository to populate this
                }).ToList(),
                TotalDebits = entry.LineItems.Sum(li => li.DebitAmount),
                TotalCredits = entry.LineItems.Sum(li => li.CreditAmount),
                CreatedAt = entry.CreatedAt,
                CreatedBy = entry.CreatedBy,
                UpdatedAt = entry.UpdatedAt
            };
        }
        
        private JournalEntrySummaryDto MapToSummaryDto(JournalEntry entry)
        {
            decimal totalAmount = entry.LineItems.Sum(li => li.DebitAmount); // Either debits or credits will work as they should be equal
            
            return new JournalEntrySummaryDto
            {
                Id = entry.Id,
                EntryNumber = entry.EntryNumber,
                EntryDate = entry.EntryDate,
                Description = entry.Description,
                Reference = entry.Reference,
                IsPosted = entry.IsPosted,
                TotalAmount = totalAmount
            };
        }
        
        #endregion
    }
}
