using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierSummaryDto>> GetAllSuppliersAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<SupplierDetailDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SupplierDetailDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto, CancellationToken cancellationToken = default);
        Task<SupplierDetailDto?> UpdateSupplierAsync(Guid id, UpdateSupplierDto updateSupplierDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> SupplierExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> GetPartsBySupplierId(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    }
}
