using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;

namespace DMS.PartsManagement.Core.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<PartInventoryDto>> GetAllInventoriesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartInventoryDto?> GetInventoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartInventoryDto?> GetInventoryByPartAndLocationAsync(Guid partId, Guid locationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventoryDto>> GetInventoriesByPartIdAsync(Guid partId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventoryDto>> GetInventoriesByLocationIdAsync(Guid locationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventoryDto>> GetBelowReorderPointAsync(Guid locationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartInventoryDto> CreateInventoryAsync(CreatePartInventoryDto createDto, CancellationToken cancellationToken = default);
        Task<PartInventoryDto?> UpdateInventoryAsync(Guid id, UpdatePartInventoryDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteInventoryAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartInventoryDto?> ProcessInventoryCountAsync(InventoryCountDto countDto, CancellationToken cancellationToken = default);
        Task<PartInventoryDto?> ProcessInventoryAdjustmentAsync(InventoryAdjustmentDto adjustmentDto, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventorySummaryDto>> GetInventorySummaryByPartIdAsync(Guid partId, CancellationToken cancellationToken = default);
        Task<LocationInventorySummaryDto> GetLocationInventorySummaryAsync(Guid locationId, CancellationToken cancellationToken = default);
        Task<int> GetTotalInventoryCountAsync(CancellationToken cancellationToken = default);
        Task<bool> InventoryExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
