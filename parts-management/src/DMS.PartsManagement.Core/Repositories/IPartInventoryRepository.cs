using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface IPartInventoryRepository
    {
        Task<IEnumerable<PartInventory>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartInventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartInventory?> GetByPartIdAndLocationAsync(Guid partId, Guid locationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventory>> GetByPartIdAsync(Guid partId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventory>> GetByLocationIdAsync(Guid locationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartInventory>> GetBelowReorderPointAsync(Guid locationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartInventory> AddAsync(PartInventory inventory, CancellationToken cancellationToken = default);
        Task<PartInventory> UpdateAsync(PartInventory inventory, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> CountByMovementClassAsync(Guid locationId, MovementClass movementClass, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
