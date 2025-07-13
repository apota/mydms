using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IPartTransactionRepository _transactionRepository;
        private readonly IPartRepository _partRepository;
        private readonly IPartInventoryRepository _inventoryRepository;

        public TransactionService(
            IPartTransactionRepository transactionRepository,
            IPartRepository partRepository,
            IPartInventoryRepository inventoryRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        }

        public async Task<IEnumerable<PartTransactionDto>> GetAllTransactionsAsync(int skip = 0, int take = 50, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<PartTransaction> transactions;
            
            if (startDate.HasValue && endDate.HasValue)
            {
                transactions = await _transactionRepository.GetByDateRangeAsync(startDate.Value, endDate.Value, skip, take, cancellationToken);
            }
            else
            {
                transactions = await _transactionRepository.GetAllAsync(skip, take, cancellationToken);
            }

            return transactions.Select(t => MapToDto(t));
        }

        public async Task<PartTransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
            {
                return null;
            }

            return MapToDto(transaction);
        }

        public async Task<IEnumerable<PartTransactionDto>> GetTransactionsByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.GetByPartIdAsync(partId, skip, take, cancellationToken);
            return transactions.Select(t => MapToDto(t));
        }

        public async Task<IEnumerable<PartTransactionDto>> GetTransactionsByTypeAsync(TransactionType type, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.GetByTypeAsync(type, skip, take, cancellationToken);
            return transactions.Select(t => MapToDto(t));
        }

        public async Task<PartTransactionDto> IssuePartsAsync(IssuePartsDto issuePartsDto, CancellationToken cancellationToken = default)
        {
            // Verify part exists
            var part = await _partRepository.GetByIdAsync(issuePartsDto.PartId, cancellationToken);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {issuePartsDto.PartId} not found.");
            }

            // Verify source location has enough inventory
            var inventory = await _inventoryRepository.GetByPartIdAndLocationAsync(issuePartsDto.PartId, issuePartsDto.LocationId, cancellationToken);
            if (inventory == null || inventory.QuantityOnHand < issuePartsDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient inventory for part {part.PartNumber} at the specified location.");
            }

            // Create the transaction
            var transaction = new PartTransaction
            {
                Id = Guid.NewGuid(),
                TransactionType = TransactionType.Issue,
                PartId = issuePartsDto.PartId,
                Quantity = issuePartsDto.Quantity,
                SourceLocationId = issuePartsDto.LocationId,
                ReferenceType = issuePartsDto.ReferenceType,
                ReferenceId = issuePartsDto.ReferenceId,
                UserId = Guid.Empty, // In a real system, this would be the current user's ID
                Notes = issuePartsDto.Notes ?? string.Empty,
                UnitCost = part.Pricing?.CostPrice ?? 0,
                ExtendedCost = (part.Pricing?.CostPrice ?? 0) * issuePartsDto.Quantity,
                UnitPrice = issuePartsDto.UnitPrice ?? (part.Pricing?.RetailPrice ?? 0),
                ExtendedPrice = (issuePartsDto.UnitPrice ?? (part.Pricing?.RetailPrice ?? 0)) * issuePartsDto.Quantity,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Update inventory
            inventory.QuantityOnHand -= issuePartsDto.Quantity;
            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // Save transaction
            var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
            return MapToDto(createdTransaction);
        }

        public async Task<PartTransactionDto> ReturnPartsAsync(ReturnPartsDto returnPartsDto, CancellationToken cancellationToken = default)
        {
            // Verify part exists
            var part = await _partRepository.GetByIdAsync(returnPartsDto.PartId, cancellationToken);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {returnPartsDto.PartId} not found.");
            }

            // Check if inventory exists for this location, create if not
            var inventory = await _inventoryRepository.GetByPartIdAndLocationAsync(returnPartsDto.PartId, returnPartsDto.LocationId, cancellationToken);
            if (inventory == null)
            {
                inventory = new PartInventory
                {
                    Id = Guid.NewGuid(),
                    PartId = returnPartsDto.PartId,
                    LocationId = returnPartsDto.LocationId,
                    QuantityOnHand = 0,
                    ReorderPoint = 0,
                    ReorderQuantity = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryRepository.AddAsync(inventory, cancellationToken);
            }

            // Create the transaction
            var transaction = new PartTransaction
            {
                Id = Guid.NewGuid(),
                TransactionType = TransactionType.Return,
                PartId = returnPartsDto.PartId,
                Quantity = returnPartsDto.Quantity,
                DestinationLocationId = returnPartsDto.LocationId,
                ReferenceType = returnPartsDto.ReferenceType,
                ReferenceId = returnPartsDto.ReferenceId,
                UserId = Guid.Empty, // In a real system, this would be the current user's ID
                Notes = returnPartsDto.Notes ?? string.Empty,
                UnitCost = part.Pricing?.CostPrice ?? 0,
                ExtendedCost = (part.Pricing?.CostPrice ?? 0) * returnPartsDto.Quantity,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Update inventory
            inventory.QuantityOnHand += returnPartsDto.Quantity;
            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // Save transaction
            var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
            return MapToDto(createdTransaction);
        }

        public async Task<PartTransactionDto> AdjustInventoryAsync(AdjustInventoryDto adjustInventoryDto, CancellationToken cancellationToken = default)
        {
            // Verify part exists
            var part = await _partRepository.GetByIdAsync(adjustInventoryDto.PartId, cancellationToken);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {adjustInventoryDto.PartId} not found.");
            }

            // Check if inventory exists for this location, create if not
            var inventory = await _inventoryRepository.GetByPartIdAndLocationAsync(adjustInventoryDto.PartId, adjustInventoryDto.LocationId, cancellationToken);
            if (inventory == null)
            {
                if (adjustInventoryDto.QuantityAdjustment < 0)
                {
                    throw new InvalidOperationException($"Cannot adjust inventory by negative amount when no inventory exists.");
                }

                inventory = new PartInventory
                {
                    Id = Guid.NewGuid(),
                    PartId = adjustInventoryDto.PartId,
                    LocationId = adjustInventoryDto.LocationId,
                    QuantityOnHand = 0,
                    ReorderPoint = 0,
                    ReorderQuantity = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryRepository.AddAsync(inventory, cancellationToken);
            }
            else if (inventory.QuantityOnHand + adjustInventoryDto.QuantityAdjustment < 0)
            {
                // Ensure we don't adjust below zero
                throw new InvalidOperationException($"Adjustment would result in negative inventory for part {part.PartNumber}.");
            }

            // Create the transaction
            var transaction = new PartTransaction
            {
                Id = Guid.NewGuid(),
                TransactionType = TransactionType.Adjustment,
                PartId = adjustInventoryDto.PartId,
                Quantity = Math.Abs(adjustInventoryDto.QuantityAdjustment), // Store absolute quantity
                SourceLocationId = adjustInventoryDto.QuantityAdjustment < 0 ? adjustInventoryDto.LocationId : null,
                DestinationLocationId = adjustInventoryDto.QuantityAdjustment > 0 ? adjustInventoryDto.LocationId : null,
                ReferenceType = adjustInventoryDto.AdjustmentReason,
                UserId = Guid.Empty, // In a real system, this would be the current user's ID
                Notes = adjustInventoryDto.Notes ?? string.Empty,
                UnitCost = part.Pricing?.CostPrice ?? 0,
                ExtendedCost = (part.Pricing?.CostPrice ?? 0) * Math.Abs(adjustInventoryDto.QuantityAdjustment),
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Update inventory
            inventory.QuantityOnHand += adjustInventoryDto.QuantityAdjustment;
            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            // Save transaction
            var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
            return MapToDto(createdTransaction);
        }

        public async Task<PartTransactionDto> TransferPartsAsync(TransferPartsDto transferPartsDto, CancellationToken cancellationToken = default)
        {
            // Verify part exists
            var part = await _partRepository.GetByIdAsync(transferPartsDto.PartId, cancellationToken);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {transferPartsDto.PartId} not found.");
            }

            // Verify source location has enough inventory
            var sourceInventory = await _inventoryRepository.GetByPartIdAndLocationAsync(transferPartsDto.PartId, transferPartsDto.SourceLocationId, cancellationToken);
            if (sourceInventory == null || sourceInventory.QuantityOnHand < transferPartsDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient inventory for part {part.PartNumber} at the source location.");
            }

            // Check or create destination inventory
            var destInventory = await _inventoryRepository.GetByPartIdAndLocationAsync(transferPartsDto.PartId, transferPartsDto.DestinationLocationId, cancellationToken);
            if (destInventory == null)
            {
                destInventory = new PartInventory
                {
                    Id = Guid.NewGuid(),
                    PartId = transferPartsDto.PartId,
                    LocationId = transferPartsDto.DestinationLocationId,
                    QuantityOnHand = 0,
                    ReorderPoint = 0,
                    ReorderQuantity = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _inventoryRepository.AddAsync(destInventory, cancellationToken);
            }

            // Create the transaction
            var transaction = new PartTransaction
            {
                Id = Guid.NewGuid(),
                TransactionType = TransactionType.Transfer,
                PartId = transferPartsDto.PartId,
                Quantity = transferPartsDto.Quantity,
                SourceLocationId = transferPartsDto.SourceLocationId,
                DestinationLocationId = transferPartsDto.DestinationLocationId,
                UserId = Guid.Empty, // In a real system, this would be the current user's ID
                Notes = transferPartsDto.Notes ?? string.Empty,
                UnitCost = part.Pricing?.CostPrice ?? 0,
                ExtendedCost = (part.Pricing?.CostPrice ?? 0) * transferPartsDto.Quantity,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Update inventory
            sourceInventory.QuantityOnHand -= transferPartsDto.Quantity;
            await _inventoryRepository.UpdateAsync(sourceInventory, cancellationToken);

            destInventory.QuantityOnHand += transferPartsDto.Quantity;
            await _inventoryRepository.UpdateAsync(destInventory, cancellationToken);

            // Save transaction
            var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
            return MapToDto(createdTransaction);
        }

        public async Task<IEnumerable<PartTransactionSummaryDto>> GetTransactionHistoryAsync(Guid partId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.GetByPartIdAsync(partId, 0, 1000, cancellationToken);
            
            return transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Select(t => new PartTransactionSummaryDto
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    LocationName = DetermineLocationName(t),
                    ReferenceNumber = t.ReferenceType,
                    TransactionDate = t.TransactionDate
                });
        }

        private string DetermineLocationName(PartTransaction transaction)
        {
            // In a real system, we'd look up the location names
            // For now, just return a placeholder based on the transaction type
            switch (transaction.TransactionType)
            {
                case TransactionType.Issue:
                    return transaction.SourceLocation?.Name ?? "Unknown Source";
                case TransactionType.Return:
                    return transaction.DestinationLocation?.Name ?? "Unknown Destination";
                case TransactionType.Transfer:
                    return $"{transaction.SourceLocation?.Name ?? "Unknown"} → {transaction.DestinationLocation?.Name ?? "Unknown"}";
                default:
                    return transaction.SourceLocation?.Name ?? transaction.DestinationLocation?.Name ?? "Unknown";
            }
        }

        private PartTransactionDto MapToDto(PartTransaction transaction)
        {
            return new PartTransactionDto
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType.ToString(),
                PartId = transaction.PartId,
                PartNumber = transaction.Part?.PartNumber ?? string.Empty,
                PartDescription = transaction.Part?.Description ?? string.Empty,
                Quantity = transaction.Quantity,
                SourceLocationId = transaction.SourceLocationId,
                SourceLocationName = transaction.SourceLocation?.Name ?? string.Empty,
                DestinationLocationId = transaction.DestinationLocationId,
                DestinationLocationName = transaction.DestinationLocation?.Name ?? string.Empty,
                ReferenceType = transaction.ReferenceType ?? string.Empty,
                ReferenceId = transaction.ReferenceId,
                ReferenceNumber = transaction.ReferenceType ?? string.Empty, // In a real system, we'd look up the reference number
                UserId = transaction.UserId ?? Guid.Empty,
                UserName = string.Empty, // In a real system, we'd look up the user name
                Notes = transaction.Notes,
                UnitCost = transaction.UnitCost,
                ExtendedCost = transaction.ExtendedCost,
                UnitPrice = transaction.UnitPrice,
                ExtendedPrice = transaction.ExtendedPrice,
                TransactionDate = transaction.TransactionDate,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
