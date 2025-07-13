using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface IPartOrderRepository
    {
        Task<IEnumerable<PartOrder>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrder>> GetBySupplierAsync(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrder>> GetByStatusAsync(OrderStatus status, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartOrder> AddAsync(PartOrder order, CancellationToken cancellationToken = default);
        Task<PartOrder> UpdateAsync(PartOrder order, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalValueByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
