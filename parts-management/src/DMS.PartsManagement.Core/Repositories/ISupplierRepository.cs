using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> GetByNameAsync(string name, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> GetByTypeAsync(SupplierType type, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> GetActiveAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
