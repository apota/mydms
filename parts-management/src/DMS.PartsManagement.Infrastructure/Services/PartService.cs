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
    public class PartService : IPartService
    {
        private readonly IPartRepository _partRepository;
        private readonly IPartInventoryRepository _inventoryRepository;

        public PartService(
            IPartRepository partRepository,
            IPartInventoryRepository inventoryRepository)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        }

        public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
        {
            var parts = await _partRepository.GetAllAsync();
            return parts.Select(p => MapToDto(p));
        }

        public async Task<PartDetailDto> GetPartByIdAsync(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            if (part == null)
            {
                return null;
            }

            var inventories = await _inventoryRepository.GetByPartIdAsync(id);
            return MapToDetailDto(part, inventories);
        }

        public async Task<PartDetailDto> GetPartByPartNumberAsync(string partNumber)
        {
            var part = await _partRepository.GetByPartNumberAsync(partNumber);
            if (part == null)
            {
                return null;
            }

            var inventories = await _inventoryRepository.GetByPartIdAsync(part.Id);
            return MapToDetailDto(part, inventories);
        }

        public async Task<IEnumerable<PartDto>> SearchPartsAsync(string searchTerm)
        {
            var parts = await _partRepository.SearchAsync(searchTerm);
            return parts.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<PartDto>> GetPartsByFitmentAsync(int year, string make, string model, string subModel = null)
        {
            var parts = await _partRepository.GetByFitmentAsync(year, make, model, subModel);
            return parts.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<PartDto>> GetSupersessionChainAsync(string partNumber)
        {
            var parts = await _partRepository.GetSupersessionChainAsync(partNumber);
            return parts.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<PartDto>> GetCrossSellPartsAsync(int partId)
        {
            var parts = await _partRepository.GetCrossSellPartsAsync(partId);
            return parts.Select(p => MapToDto(p));
        }

        public async Task<int> CreatePartAsync(CreatePartDto createPartDto)
        {
            var part = new Part
            {
                PartNumber = createPartDto.PartNumber,
                Description = createPartDto.Description,
                ManufacturerId = createPartDto.ManufacturerId,
                CategoryId = createPartDto.CategoryId,
                FitmentInfo = createPartDto.FitmentInfo,
                IsActive = createPartDto.IsActive,
                SupersededByPartId = createPartDto.SupersededByPartId,
                Notes = createPartDto.Notes
            };

            // Create part pricing
            part.Pricing = new PartPricing
            {
                CostPrice = createPartDto.CostPrice,
                RetailPrice = createPartDto.RetailPrice,
                WholesalePrice = createPartDto.WholesalePrice ?? (createPartDto.RetailPrice * 0.85m), // Default wholesale is 85% of retail
                CoreCharge = createPartDto.CoreCharge,
                HasCore = createPartDto.CoreCharge > 0
            };

            return await _partRepository.AddAsync(part);
        }

        public async Task UpdatePartAsync(int id, UpdatePartDto updatePartDto)
        {
            var part = await _partRepository.GetByIdAsync(id);
            if (part == null)
            {
                throw new KeyNotFoundException($"Part with ID {id} not found.");
            }

            // Update part properties
            part.Description = updatePartDto.Description;
            part.ManufacturerId = updatePartDto.ManufacturerId;
            part.CategoryId = updatePartDto.CategoryId;
            part.FitmentInfo = updatePartDto.FitmentInfo;
            part.IsActive = updatePartDto.IsActive;
            part.SupersededByPartId = updatePartDto.SupersededByPartId;
            part.Notes = updatePartDto.Notes;

            // Update pricing
            if (part.Pricing == null)
            {
                part.Pricing = new PartPricing();
            }

            part.Pricing.CostPrice = updatePartDto.CostPrice;
            part.Pricing.RetailPrice = updatePartDto.RetailPrice;
            part.Pricing.WholesalePrice = updatePartDto.WholesalePrice ?? (updatePartDto.RetailPrice * 0.85m);
            part.Pricing.CoreCharge = updatePartDto.CoreCharge;
            part.Pricing.HasCore = updatePartDto.CoreCharge > 0;

            await _partRepository.UpdateAsync(part);
        }

        public async Task DeletePartAsync(int id)
        {
            await _partRepository.DeleteAsync(id);
        }

        private PartDto MapToDto(Part part)
        {
            return new PartDto
            {
                Id = part.Id,
                PartNumber = part.PartNumber,
                Description = part.Description,
                ManufacturerId = part.ManufacturerId,
                ManufacturerName = part.Manufacturer?.Name,
                CategoryId = part.CategoryId,
                CategoryName = part.Category?.Name,
                RetailPrice = part.Pricing?.RetailPrice ?? 0,
                WholesalePrice = part.Pricing?.WholesalePrice ?? 0,
                HasCore = part.Pricing?.HasCore ?? false,
                CoreCharge = part.Pricing?.CoreCharge ?? 0,
                IsActive = part.IsActive,
                FitmentInfo = part.FitmentInfo
            };
        }

        private PartDetailDto MapToDetailDto(Part part, IEnumerable<PartInventory> inventories)
        {
            return new PartDetailDto
            {
                Id = part.Id,
                PartNumber = part.PartNumber,
                Description = part.Description,
                ManufacturerId = part.ManufacturerId,
                ManufacturerName = part.Manufacturer?.Name,
                CategoryId = part.CategoryId,
                CategoryName = part.Category?.Name,
                RetailPrice = part.Pricing?.RetailPrice ?? 0,
                WholesalePrice = part.Pricing?.WholesalePrice ?? 0,
                CostPrice = part.Pricing?.CostPrice ?? 0,
                HasCore = part.Pricing?.HasCore ?? false,
                CoreCharge = part.Pricing?.CoreCharge ?? 0,
                IsActive = part.IsActive,
                FitmentInfo = part.FitmentInfo,
                SupersededByPartId = part.SupersededByPartId,
                SupersededByPartNumber = part.SupersededByPart?.PartNumber,
                SupersedesPartId = part.SupersedesPartId,
                SupersedesPartNumber = part.SupersedesPart?.PartNumber,
                Notes = part.Notes,
                Inventories = inventories.Select(i => new PartInventoryDto
                {
                    Id = i.Id,
                    PartId = i.PartId,
                    LocationId = i.LocationId,
                    LocationName = i.Location?.Name,
                    BinLocation = i.BinLocation,
                    QuantityOnHand = i.QuantityOnHand,
                    ReorderPoint = i.ReorderPoint,
                    ReorderQuantity = i.ReorderQuantity
                }).ToList()
            };
        }
    }
}
