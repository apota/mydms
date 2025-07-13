using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.Services
{
    public interface IPartService
    {
        Task<IEnumerable<PartSummaryDto>> GetAllPartsAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartDetailDto?> GetPartByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PartDetailDto?> GetPartByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> SearchPartsAsync(string searchTerm, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> GetPartsByCategoryAsync(Guid categoryId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> GetPartsByManufacturerAsync(Guid manufacturerId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> FindPartsByVehicleFitmentAsync(int year, string make, string model, string? trim = null, string? engine = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<PartDetailDto> CreatePartAsync(CreatePartDto createPartDto, CancellationToken cancellationToken = default);
        Task<PartDetailDto?> UpdatePartAsync(Guid id, UpdatePartDto updatePartDto, CancellationToken cancellationToken = default);
        Task<bool> DeletePartAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetTotalPartCountAsync(CancellationToken cancellationToken = default);
        Task<bool> PartExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PartExistsByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> GetSimilarPartsAsync(Guid partId, int take = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<PartSummaryDto>> GetSupersessionChainAsync(Guid partId, CancellationToken cancellationToken = default);
    }
}
