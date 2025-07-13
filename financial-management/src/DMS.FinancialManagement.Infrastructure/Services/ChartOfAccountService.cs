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
    public class ChartOfAccountService : IChartOfAccountService
    {
        private readonly IChartOfAccountRepository _accountRepository;

        public ChartOfAccountService(IChartOfAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetAllAccountsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var accounts = await _accountRepository.GetAllAsync(skip, take);
            return accounts.Select(MapToDto);
        }

        public async Task<ChartOfAccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetAccountsByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
        {
            var accounts = await _accountRepository.GetByTypeAsync(accountType, cancellationToken);
            return accounts.Select(MapToDto);
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetAccountsByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            var accounts = await _accountRepository.GetByParentIdAsync(parentId, cancellationToken);
            return accounts.Select(MapToDto);
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetAccountHierarchyAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var accountHierarchy = await _accountRepository.GetHierarchyAsync(includeInactive, cancellationToken);
            return accountHierarchy.Select(MapToHierarchyDto);
        }

        public async Task<ChartOfAccountDto> CreateAccountAsync(ChartOfAccountCreateDto accountDto, CancellationToken cancellationToken = default)
        {
            // Check if account code already exists
            var existingAccount = await _accountRepository.GetByAccountCodeAsync(accountDto.AccountCode, cancellationToken);
            if (existingAccount != null)
            {
                throw new InvalidOperationException($"Account with code {accountDto.AccountCode} already exists");
            }

            // Validate parent account if specified
            if (accountDto.ParentAccountId.HasValue)
            {
                var parentAccount = await _accountRepository.GetByIdAsync(accountDto.ParentAccountId.Value);
                if (parentAccount == null)
                {
                    throw new InvalidOperationException($"Parent account with ID {accountDto.ParentAccountId} not found");
                }
            }

            // Create new account
            var account = new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountCode = accountDto.AccountCode,
                AccountName = accountDto.AccountName,
                AccountType = accountDto.AccountType,
                ParentAccountId = accountDto.ParentAccountId,
                Description = accountDto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdAccount = await _accountRepository.AddAsync(account);
            return MapToDto(createdAccount);
        }

        public async Task<ChartOfAccountDto?> UpdateAccountAsync(Guid id, ChartOfAccountUpdateDto accountDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                return null;
            }

            // Validate parent account if specified
            if (accountDto.ParentAccountId.HasValue)
            {
                // Cannot set parent to self or descendant
                if (accountDto.ParentAccountId == id)
                {
                    throw new InvalidOperationException("Cannot set an account as its own parent");
                }

                var parentAccount = await _accountRepository.GetByIdAsync(accountDto.ParentAccountId.Value);
                if (parentAccount == null)
                {
                    throw new InvalidOperationException($"Parent account with ID {accountDto.ParentAccountId} not found");
                }

                // Check if the new parent is a descendant of this account (would create a cycle)
                bool isDescendant = await IsDescendantAccountAsync(id, accountDto.ParentAccountId.Value, cancellationToken);
                if (isDescendant)
                {
                    throw new InvalidOperationException("Cannot set a descendant account as parent (would create a cycle)");
                }
            }

            // Update account properties
            account.AccountName = accountDto.AccountName;
            account.AccountType = accountDto.AccountType;
            account.ParentAccountId = accountDto.ParentAccountId;
            account.Description = accountDto.Description;
            account.IsActive = accountDto.IsActive;
            account.UpdatedAt = DateTime.UtcNow;

            var updatedAccount = await _accountRepository.UpdateAsync(account);
            return MapToDto(updatedAccount);
        }

        public async Task<bool> DeactivateAccountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                return false;
            }

            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;

            await _accountRepository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> ActivateAccountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
            {
                return false;
            }

            account.IsActive = true;
            account.UpdatedAt = DateTime.UtcNow;

            await _accountRepository.UpdateAsync(account);
            return true;
        }

        private ChartOfAccountDto MapToDto(ChartOfAccount account)
        {
            return new ChartOfAccountDto
            {
                Id = account.Id,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                ParentAccountId = account.ParentAccountId,
                ParentAccountName = account.ParentAccount?.AccountName ?? string.Empty,
                Description = account.Description,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt,
                ChildAccounts = new List<ChartOfAccountDto>() // Don't include child accounts in basic mapping
            };
        }

        private ChartOfAccountDto MapToHierarchyDto(ChartOfAccount account)
        {
            return new ChartOfAccountDto
            {
                Id = account.Id,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                ParentAccountId = account.ParentAccountId,
                ParentAccountName = account.ParentAccount?.AccountName ?? string.Empty,
                Description = account.Description,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt,
                ChildAccounts = account.ChildAccounts.Select(MapToHierarchyDto).ToList()
            };
        }
        
        private async Task<bool> IsDescendantAccountAsync(Guid ancestorId, Guid descendantId, CancellationToken cancellationToken)
        {
            var descendant = await _accountRepository.GetByIdAsync(descendantId);
            if (descendant == null)
            {
                return false;
            }
            
            // Check if this is a direct child of the ancestor
            if (descendant.ParentAccountId == ancestorId)
            {
                return true;
            }
            
            // If not, check if its parent is a descendant of the ancestor
            if (descendant.ParentAccountId.HasValue)
            {
                return await IsDescendantAccountAsync(ancestorId, descendant.ParentAccountId.Value, cancellationToken);
            }
            
            return false;
        }
    }
}
