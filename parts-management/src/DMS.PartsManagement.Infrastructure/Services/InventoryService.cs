using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using DMS.PartsManagement.Core.Repositories;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IPartInventoryRepository _inventoryRepository;
        private readonly IPartTransactionRepository _transactionRepository;
        private readonly IPartRepository _partRepository;

        public InventoryService(
            IPartInventoryRepository inventoryRepository,
            IPartTransactionRepository transactionRepository,
            IPartRepository partRepository)
        {
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
        }

        public async Task<IEnumerable<PartInventoryDto>> GetAllInventoryAsync()
        {
            var inventories = await _inventoryRepository.GetAllAsync();
            return inventories.Select(MapToDto);
        }

        public async Task<IEnumerable<PartInventoryDto>> GetInventoryByPartIdAsync(int partId)
        {
            var inventories = await _inventoryRepository.GetByPartIdAsync(partId);
            return inventories.Select(MapToDto);
        }

        public async Task<IEnumerable<PartInventoryDto>> GetInventoryByLocationIdAsync(int locationId)
        {
            var inventories = await _inventoryRepository.GetByLocationIdAsync(locationId);
            return inventories.Select(MapToDto);
        }

        public async Task<PartInventoryDto> GetInventoryByPartAndLocationAsync(int partId, int locationId)
        {
            var inventory = await _inventoryRepository.GetByPartAndLocationAsync(partId, locationId);
            return inventory != null ? MapToDto(inventory) : null;
        }

        public async Task<IEnumerable<PartInventoryDto>> GetLowStockInventoryAsync(int threshold = 5)
        {
            var inventories = await _inventoryRepository.GetLowStockAsync(threshold);
            return inventories.Select(MapToDto);
        }

        public async Task<int> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
        {
            var inventory = new PartInventory
            {
                PartId = createInventoryDto.PartId,
                LocationId = createInventoryDto.LocationId,
                BinLocation = createInventoryDto.BinLocation,
                QuantityOnHand = createInventoryDto.Quantity,
                ReorderPoint = createInventoryDto.ReorderPoint,
                ReorderQuantity = createInventoryDto.ReorderQuantity
            };

            var inventoryId = await _inventoryRepository.AddAsync(inventory);

            // Create an adjustment transaction to record the initial inventory
            if (createInventoryDto.Quantity > 0)
            {
                await CreateInventoryTransaction(
                    createInventoryDto.PartId,
                    createInventoryDto.LocationId,
                    createInventoryDto.Quantity,
                    TransactionType.Adjustment,
                    "Initial inventory setup",
                    null
                );
            }

            return inventoryId;
        }

        public async Task UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
            {
                throw new KeyNotFoundException($"Inventory with ID {id} not found.");
            }

            int quantityDifference = updateInventoryDto.Quantity - inventory.QuantityOnHand;

            // Update inventory properties
            inventory.BinLocation = updateInventoryDto.BinLocation;
            inventory.QuantityOnHand = updateInventoryDto.Quantity;
            inventory.ReorderPoint = updateInventoryDto.ReorderPoint;
            inventory.ReorderQuantity = updateInventoryDto.ReorderQuantity;

            await _inventoryRepository.UpdateAsync(inventory);

            // Create an adjustment transaction to record the quantity change
            if (quantityDifference != 0)
            {
                await CreateInventoryTransaction(
                    inventory.PartId,
                    inventory.LocationId,
                    Math.Abs(quantityDifference),
                    quantityDifference > 0 ? TransactionType.Adjustment : TransactionType.Adjustment,
                    $"Manual adjustment: {updateInventoryDto.Notes}",
                    null
                );
            }
        }

        public async Task UpdateQuantityAsync(int partId, int locationId, int quantityChange, string notes)
        {
            await _inventoryRepository.UpdateQuantityAsync(partId, locationId, quantityChange);

            TransactionType transactionType = quantityChange > 0 ? TransactionType.Adjustment : TransactionType.Adjustment;
            await CreateInventoryTransaction(
                partId,
                locationId,
                Math.Abs(quantityChange),
                transactionType,
                notes,
                null
            );
        }

        public async Task<IEnumerable<PartTransactionDto>> GetTransactionHistoryAsync(int partId)
        {
            var transactions = await _transactionRepository.GetByPartIdAsync(partId);
            return transactions.Select(MapToTransactionDto);
        }

        public async Task CreatePartReceiptAsync(int partId, int locationId, int quantity, string poNumber, decimal costPerUnit)
        {
            // Update inventory
            await _inventoryRepository.UpdateQuantityAsync(partId, locationId, quantity);

            // Record transaction
            await CreateInventoryTransaction(
                partId,
                locationId,
                quantity,
                TransactionType.Receipt,
                $"PO: {poNumber}",
                poNumber,
                costPerUnit
            );
        }

        public async Task CreatePartSaleAsync(int partId, int locationId, int quantity, string orderNumber, string customerInfo)
        {
            var inventory = await _inventoryRepository.GetByPartAndLocationAsync(partId, locationId);
            
            if (inventory == null || inventory.QuantityOnHand < quantity)
            {
                throw new InvalidOperationException("Insufficient inventory for this transaction.");
            }

            // Update inventory
            await _inventoryRepository.UpdateQuantityAsync(partId, locationId, -quantity);

            // Record transaction
            await CreateInventoryTransaction(
                partId,
                locationId,
                quantity,
                TransactionType.Sale,
                $"Order: {orderNumber}, Customer: {customerInfo}",
                orderNumber
            );
        }

        public async Task CreatePartTransferAsync(int partId, int sourceLocationId, int destinationLocationId, int quantity, string notes)
        {
            var sourceInventory = await _inventoryRepository.GetByPartAndLocationAsync(partId, sourceLocationId);
            
            if (sourceInventory == null || sourceInventory.QuantityOnHand < quantity)
            {
                throw new InvalidOperationException("Insufficient inventory for this transfer.");
            }

            // Update source inventory
            await _inventoryRepository.UpdateQuantityAsync(partId, sourceLocationId, -quantity);

            // Update destination inventory
            await _inventoryRepository.UpdateQuantityAsync(partId, destinationLocationId, quantity);

            // Record outbound transaction
            await CreateInventoryTransaction(
                partId,
                sourceLocationId,
                quantity,
                TransactionType.Transfer,
                $"Transfer to Location {destinationLocationId}: {notes}",
                null
            );

            // Record inbound transaction
            await CreateInventoryTransaction(
                partId,
                destinationLocationId,
                quantity,
                TransactionType.Receipt,
                $"Transfer from Location {sourceLocationId}: {notes}",
                null
            );
        }

        public async Task CreatePartReturnAsync(int partId, int locationId, int quantity, string returnReason, string customerInfo)
        {
            // Update inventory
            await _inventoryRepository.UpdateQuantityAsync(partId, locationId, quantity);

            // Record transaction
            await CreateInventoryTransaction(
                partId,
                locationId,
                quantity,
                TransactionType.Return,
                $"Return from customer: {customerInfo}, Reason: {returnReason}",
                null
            );
        }

        private async Task CreateInventoryTransaction(
            int partId, 
            int locationId, 
            int quantity,
            TransactionType type,
            string notes,
            string referenceNumber,
            decimal? unitCost = null)
        {
            var part = await _partRepository.GetByIdAsync(partId);
            
            var transaction = new PartTransaction
            {
                PartId = partId,
                LocationId = locationId,
                Quantity = quantity,
                Type = type,
                TransactionDate = DateTime.UtcNow,
                Notes = notes,
                ReferenceNumber = referenceNumber,
                UnitCost = unitCost ?? part?.Pricing?.CostPrice ?? 0
            };

            await _transactionRepository.AddAsync(transaction);
        }

        private PartInventoryDto MapToDto(PartInventory inventory)
        {
            return new PartInventoryDto
            {
                Id = inventory.Id,
                PartId = inventory.PartId,
                PartNumber = inventory.Part?.PartNumber,
                Description = inventory.Part?.Description,
                LocationId = inventory.LocationId,
                LocationName = inventory.Location?.Name,
                BinLocation = inventory.BinLocation,
                QuantityOnHand = inventory.QuantityOnHand,
                ReorderPoint = inventory.ReorderPoint,
                ReorderQuantity = inventory.ReorderQuantity
            };
        }

        private PartTransactionDto MapToTransactionDto(PartTransaction transaction)
        {
            return new PartTransactionDto
            {
                Id = transaction.Id,
                PartId = transaction.PartId,
                PartNumber = transaction.Part?.PartNumber,
                Description = transaction.Part?.Description,
                LocationId = transaction.LocationId,
                LocationName = transaction.Location?.Name,
                Type = transaction.Type.ToString(),
                Quantity = transaction.Quantity,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes,
                ReferenceNumber = transaction.ReferenceNumber,
                UnitCost = transaction.UnitCost
            };
        }
    }
}
