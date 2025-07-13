using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface IPartCoreTrackingRepository
    {
        Task<IEnumerable<PartCoreTracking>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartCoreTracking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCoreTracking>> GetByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCoreTracking>> GetByStatusAsync(CoreStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCoreTracking>> GetBySoldDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartCoreTracking>> GetByReturnedDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartCoreTracking> AddAsync(PartCoreTracking coreTracking, CancellationToken cancellationToken = default);
        Task<PartCoreTracking> UpdateAsync(PartCoreTracking coreTracking, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalCoreValueByStatusAsync(CoreStatus status, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
