using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Repositories
{
    public interface IPartRepository
    {
        Task<IEnumerable<Part>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Part>> SearchAsync(string searchTerm, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Part>> GetByCategoryAsync(Guid categoryId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Part>> GetByManufacturerAsync(Guid manufacturerId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<Part>> FindByVehicleFitmentAsync(int year, string make, string model, string? trim = null, string? engine = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<Part> AddAsync(Part part, CancellationToken cancellationToken = default);
        Task<Part> UpdateAsync(Part part, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Part>> GetPartsBySupplierIdAsync(Guid supplierId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    }
}
