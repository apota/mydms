using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.Models;

namespace DMS.SalesManagement.Core.Repositories
{
    public interface ICommissionRepository
    {
        Task<Commission> GetByIdAsync(Guid id);
        Task<IEnumerable<Commission>> GetByDealIdAsync(Guid dealId);
        Task<IEnumerable<Commission>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Commission>> GetByStatusAsync(CommissionStatus status);
        Task<IEnumerable<Commission>> CalculateCommissionsForDealAsync(Guid dealId);
        Task AddAsync(Commission commission);
        Task UpdateAsync(Commission commission);
        Task DeleteAsync(Guid id);
    }
}
