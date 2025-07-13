using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface IPartTransactionRepository
    {
        Task<IEnumerable<PartTransaction>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransaction>> GetByPartIdAsync(Guid partId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransaction>> GetByLocationAsync(Guid locationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransaction>> GetByTypeAsync(TransactionType type, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartTransaction>> GetByReferenceAsync(string referenceType, Guid referenceId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartTransaction> AddAsync(PartTransaction transaction, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalValueByDateRangeAndTypeAsync(DateTime startDate, DateTime endDate, TransactionType type, CancellationToken cancellationToken = default);
    }
}
